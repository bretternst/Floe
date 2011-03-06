#include "Stdafx.h"
#include "AudioClient.h"

namespace Floe
{
	namespace Interop
	{
		const CLSID CLSID_MMDeviceEnumerator = __uuidof(MMDeviceEnumerator);
		const IID IID_IMMDeviceEnumerator = __uuidof(IMMDeviceEnumerator);
		const IID IID_IAudioClient = __uuidof(IAudioClient);
		const IID IID_IAudioMeterInformation = __uuidof(IAudioMeterInformation);

		AudioDevice::AudioDevice(IMMDevice *immd)
		{
			m_immd = immd;
		}

		IAudioClient *AudioDevice::Activate()
		{
			IAudioClient *iac;
			ThrowOnFailure(m_immd->Activate(IID_IAudioClient, CLSCTX_ALL, 0, (void**)&iac));
			return iac;
		}

		IAudioMeterInformation *AudioDevice::ActivateMeter()
		{
			IAudioMeterInformation *iami;
			ThrowOnFailure(m_immd->Activate(IID_IAudioMeterInformation, CLSCTX_ALL, 0, (void**)&iami));
			return iami;
		}

		AudioDevice^ AudioDevice::GetDefaultDevice(EDataFlow role)
		{
			IMMDeviceEnumerator *immde = 0;
			IMMDevice *immd = 0;
			try
			{
				ThrowOnFailure(CoCreateInstance(CLSID_MMDeviceEnumerator, 0, CLSCTX_ALL, IID_IMMDeviceEnumerator, (void**)&immde));
				immde->GetDefaultAudioEndpoint((EDataFlow)role, eMultimedia, &immd);
				if(immd == 0)
				{
					throw gcnew AudioException("Unable to locate default capture device.");
				}
				return gcnew AudioDevice(immd);
			}
			finally
			{
				if(immde != 0)
				{
					immde->Release();
				}
			}
		}

		AudioDevice::~AudioDevice()
		{
			if(m_immd != 0)
			{
				m_immd->Release();
				m_immd = 0;
			}
		}

		AudioDevice::!AudioDevice()
		{
			this->~AudioDevice();
		}
	}
}