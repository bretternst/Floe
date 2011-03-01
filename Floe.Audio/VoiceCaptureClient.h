#pragma once
#include "Stdafx.h"
#include "AudioCaptureClient.h"
#include "AudioConverter.h"

namespace Floe
{
	namespace Audio
	{
		public ref class VoiceCaptureClient abstract : AudioCaptureClient
		{
		private:
			AudioConverter ^m_converter;
			int m_packetSize;
			BYTE *m_buffer;
			int m_used;

		public:
			VoiceCaptureClient(AudioDevice^ device);

			property int PacketSize
			{
				int get()
				{
					return m_packetSize;
				}
			}

		protected:
			virtual void OnCapture(int count, IntPtr buffer) override;
			virtual void OnWritePacket(IntPtr buffer) abstract;

		private:
			~VoiceCaptureClient();
			!VoiceCaptureClient();
		};
	}
}
