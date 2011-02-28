#pragma once
#include "Stdafx.h"
#include "AudioCaptureClient.h"

namespace Floe
{
	namespace Audio
	{
		public ref class VoiceCaptureClient abstract : AudioCaptureClient
		{
		private:
			WAVEFORMATEX *m_format;
			HACMSTREAM m_acmStream;
			LPACMSTREAMHEADER m_acmHeader;

		public:
			VoiceCaptureClient();

			property WAVEFORMATEX *VoiceFormat
			{
				WAVEFORMATEX *get()
				{
					return m_format;
				}
			}

			property int VoiceFrameSize
			{
				int get()
				{
					return m_format->nBlockAlign;
				}
			}

			property int VoiceBufferSize
			{
				int get()
				{
					return m_acmHeader->cbDstLength;
				}
			}

		protected:
			virtual void OnCapture(int count, IntPtr buffer) override;
			virtual void OnPacketReady(int size, IntPtr buffer) abstract;

		private:
			~VoiceCaptureClient();
			!VoiceCaptureClient();
		};
	}
}
