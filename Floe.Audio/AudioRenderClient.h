#pragma once
#include "Stdafx.h"
#include "AudioClient.h"
#include "AudioConverter.h"

namespace Floe
{
	namespace Audio
	{
		using System::IntPtr;

		public ref class AudioRenderClient abstract : AudioClient
		{
		private:
			IAudioRenderClient *m_iarc;
			AudioConverter ^m_converter;
			int m_packetSize;
			BYTE *m_buffer;
			BYTE *m_packet;
			int m_used;

		public:
			AudioRenderClient(AudioDevice^ device, int packetSize, int minBufferSize, ...array<WaveFormat^> ^conversions);

			property int PacketSize
			{
				int get()
				{
					return m_packetSize;
				}
			}

		protected:
			virtual void Loop() override;
			virtual int OnRender(int count, IntPtr buffer);
			virtual bool OnReadPacket(IntPtr buffer) abstract;

		private:
			void RenderBuffer(int count);
			~AudioRenderClient();
			!AudioRenderClient();
		};
	}
}
