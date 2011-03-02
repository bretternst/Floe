#include "Stdafx.h"
#include "VoiceCaptureClient.h"

namespace Floe
{
	namespace Audio
	{
		using System::Math;

		VoiceCaptureClient::VoiceCaptureClient(AudioDevice^ device)
			: AudioCaptureClient(device)
		{

		}


		VoiceCaptureClient::~VoiceCaptureClient()
		{

		}

		VoiceCaptureClient::!VoiceCaptureClient()
		{
			this->~VoiceCaptureClient();
		}
	}
}
