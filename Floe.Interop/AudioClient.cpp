#include "Stdafx.h"
#include "AudioClient.h"

namespace Floe
{
	namespace Interop
	{
		const IID IID_ISimpleAudioVolume = __uuidof(ISimpleAudioVolume);

		AudioClient::AudioClient(AudioDevice^ device)
		{
			m_iac = device->Activate();
			ISimpleAudioVolume *isav;

			WAVEFORMATEX* fmt;
			ThrowOnFailure(m_iac->GetMixFormat(&fmt));
			fmt->wFormatTag = WAVE_FORMAT_PCM;
			fmt->cbSize = 0;
			fmt->wBitsPerSample = 16;
			fmt->nBlockAlign = fmt->nChannels * fmt->wBitsPerSample / 8;
			fmt->nAvgBytesPerSec = fmt->nSamplesPerSec * fmt->nBlockAlign;
			m_format = gcnew WaveFormat(fmt);
			CoTaskMemFree(fmt);
			m_cancelEvent = gcnew ManualResetEvent(false);
			m_bufferEvent = gcnew AutoResetEvent(false);

			ThrowOnFailure(m_iac->Initialize(AUDCLNT_SHAREMODE_SHARED, AUDCLNT_STREAMFLAGS_EVENTCALLBACK, 0, 0, m_format->Data, 0));
			ThrowOnFailure(m_iac->SetEventHandle((HANDLE)m_bufferEvent->Handle));
			int bufferSize;
			ThrowOnFailure(m_iac->GetBufferSize((UINT32*)&bufferSize));
			m_bufferSize = bufferSize;
			ThrowOnFailure(m_iac->GetService(IID_ISimpleAudioVolume, (void**)&isav));
			m_isav = isav;
		}

		void AudioClient::Start()
		{
			m_cancelEvent->Reset();
			m_bufferEvent->Reset();
			m_task = Task::Factory->StartNew(gcnew System::Action(this, &AudioClient::Loop));
		}

		void AudioClient::Stop()
		{
			if(m_task != nullptr)
			{
				m_cancelEvent->Set();
				m_task->Wait();
				m_task = nullptr;
			}
		}

		AudioClient::~AudioClient()
		{
			this->Stop();
			if(m_iac != 0)
			{
				m_iac->Release();
				m_iac = 0;
			}
			if(m_isav != 0)
			{
				m_isav->Release();
				m_isav = 0;
			}
			if(m_cancelEvent != nullptr)
			{
				m_cancelEvent->Close();
			}
			if(m_bufferEvent != nullptr)
			{
				m_bufferEvent->Close();
			}
		}

		AudioClient::!AudioClient()
		{
			this->~AudioClient();
		}
	}
}
