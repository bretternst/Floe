#include "Stdafx.h"
#include "AudioMeter.h"

namespace Floe
{
	namespace Interop
	{
		AudioMeter::AudioMeter(AudioDevice ^device)
		{
			m_iac = device->Activate();
			m_iami = device->ActivateMeter();
			m_iac->Start();
		}

		AudioMeter::~AudioMeter()
		{
			if(m_iami != 0)
			{
				m_iac->Stop();
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