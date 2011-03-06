#pragma once
#include "Stdafx.h"
#include "AudioClient.h"
#include "AudioConverter.h"

namespace Floe
{
	namespace Interop
	{
		using System::IntPtr;

		public ref class WritePacketEventArgs : System::EventArgs
		{
		private:
			IntPtr m_buffer;

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
		};

		public ref class AudioCaptureClient : AudioClient
		{
		private:
			IAudioCaptureClient *m_iacc;
			AudioConverter ^m_converter;
			int m_packetSize;
			BYTE *m_buffer;
			int m_used;
			WritePacketEventArgs ^m_eventArgs;

		public:
			AudioCaptureClient(AudioDevice^ device, int packetSize, int minBufferSize, ...array<WaveFormat^> ^conversions);

			event System::EventHandler<WritePacketEventArgs^> ^WritePacket;

			property int PacketSize
			{
				int get()
				{
					return m_packetSize;
				}
			}

		protected:
			virtual void __clrcall Loop() override;
			virtual void __clrcall OnCapture(int count, IntPtr buffer);
			virtual void __clrcall OnWritePacket(IntPtr buffer);

		private:
			void CaptureBuffer();
			~AudioCaptureClient();
			!AudioCaptureClient();
		};
	}
}
