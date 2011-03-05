#pragma once
#include "Stdafx.h"
#include "AudioClient.h"
#include "AudioConverter.h"

namespace Floe
{
	namespace Audio
	{
		using System::IntPtr;

		public ref class ReadPacketEventArgs : System::EventArgs
		{
		private:
			IntPtr m_buffer;
			bool m_hasData;

		public:
			property IntPtr Buffer
			{
				IntPtr get()
				{
					return m_buffer;
				}
			internal:
				void set(IntPtr buffer)
				{
					m_buffer = buffer;
				}
			}

			property bool HasData
			{
				bool get()
				{
					return m_hasData;
				}
				void set(bool hasData)
				{
					m_hasData = hasData;
				}
			}
		};

		public ref class AudioRenderClient : AudioClient
		{
		private:
			IAudioRenderClient *m_iarc;
			AudioConverter ^m_converter;
			int m_packetSize;
			BYTE *m_buffer;
			BYTE *m_packet;
			int m_used;
			ReadPacketEventArgs ^m_eventArgs;

		public:
			AudioRenderClient(AudioDevice^ device, int packetSize, int minBufferSize, ...array<WaveFormat^> ^conversions);

			event System::EventHandler<ReadPacketEventArgs^> ^ReadPacket;

			property int PacketSize
			{
				int get()
				{
					return m_packetSize;
				}
			}

		protected:
			virtual void __clrcall Loop() override;
			virtual int __clrcall OnRender(int count, IntPtr buffer);
			virtual bool __clrcall OnReadPacket(IntPtr buffer);

		private:
			void RenderBuffer(int count);
			~AudioRenderClient();
			!AudioRenderClient();
		};
	}
}
