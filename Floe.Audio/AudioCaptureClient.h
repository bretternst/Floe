#pragma once
#include "Stdafx.h"
#include "AudioClient.h"
#include "AudioConverter.h"

namespace Floe
{
	namespace Audio
	{
		using System::IntPtr;

		public ref class AudioCaptureClient abstract : AudioClient
		{
		private:
			IAudioCaptureClient *m_iacc;
			AudioConverter ^m_converter;
			int m_packetSize;
			BYTE *m_buffer;
			int m_used;

		public:
			AudioCaptureClient(AudioDevice^ device, int packetSize, ...array<WaveFormat^> ^conversions);

			property int PacketSize
			{
				int get()
				{
					return m_packetSize;
				}
			}

		protected:
			virtual void Loop() override;
			virtual void OnCapture(int count, IntPtr buffer);
			virtual void OnWritePacket(IntPtr buffer) abstract;

		private:
			void CaptureBuffer();
			~AudioCaptureClient();
			!AudioCaptureClient();
		};
	}
}
