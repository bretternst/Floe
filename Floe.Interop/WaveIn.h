#pragma once
#include "Stdafx.h"
#include "Common.h"

namespace Floe
{
	namespace Interop
	{
		using namespace System::IO;
		using namespace System::Threading;
		using namespace System::Threading::Tasks;

		public ref class WaveIn
		{
		private:
			Stream ^m_stream;
			WaveFormat ^m_format;
			Task ^m_task;
			AutoResetEvent ^m_stop;
			int m_bufferSize;
			HWAVEIN m_wavHandle;

		public:
			WaveIn(Stream ^stream, WaveFormat ^format, int bufferSize);
			void Start();
			void Pause();
			void Resume();
			void Close();

		private:
			void Loop();
			~WaveIn();
			!WaveIn();
		};
	}
}
