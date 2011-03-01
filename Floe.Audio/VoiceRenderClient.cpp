#include "Stdafx.h"
#include "VoiceRenderClient.h"

namespace Floe
{
	namespace Audio
	{
		VoiceRenderClient::VoiceRenderClient(AudioDevice^ device)
			: AudioRenderClient(device)
		{
			WAVEFORMATEX format;
			format.wFormatTag = WAVE_FORMAT_PCM;
			format.nSamplesPerSec = 22050;
			format.wBitsPerSample = 16;
			format.nChannels = 1;
			format.cbSize = 0;
			format.nBlockAlign = format.nChannels * format.wBitsPerSample / 8;
			format.nAvgBytesPerSec = format.nSamplesPerSec * format.nBlockAlign;

			m_packetSize = 800;
			m_converter = gcnew AudioConverter(m_packetSize, &format, this->Format);
			m_buffer = new BYTE[this->BufferSizeInBytes + m_converter->DestBufferSize];
		}

		int VoiceRenderClient::OnRender(int count, IntPtr buffer)
		{
			count *= this->FrameSize;

			while(m_used < count && this->OnReadPacket(m_converter->Buffer))
			{
				IntPtr dst;
				int total = m_converter->Convert(m_packetSize, dst);
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
		}

		VoiceRenderClient::~VoiceRenderClient()
		{
			if(m_buffer != 0)
			{
				delete m_buffer;
				m_buffer = 0;
			}
		}

		VoiceRenderClient::!VoiceRenderClient()
		{
			this->~VoiceRenderClient();
		}
	}
}
