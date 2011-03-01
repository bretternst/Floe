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
			format.nSamplesPerSec = 11025;
			format.wBitsPerSample = 16;
			format.nChannels = 1;
			format.cbSize = 0;
			format.nBlockAlign = format.nChannels * format.wBitsPerSample / 8;
			format.nAvgBytesPerSec = format.nSamplesPerSec * format.nBlockAlign;

			m_packetSize = 1000;
			m_converter = gcnew AudioConverter(m_packetSize, &format, this->Format);
		}

		int VoiceRenderClient::OnRender(int count, IntPtr buffer)
		{
			return 0;
		}

		VoiceRenderClient::~VoiceRenderClient()
		{
		}

		VoiceRenderClient::!VoiceRenderClient()
		{
			this->~VoiceRenderClient();
		}
	}
}
