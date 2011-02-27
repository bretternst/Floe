#include "Stdafx.h"
#include "AudioCaptureClient.h"

namespace Floe
{
	namespace Audio
	{
		using namespace System::Threading;
		const IID IID_IAudioCaptureClient = __uuidof(IAudioCaptureClient);

		AudioCaptureClient::AudioCaptureClient()
		{
			IAudioCaptureClient *iacc;
			ThrowOnFailure(this->Client->GetService(IID_IAudioCaptureClient, (void**)&iacc));
			m_iacc = iacc;
		}

		void AudioCaptureClient::Loop()
		{
			array<WaitHandle^>^ handles = { this->CancelHandle, this->BufferHandle };

			ThrowOnFailure(this->Client->Start());

			try
			{
				switch(WaitHandle::WaitAny(handles))
				{
				case 0:
					return;
				case 1:
					this->CaptureBuffer();
					break;
				}
			}
			finally
			{
				ThrowOnFailure(this->Client->Stop());
			}
		}

		void AudioCaptureClient::CaptureBuffer()
		{
			BYTE *buffer;
			int count, flags;
			ThrowOnFailure(m_iacc->GetBuffer(&buffer, (UINT32*)&count, (DWORD*)&flags, 0, 0));
			this->OnCapture(count, (IntPtr)buffer);
			ThrowOnFailure(m_iacc->ReleaseBuffer(count));
		}

		AudioCaptureClient::~AudioCaptureClient()
		{
			if(m_iacc != 0)
			{
				m_iacc->Release();
				m_iacc = 0;
			}
		}

		AudioCaptureClient::!AudioCaptureClient()
		{
			this->~AudioCaptureClient();
		}
	}
}
