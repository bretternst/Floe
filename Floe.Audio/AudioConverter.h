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
			int Convert(int size, [Out] IntPtr &dstBuffer);

			property IntPtr Buffer
			{
				IntPtr get()
				{
					return (IntPtr)m_headers[0]->pbSrc;
				}
			}

			property int SourceBufferSize
			{
				int get()
				{
					return m_headers[0]->dwSrcUser;
				}
			}

			property int DestBufferSize
			{
				int get()
				{
					return m_headers[m_count - 1]->dwDstUser;
				}
			}

		private:
			~AudioConverter();
			!AudioConverter();
		};
	}
}