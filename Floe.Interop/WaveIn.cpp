#include "Stdafx.h"
#include "WaveIn.h"

namespace Floe
{
	namespace Interop
	{
		WaveIn::WaveIn(Stream ^stream, WaveFormat ^format, int bufferSize)
		{
			m_stream = stream;
			m_format = format;
			m_bufferSize = bufferSize;
			m_stop = gcnew AutoResetEvent(false);
		}

		void WaveIn::Start()
		{
			m_task = Task::Factory->StartNew(gcnew System::Action(this, &WaveIn::Loop));
		}

		void WaveIn::Pause()
		{
			waveInStop(m_wavHandle);
		}

		void WaveIn::Resume()
		{
			waveInStart(m_wavHandle);
		}

		void WaveIn::Close()
		{
			m_stop->Set();
		}

		void WaveIn::Loop()
		{
			using namespace System::Runtime::InteropServices;

			DWORD taskIndex = 0;
			HANDLE taskHandle = AvSetMmThreadCharacteristics(TEXT("Audio"), &taskIndex);
			array<System::Byte> ^bytes = gcnew array<System::Byte>(m_bufferSize);
			AutoResetEvent ^bufEvent = gcnew AutoResetEvent(false);
			array<WaitHandle^> ^handles = { m_stop, bufEvent };
			int bufIdx = 0;
			HWAVEIN wavHandle;
			WAVEHDR hdr[2];

			try
			{
				ThrowOnFailure(waveInOpen(&wavHandle, WAVE_MAPPER, m_format->Data, (int)bufEvent->Handle, 0, CALLBACK_EVENT));
				m_wavHandle = wavHandle;
				for(int i = 0; i < 2; i++)
				{
					hdr[i].lpData = (LPSTR)new BYTE[m_bufferSize];
					hdr[i].dwBufferLength = m_bufferSize;
					hdr[i].dwFlags = hdr[i].dwBytesRecorded = 0;
					ThrowOnFailure(waveInPrepareHeader(wavHandle, &hdr[i], sizeof(WAVEHDR)));
				}
				waveInStart(wavHandle);

				while(true)
				{
					switch(WaitHandle::WaitAny(handles))
					{
					case 0:
						return;
					case 1:
						for(int i = 0; i < 2; i++)
						{
							if((hdr[i].dwFlags & WHDR_INQUEUE) == 0)
							{
								int count = hdr[i].dwBytesRecorded;
								if(count > 0)
								{
									Marshal::Copy((IntPtr)hdr[i].lpData, bytes, 0, count);
									m_stream->Write(bytes, 0, count);
								}
								ThrowOnFailure(waveInAddBuffer(wavHandle, &hdr[i], sizeof(WAVEHDR)));
							}
						}
					}
				}
			}
			finally
			{
				ThrowOnFailure(waveInReset(wavHandle));
				for(int i = 0; i < 2; i++)
				{
					ThrowOnFailure(waveInUnprepareHeader(wavHandle, &hdr[i], sizeof(WAVEHDR)));
					delete[] (BYTE*)hdr[i].lpData;
				}
				ThrowOnFailure(waveInClose(wavHandle));
				AvRevertMmThreadCharacteristics(taskHandle);
			}
		}

		WaveIn::~WaveIn()
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

		WaveIn::!WaveIn()
		{
			this->~WaveIn();
		}
	}
}
