#include "Stdafx.h"
#include "VoiceCaptureClient.h"

namespace Floe
{
	namespace Audio
	{
		using System::Math;

		VoiceCaptureClient::VoiceCaptureClient(AudioDevice^ device)
			: AudioCaptureClient(device)
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
			m_converter = gcnew AudioConverter(this->BufferSizeInBytes, this->Format, &format);
			m_buffer = new BYTE[m_packetSize];
		}

		void VoiceCaptureClient::OnCapture(int count, IntPtr buffer)
		{
			count *= this->FrameSize;
			memcpy((void*)m_converter->Buffer, (void*)buffer, count);
			int total = m_converter->Convert(count, buffer);
			BYTE *src = (BYTE*)(void*)buffer;
			while(total > 0)
			{
				int copied = Math::Min(m_packetSize - m_used, total);
				memcpy((void*)(m_buffer+m_used), src, copied);
				src += copied;
				m_used += copied;
				total -= copied;

				if(m_used == m_packetSize)
				{
					this->OnWritePacket((IntPtr)m_buffer);
					m_used = 0;
				}
			}
		}

		VoiceCaptureClient::~VoiceCaptureClient()
		{
			if(m_buffer != 0)
			{
				delete m_buffer;
				m_buffer = 0;
			}
		}

		VoiceCaptureClient::!VoiceCaptureClient()
		{
			this->~VoiceCaptureClient();
		}
	}
}