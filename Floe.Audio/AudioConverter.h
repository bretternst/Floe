#pragma once
#include "Stdafx.h"
#include "Common.h"

namespace Floe
{
	namespace Audio
	{
		using System::IntPtr;
		using namespace System::Collections::Generic;
		using namespace System::Runtime::InteropServices;

		public ref class AudioConverter
		{
		private:
			HACMSTREAM *m_streams;
			LPACMSTREAMHEADER *m_headers;
			int m_count;

		public:
			AudioConverter(int maxSrcSize, WAVEFORMATEX *srcFormat, ...array<WAVEFORMATEX*> ^dstFormats);
			int Convert(IntPtr srcBuffer, int size, [Out] IntPtr &dstBuffer);

		private:
			~AudioConverter();
			!AudioConverter();
		};
	}
}