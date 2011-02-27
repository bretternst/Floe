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
			AudioRenderClient();

		protected:
			virtual void Loop() override;
			virtual int OnRender(int count, IntPtr buffer);

		private:
			bool RenderBuffer(int count);
			~AudioRenderClient();
			!AudioRenderClient();
		};
	}
}
