#include "Stdafx.h"
#include "AudioRenderClient.h"

namespace Floe
{
	namespace Audio
	{
		using namespace System::Threading;
		const IID IID_IAudioRenderClient = __uuidof(IAudioRenderClient);

		AudioRenderClient::AudioRenderClient()
			: AudioClient()
		{
			IAudioRenderClient *iarc;
			ThrowOnFailure(m_iac->GetService(IID_IAudioRenderClient, &iarc));
			m_iarc = iarc;
		}

		void AudioRenderClient::Loop()
		{
			array<WaitHandle^>^ handles = { this->CancelEvent, this->BufferEvent };
			if(!this->RenderBuffer(this->BufferSize))
			{
				return;
			}

			ThrowOnFailure(this->Client->Start());

			try
			{
				switch(WaitHandle::WaitAny(handles))
				{
				case 0:
					return;
				case 1:
					int padding;
					this->Client->GetCurrentPadding(&padding);
					if(!this->RenderBuffer(this->BufferSize - padding))
					{
						return;
					}
				}
			}
			finally
			{
				ThrowOnFailure(this->Client->Stop());
			}
		}

		void AudioRenderClient::RenderBuffer(int count)
		{
			BYTE *buffer;
			ThrowOnFailure(m_iarc->GetBuffer(count, &buffer));
			count = this->OnRender(count, (IntPtr)buffer);
			ThrowOnFailure(m_iarc->ReleaseBuffer(count, 0));
			return count > 0;
		}

		AudioRenderClient::~AudioRenderClient()
		{
			if(m_iarc != 0)
			{
				m_iarc->Release();
				m_iarc = 0;
			}
		}

		AudioRenderClient::!AudioRenderClient()
		{
			this->~AudioRenderClient();
		}
	}
}
