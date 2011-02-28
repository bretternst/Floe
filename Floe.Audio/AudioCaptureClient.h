#pragma once
#include "Stdafx.h"
#include "AudioClient.h"

namespace Floe
{
	namespace Audio
	{
		using System::IntPtr;

		public ref class AudioCaptureClient abstract : AudioClient
		{
		private:
			IAudioCaptureClient *m_iacc;

		public:
			AudioCaptureClient();

		protected:
			virtual void Loop() override;
			virtual void OnCapture(int count, IntPtr buffer) abstract;

		private:
			void CaptureBuffer();
			~AudioCaptureClient();
			!AudioCaptureClient();
		};
	}
}
