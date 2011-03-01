#pragma once
#include "Stdafx.h"
#include "AudioRenderClient.h"
#include "AudioConverter.h"

namespace Floe
{
	namespace Audio
	{
		public ref class VoiceRenderClient abstract : AudioRenderClient
		{
		private:
			AudioConverter ^m_converter;
			int m_packetSize;
			BYTE *m_buffer;
			int m_used;

		public:
			VoiceRenderClient(AudioDevice^ device);

			property int PacketSize
			{
				int get()
				{
					return m_packetSize;
				}
			}

		protected:
			virtual int OnRender(int count, IntPtr buffer) override;
			virtual bool OnReadPacket(IntPtr buffer) abstract;

		private:
			~VoiceRenderClient();
			!VoiceRenderClient();
		};
	}
}
