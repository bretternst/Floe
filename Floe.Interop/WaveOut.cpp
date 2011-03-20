#include "Stdafx.h"
#include "WaveOut.h"

namespace Floe
{
	namespace Interop
	{
		WaveOut::WaveOut(Stream ^stream, WaveFormat ^format, int bufferSize)
		{
			m_stream = stream;
			m_format = format;
			m_bufferSize = bufferSize;
			m_stop = gcnew AutoResetEvent(false);
			m_volume = 1.0f;
		}

		void WaveOut::Start()
		{
			m_task = Task::Factory->StartNew(gcnew System::Action(this, &WaveOut::Loop));
		}

		void WaveOut::Pause()
		{
			waveOutPause(m_wavHandle);
		}

		void WaveOut::Resume()
		{
			waveOutRestart(m_wavHandle);
		}

		void WaveOut::Close()
		{
			m_stop->Set();
		}

		void WaveOut::Loop()
		{
			using namespace System::Runtime::InteropServices;

			DWORD taskIndex = 0;
			HANDLE taskHandle = AvSetMmThreadCharacteristics(TEXT("Audio"), &taskIndex);
			array<System::Byte> ^bytes = gcnew array<System::Byte>(m_bufferSize);
			AutoResetEvent ^bufEvent = gcnew AutoResetEvent(false);
			array<WaitHandle^> ^handles = { m_stop, bufEvent };
			int bufIdx = 0;
			HWAVEOUT wavHandle;
			WAVEHDR hdr[2];

			try
			{
				ThrowOnFailure(waveOutOpen(&wavHandle, WAVE_MAPPER, m_format->Data, (int)bufEvent->Handle, 0, CALLBACK_EVENT));
				m_wavHandle = wavHandle;
				for(int i = 0; i < 2; i++)
				{
					hdr[i].lpData = (LPSTR)new BYTE[m_bufferSize];
					hdr[i].dwBufferLength = m_bufferSize;
					hdr[i].dwFlags = 0;
					ThrowOnFailure(waveOutPrepareHeader(wavHandle, &hdr[i], sizeof(WAVEHDR)));
				}
				this->Volume = m_volume;

				while(true)
				{
					switch(WaitHandle::WaitAny(handles))
					{
					case 0:
						return;
					case 1:
						bool eos = true;
						for(int i = 0; i < 2; i++)
						{
							if((hdr[i].dwFlags & WHDR_INQUEUE) == 0)
							{
								hdr[i].dwBufferLength = (int)m_stream->Read(bytes, 0, m_bufferSize);
								Marshal::Copy(bytes, 0, (IntPtr)hdr[i].lpData, hdr[i].dwBufferLength);
								ThrowOnFailure(waveOutWrite(wavHandle, &hdr[i], sizeof(WAVEHDR)));
								if(hdr[i].dwBufferLength > 0)
								{
									eos = false;
								}
							}
						}
						if(eos)
						{
							this->EndOfStream(this, System::EventArgs::Empty);
						}
					}
				}
			}
			finally
			{
				ThrowOnFailure(waveOutReset(wavHandle));
				for(int i = 0; i < 2; i++)
				{
					ThrowOnFailure(waveOutUnprepareHeader(wavHandle, &hdr[i], sizeof(WAVEHDR)));
					delete[] (BYTE*)hdr[i].lpData;
				}
				ThrowOnFailure(waveOutClose(wavHandle));
				AvRevertMmThreadCharacteristics(taskHandle);
			}
		}

		WaveOut::~WaveOut()
		{
			this->Close();
			if(m_task != nullptr)
			{
				try
				{
					m_task->Wait();
				}
				catch(System::AggregateException^)
				{
				}
				m_task = nullptr;
			}
		}

		WaveOut::!WaveOut()
		{
			this->~WaveOut();
		}
	}
}
