#include "Stdafx.h"
#include "AudioRenderClient.h"

namespace Floe
{
	namespace Interop
	{
		using namespace System::Threading;
		const IID IID_IAudioRenderClient = __uuidof(IAudioRenderClient);

		AudioRenderClient::AudioRenderClient(AudioDevice^ device, int packetSize, int minBufferSize, ...array<WaveFormat^> ^conversions)
			: AudioClient(device)
		{
			IAudioRenderClient *iarc;
			ThrowOnFailure(this->Client->GetService(IID_IAudioRenderClient, (void**)&iarc));
			m_iarc = iarc;
			m_packetSize = packetSize;

			if(conversions->Length < 1)
			{
				throw gcnew System::ArgumentException("Must specify at least one conversion format.");
			}
			array<WaveFormat^> ^waveConversions = gcnew array<WaveFormat^>(conversions->Length);
			if(conversions->Length > 1)
			{
				System::Array::Copy(conversions, 1, waveConversions, 0, conversions->Length - 1);
			}
			waveConversions[conversions->Length - 1] = this->Format;
			m_converter = gcnew AudioConverter(System::Math::Max(minBufferSize, m_packetSize), conversions[0], waveConversions);
			m_buffer = new BYTE[this->BufferSizeInBytes + m_converter->DestBufferSize];
			m_packet = new BYTE[packetSize];
			m_eventArgs = gcnew ReadPacketEventArgs();
		}

		int AudioRenderClient::OnRender(int count, IntPtr buffer)
		{
			count *= this->FrameSize;
			int packetLength = 0;

			while(m_used < count && (packetLength = this->OnReadPacket((IntPtr)m_packet)) > 0)
			{
				IntPtr dst;
				int total = m_converter->Convert((IntPtr)(void*)m_packet, packetLength, dst);
				memcpy((void*)(m_buffer + m_used), (void*)dst, total);
				m_used += total;
			}

			if(m_used >= count)
			{
				memcpy((void*)buffer, (void*)m_buffer, count);
				m_used -= count;
				if(m_used > 0)
				{
					memmove((void*)m_buffer, (void*)(m_buffer+count), m_used);
				}
				return count / this->FrameSize;
			}
			else if (m_used > 0)
			{
				memcpy((void*)buffer, (void*)m_buffer, m_used);
				m_used = 0;
				return m_used / this->FrameSize;
			}
			else
			{
				return 0;
			}
		}

		int AudioRenderClient::OnReadPacket(IntPtr buffer)
		{
			m_eventArgs->Buffer = buffer;
			m_eventArgs->Length = 0;
			this->ReadPacket(this, m_eventArgs);
			return m_eventArgs->Length;
		}

		void AudioRenderClient::Loop()
		{
			DWORD taskIndex = 0;
			HANDLE taskHandle = AvSetMmThreadCharacteristics(TEXT("Audio"), &taskIndex);
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
				if(taskHandle != 0)
				{
					AvRevertMmThreadCharacteristics(taskHandle);
				}
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
			this->Stop();
			if(m_iarc != 0)
			{
				m_iarc->Release();
				m_iarc = 0;
			}
			if(m_buffer != 0)
			{
				delete m_buffer;
				m_buffer = 0;
			}
			if(m_packet != 0)
			{
				delete m_packet;
				m_packet = 0;
			}
		}

		AudioRenderClient::!AudioRenderClient()
		{
			this->~AudioRenderClient();
		}
	}
}
