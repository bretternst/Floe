#include "Stdafx.h"
#include "VoiceRenderClient.h"

namespace Floe
{
	namespace Audio
	{
		VoiceRenderClient::VoiceRenderClient(AudioDevice^ device)
			: AudioRenderClient(device)
		{
		}


		VoiceRenderClient::~VoiceRenderClient()
		{

		}

		VoiceRenderClient::!VoiceRenderClient()
		{
			this->~VoiceRenderClient();
		}
	}
}
