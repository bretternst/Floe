#pragma once
#include "Stdafx.h"
#include "AudioCaptureClient.h"

namespace Floe
{
	namespace Interop
	{
		public ref class AudioMeter
		{
		private:
			IAudioMeterInformation *m_iami;
			IAudioClient *m_iac;

		public:
			AudioMeter(AudioDevice ^device);

			property float Peak
			{
				float get()
				{
					float peak;
					m_iami->GetPeakValue(&peak);
					return peak;
				}
			}

		private:
			~AudioMeter();
			!AudioMeter();
		};
	}
}