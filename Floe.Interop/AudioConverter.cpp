#include "Stdafx.h"
#include "AudioConverter.h"

namespace Floe
{
	namespace Interop
	{
		AudioConverter::AudioConverter(int maxSrcSize, WaveFormat ^srcFormat, ...array<WaveFormat^> ^dstFormats)
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
				ThrowOnFailure(acmStreamOpen(&m_streams[i], 0, i == 0 ? srcFormat->Data : dstFormats[i-1]->Data, dstFormats[i]->Data, 0, 0, 0, 0));
				m_headers[i] = new ACMSTREAMHEADER();
				m_headers[i]->cbStruct = sizeof(ACMSTREAMHEADER);
				m_headers[i]->cbSrcLength = m_headers[i]->dwSrcUser = srcSize = i == 0 ? maxSrcSize * 2 : m_headers[i-1]->cbDstLength;
				m_headers[i]->pbSrc = (LPBYTE)malloc(srcSize);
				ThrowOnFailure(acmStreamSize(m_streams[i], srcSize, (LPDWORD)&dstSize, ACM_STREAMSIZEF_SOURCE));
				m_headers[i]->cbDstLength = m_headers[i]->dwDstUser = dstSize;
				m_headers[i]->pbDst = (LPBYTE)malloc(dstSize);
				m_headers[i]->fdwStatus = 0;
				m_headers[i]->cbSrcLengthUsed = m_headers[i]->cbDstLengthUsed = 0;
				ThrowOnFailure(acmStreamPrepareHeader(m_streams[i], m_headers[i], 0));
				m_headers[i]->cbSrcLengthUsed = m_headers[i]->cbSrcLength;
			}
		}

		int AudioConverter::Convert(IntPtr srcBuffer, int size, [Out] IntPtr &dstBuffer)
		{
			for(int i = 0; i < m_count; i++)
			{
				int remainder = m_headers[i]->cbSrcLength - m_headers[i]->cbSrcLengthUsed;
				if(remainder > 0)
				{
					memmove(m_headers[i]->pbSrc, m_headers[i]->pbSrc + m_headers[i]->cbSrcLengthUsed, remainder);
				}
				m_headers[i]->cbSrcLength = i == 0 ? size : m_headers[i-1]->cbDstLengthUsed;
				memcpy(m_headers[i]->pbSrc + remainder, i == 0 ? (void*)srcBuffer : m_headers[i-1]->pbDst, m_headers[i]->cbSrcLength);
				m_headers[i]->cbSrcLength += remainder;
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
					delete m_headers[i]->pbDst;
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
