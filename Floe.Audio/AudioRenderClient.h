#pragma once
#include "Stdafx.h"
#include "AudioClient.h"

namespace Floe
{
	namespace Audio
	{
		using System::IntPtr;

		public ref class AudioRenderClient abstract : AudioClient
		{
		private:
			IAudioRenderClient *m_iarc;

		public:
			AudioRenderClient(AudioDevice^ device);

		protected:
			virtual void Loop() override;
			virtual int OnRender(int count, IntPtr buffer) abstract;

		private:
			void RenderBuffer(int count);
			~AudioRenderClient();
			!AudioRenderClient();
		};
	}
}
