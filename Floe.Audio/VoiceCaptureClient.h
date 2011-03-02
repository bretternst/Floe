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

		public:
			VoiceCaptureClient(AudioDevice^ device);



		protected:
			virtual void OnCapture(int count, IntPtr buffer) override;

		private:
			~VoiceCaptureClient();
			!VoiceCaptureClient();
		};
	}
}
