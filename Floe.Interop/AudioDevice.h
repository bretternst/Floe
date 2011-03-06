#pragma once
#include "Stdafx.h"
#include "Common.h"

namespace Floe
{
	namespace Interop
	{
		public ref class AudioDevice
		{
		private:
			IMMDevice *m_immd;

		public:
			IAudioClient *Activate();
			IAudioMeterInformation *ActivateMeter();

			static property AudioDevice^ DefaultRenderDevice
			{
				AudioDevice^ get()
				{
					return GetDefaultDevice(eRender);
				}
			}

			static property AudioDevice^ DefaultCaptureDevice
			{
				AudioDevice^ get()
				{
					return GetDefaultDevice(eCapture);
				}
			}

		private:
			AudioDevice(IMMDevice *immd);
			~AudioDevice();
			!AudioDevice();
			static AudioDevice^ GetDefaultDevice(EDataFlow role);
		};
	}
}