#include "Stdafx.h"
#include "VoiceCaptureClient.h"

namespace Floe
{
	namespace Audio
	{
		VoiceCaptureClient::VoiceCaptureClient()
		{
			m_format = new WAVEFORMATEX();
			m_format->wFormatTag = WAVE_FORMAT_PCM;
			m_format->nSamplesPerSec = 11025;
			m_format->wBitsPerSample = 16;
			m_format->nChannels = 1;
			m_format->cbSize = 0;
			m_format->nBlockAlign = m_format->nChannels * m_format->wBitsPerSample / 8;
			m_format->nAvgBytesPerSec = m_format->nSamplesPerSec * m_format->nBlockAlign;

			HACMSTREAM acmStream;
			ThrowOnFailure(acmStreamOpen(&acmStream, 0, this->Format, m_format, 0, 0, 0, 0));
			m_acmStream = acmStream;

			m_acmHeader = new ACMSTREAMHEADER();
			m_acmHeader->cbStruct = sizeof(ACMSTREAMHEADER);
			m_acmHeader->cbSrcLength = m_acmHeader->dwSrcUser = this->BufferSizeInBytes;
			m_acmHeader->pbSrc = new BYTE[m_acmHeader->cbSrcLength];

			int dstSize;
			ThrowOnFailure(acmStreamSize(m_acmStream, m_acmHeader->cbSrcLength, (LPDWORD)&dstSize, ACM_STREAMSIZEF_SOURCE));
			m_acmHeader->cbDstLength = dstSize;
			m_acmHeader->pbDst = new BYTE[m_acmHeader->cbDstLength];
			m_acmHeader->fdwStatus = 0;
			ThrowOnFailure(acmStreamPrepareHeader(acmStream, m_acmHeader, 0));
		}

		void VoiceCaptureClient::OnCapture(int count, IntPtr buffer)
		{
			m_acmHeader->cbSrcLength = count * this->FrameSize;
			memcpy(m_acmHeader->pbSrc, (void*)buffer, m_acmHeader->cbSrcLengthUsed);
			ThrowOnFailure(acmStreamConvert(m_acmStream, m_acmHeader, ACM_STREAMCONVERTF_BLOCKALIGN));
			this->OnPacketReady(m_acmHeader->cbDstLengthUsed, (IntPtr)m_acmHeader->pbDst);
		}

		VoiceCaptureClient::~VoiceCaptureClient()
		{
			if(m_format != 0)
			{
				delete m_format;
				m_format = 0;
			}
			if(m_acmHeader != 0)
			{
				m_acmHeader->cbSrcLength = m_acmHeader->dwSrcUser;
				acmStreamUnprepareHeader(m_acmStream, m_acmHeader, 0);
				acmStreamClose(m_acmStream, 0);
				delete m_acmHeader->pbSrc;
				delete m_acmHeader->pbDst;
				delete m_acmHeader;
				m_acmHeader = 0;
			}
		}

		VoiceCaptureClient::!VoiceCaptureClient()
		{
			this->~VoiceCaptureClient();
		}
	}
}
