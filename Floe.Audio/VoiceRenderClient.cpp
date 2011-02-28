#include "Stdafx.h"
#include "VoiceRenderClient.h"

namespace Floe
{
	namespace Audio
	{
		VoiceRenderClient::VoiceRenderClient()
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
			ThrowOnFailure(acmStreamOpen(&acmStream, 0, m_format, this->Format, 0, 0, 0, 0));
			m_acmStream = acmStream;

			m_acmHeader = new ACMSTREAMHEADER();
			m_acmHeader->cbStruct = sizeof(ACMSTREAMHEADER);
			m_acmHeader->cbDstLength = this->BufferSizeInBytes;
			m_acmHeader->pbDst = new BYTE[m_acmHeader->cbDstLength];

			int srcSize;
			ThrowOnFailure(acmStreamSize(m_acmStream, m_acmHeader->cbDstLength, (LPDWORD)&srcSize, ACM_STREAMSIZEF_DESTINATION));
			m_acmHeader->cbSrcLength = m_acmHeader->dwSrcUser = srcSize;
			m_acmHeader->pbSrc = new BYTE[m_acmHeader->cbSrcLength];
			m_acmHeader->fdwStatus = m_acmHeader->cbSrcLengthUsed = m_acmHeader->cbDstLengthUsed = 0;
			ThrowOnFailure(acmStreamPrepareHeader(acmStream, m_acmHeader, 0));
		}

		int VoiceRenderClient::OnRender(int count, IntPtr buffer)
		{
			m_acmHeader->cbSrcLength = m_acmHeader->cbSrcLengthUsed = this->OnPacketNeeded((count / 8) * this->VoiceFrameSize, (IntPtr)m_acmHeader->pbSrc);
			ThrowOnFailure(acmStreamConvert(m_acmStream, m_acmHeader, ACM_STREAMCONVERTF_BLOCKALIGN));
			memcpy((void*)buffer, m_acmHeader->pbDst, m_acmHeader->cbDstLengthUsed);
			System::Console::WriteLine((int)m_acmHeader->cbDstLengthUsed / this->FrameSize);
			return m_acmHeader->cbDstLengthUsed / this->FrameSize;
		}

		VoiceRenderClient::~VoiceRenderClient()
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

		VoiceRenderClient::!VoiceRenderClient()
		{
			this->~VoiceRenderClient();
		}
	}
}
