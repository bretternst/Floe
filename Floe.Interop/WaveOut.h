#pragma once
#include "Stdafx.h"
#include "Common.h"

namespace Floe
{
	namespace Interop
	{
		using namespace System::IO;
		using namespace System::Threading;
		using System::Math;

		public ref class WaveOut
		{
		private:
			Stream ^m_stream;
			WaveFormat ^m_format;
			Thread ^m_thread;
			AutoResetEvent ^m_stop;
			int m_bufferSize;
			float m_volume;
			HWAVEOUT m_wavHandle;

		public:
			WaveOut(Stream ^stream, WaveFormat ^format, int bufferSize);
			void Start();
			void Pause();
			void Resume();
			void Close();
			event System::EventHandler<InteropErrorEventArgs^> ^Error;

			property float Volume
			{
				float get()
				{
					DWORD vol;
					waveOutGetVolume(m_wavHandle, &vol);
					return (float)(vol & 0xffff) / 255.0f;
				}
				void set(float value)
				{
					m_volume = Math::Max(0.0f, Math::Min(1.0f, value));
					DWORD vol = (DWORD)(value * 0xffff);
					vol |= (vol << 16);
					waveOutSetVolume(m_wavHandle, vol);
				}
			}

			event System::EventHandler ^EndOfStream;

		private:
			void Loop();
			~WaveOut();
			!WaveOut();
		};
	}
}
