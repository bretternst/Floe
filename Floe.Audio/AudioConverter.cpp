#include "Stdafx.h"
#include "AudioConverter.h"

namespace Floe
{
	namespace Audio
	{
		AudioConverter::AudioConverter(int maxSrcSize, WAVEFORMATEX *srcFormat, ...array<WAVEFORMATEX*> ^dstFormats)
		{
			m_count = dstFormats->Length;
			if(m_count < 1)
			{
				throw gcnew System::ArgumentException("At least one destination format must be specified.");
			}

			m_streams = new HACMSTREAM[m_count];
			m_headers = new LPACMSTREAMHEADER[m_count];

			int srcSize, dstSize;
			for(int i = 0; i < m_count; i++)
			{
				ThrowOnFailure(acmStreamOpen(&m_streams[i], 0, i == 0 ? srcFormat : dstFormats[i-1], dstFormats[i], 0, 0, 0, 0));
				m_headers[i] = new ACMSTREAMHEADER();
				m_headers[i]->cbStruct = sizeof(ACMSTREAMHEADER);
				m_headers[i]->cbSrcLength = m_headers[i]->dwSrcUser = srcSize = i == 0 ? maxSrcSize : m_headers[i-1]->cbDstLength;
				m_headers[i]->pbSrc = i == 0 ? new BYTE[srcSize] : m_headers[i-1]->pbDst;
				ThrowOnFailure(acmStreamSize(m_streams[i], srcSize, (LPDWORD)&dstSize, ACM_STREAMSIZEF_SOURCE));
				m_headers[i]->cbDstLength = m_headers[i]->dwDstUser = dstSize;
				m_headers[i]->pbDst = new BYTE[dstSize];
				m_headers[i]->fdwStatus = 0;
				ThrowOnFailure(acmStreamPrepareHeader(m_streams[i], m_headers[i], 0));
			}
		}

		int AudioConverter::Convert(int size, [Out] IntPtr &dstBuffer)
		{
			for(int i = 0; i < m_count; i++)
			{
				m_headers[i]->cbSrcLength = m_headers[i]->cbSrcLengthUsed = i == 0 ? size : m_headers[i-1]->cbDstLengthUsed;
				ThrowOnFailure(acmStreamConvert(m_streams[i], m_headers[i], ACM_STREAMCONVERTF_BLOCKALIGN));
			}
			dstBuffer = (IntPtr)m_headers[m_count-1]->pbDst;
			return m_headers[m_count-1]->cbDstLengthUsed;
		}

		AudioConverter::~AudioConverter()
		{
			if(m_headers != 0 && m_streams != 0)
			{
				for(int i = 0; i < m_count; i++)
				{
					m_headers[i]->cbSrcLength = m_headers[i]->dwSrcUser;
					m_headers[i]->cbDstLength = m_headers[i]->dwDstUser;
					acmStreamUnprepareHeader(m_streams[i], m_headers[i], 0);
					acmStreamClose(m_streams[i], 0);
					delete m_headers[i]->pbSrc;
					if(i == m_count - 1)
					{
						delete m_headers[m_count-1]->pbDst;
					}
					delete m_headers[i];
				}
				delete m_headers;
				delete m_streams;
				m_headers = 0;
				m_streams = 0;
			}
		}

		AudioConverter::!AudioConverter()
		{
			this->~AudioConverter();
		}
	}
}
