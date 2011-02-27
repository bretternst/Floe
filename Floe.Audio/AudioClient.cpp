#include "Stdafx.h"
#include "AudioClient.h"

namespace Floe
{
	namespace Audio
	{
		const CLSID CLSID_MMDeviceEnumerator = __uuidof(MMDeviceEnumerator);
		const IID IID_IMMDeviceEnumerator = __uuidof(IMMDeviceEnumerator);
		const IID IID_IAudioClient = __uuidof(IAudioClient);
		const IID IID_IAudioCaptureClient = __uuidof(IAudioCaptureClient);

		AudioClient::AudioClient()
		{
			IMMDeviceEnumerator *immde = 0;
			IMMDevice *immd = 0;
			try
			{
				ThrowOnFailure(CoCreateInstance(CLSID_MMDeviceEnumerator, 0, CLSCTX_ALL, IID_IMMDeviceEnumerator, (void**)&immde));
				immde->GetDefaultAudioEndpoint(eCapture, eMultimedia, &immd);
				if(immd == 0)
				{
					throw gcnew AudioException("Unable to locate default capture device.");
				}
				IAudioClient *iac;
				ThrowOnFailure(immd->Activate(IID_IAudioClient, CLSCTX_ALL, 0, (void**)&iac));
				m_iac = iac;
			}
			finally
			{
				if(immde != 0)
				{
					immde->Release();
				}
				if(immd != 0)
				{
					immd->Release();
				}
			}

			WAVEFORMATEX* fmt;
			ThrowOnFailure(m_iac->GetMixFormat(&fmt));
			m_format = fmt;
			m_cancelEvent = gcnew ManualResetEvent(false);
			m_bufferEvent = gcnew AutoResetEvent(false);

			ThrowOnFailure(m_iac->Initialize(AUDCLNT_SHAREMODE_SHARED, AUDCLNT_STREAMFLAGS_EVENTCALLBACK, 0, 0, m_format, 0));
			ThrowOnFailure(m_iac->SetEventHandle((HANDLE)m_bufferEvent->Handle));
			int bufferSize;
			ThrowOnFailure(m_iac->GetBufferSize((UINT32*)&bufferSize));
			m_bufferSize = bufferSize;
		}

		void AudioClient::Start()
		{
			m_cancelEvent->Reset();
			m_bufferEvent->Reset();
			m_task = Task::Factory->StartNew(gcnew System::Action(this, &AudioClient::Loop));
		}

		void AudioClient::Stop()
		{
			m_cancelEvent->Set();
			m_task->Wait();
		}

		AudioClient::~AudioClient()
		{
			if(m_iac != 0)
			{
				m_iac->Release();
				m_iac = 0;
			}
			if(m_format != 0)
			{
				CoTaskMemFree(m_format);
				m_format = 0;
			}
		}

		AudioClient::!AudioClient()
		{
			this->~AudioClient();
		}
	}
}
