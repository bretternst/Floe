#include "Stdafx.h"
#include "AudioRenderClient.h"

namespace Floe
{
	namespace Audio
	{
		using namespace System::Threading;
		const IID IID_IAudioRenderClient = __uuidof(IAudioRenderClient);

		AudioRenderClient::AudioRenderClient()
			: AudioClient(AudioMode::Render)
		{
			IAudioRenderClient *iarc;
			ThrowOnFailure(this->Client->GetService(IID_IAudioRenderClient, (void**)&iarc));
			m_iarc = iarc;
		}

		void AudioRenderClient::Loop()
		{
			array<WaitHandle^>^ handles = { this->CancelHandle, this->BufferHandle };
			this->RenderBuffer(this->BufferSizeInFrames);
			ThrowOnFailure(this->Client->Start());

			try
			{
				while(true)
				{
					switch(WaitHandle::WaitAny(handles))
					{
					case 0:
						return;
					case 1:
						int padding;
						this->Client->GetCurrentPadding((UINT32*)&padding);
						this->RenderBuffer(this->BufferSizeInFrames - padding);
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
			int realCount = this->OnRender(count, (IntPtr)buffer);
			count = realCount > 0 ? realCount : count;
			ThrowOnFailure(m_iarc->ReleaseBuffer(count, realCount > 0 ? 0 : AUDCLNT_BUFFERFLAGS_SILENT));
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
