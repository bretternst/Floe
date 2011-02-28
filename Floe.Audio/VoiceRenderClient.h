#pragma once
#include "Stdafx.h"
#include "AudioRenderClient.h"

namespace Floe
{
	namespace Audio
	{
		public ref class VoiceRenderClient abstract : AudioRenderClient
		{
		private:
			WAVEFORMATEX *m_format;
			HACMSTREAM m_acmStream;
			LPACMSTREAMHEADER m_acmHeader;

		public:
			VoiceRenderClient();

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
			virtual int OnRender(int count, IntPtr buffer) override;
			virtual int OnPacketNeeded(int size, IntPtr buffer) abstract;

		private:
			~VoiceRenderClient();
			!VoiceRenderClient();
		};
	}
}
