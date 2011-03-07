#include "Stdafx.h"
#include "AudioMeter.h"

namespace Floe
{
	namespace Interop
	{
		AudioMeter::AudioMeter(AudioDevice ^device)
		{
			m_iami = device->ActivateMeter();
		}

		AudioMeter::~AudioMeter()
		{
			if(m_iami != 0)
			{
				m_iami->Release();
				m_iami = 0;
			}
		}

		AudioMeter::!AudioMeter()
		{
			this->~AudioMeter();
		}
	}
}