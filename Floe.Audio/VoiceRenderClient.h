#pragma once
#include "Stdafx.h"
#include "AudioRenderClient.h"
#include "AudioConverter.h"

namespace Floe
{
	namespace Audio
	{
		public ref class VoiceRenderClient abstract : AudioRenderClient
		{
		private:

		public:
			VoiceRenderClient(AudioDevice^ device);


		protected:
			virtual int OnRender(int count, IntPtr buffer) override;

		private:
			~VoiceRenderClient();
			!VoiceRenderClient();
		};
	}
}
