#include "Stdafx.h"
#include "WaveFormat.h"

namespace Floe
{
	namespace Interop
	{
		using namespace System;
		using namespace System::IO;

		public ref class AudioFilePlayer
		{
		private:
			Stream ^m_stream;

		public:
			AudioFilePlayer(String ^fileName);
			void Start();
			void Stop();
		};
	}
}
