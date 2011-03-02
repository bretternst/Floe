#pragma once

namespace Floe
{
	namespace Audio
	{
		using System::IntPtr;

		public ref class WaveFormat
		{
		private:
			WAVEFORMATEX *m_format;

		public:
			WaveFormat(WAVEFORMATEX *format)
			{
				this->Init(format);
			}

			WaveFormat(IntPtr format)
			{
				this->Init((WAVEFORMATEX*)(void*)format);
			}

			property WAVEFORMATEX *Data
			{
				WAVEFORMATEX *get()
				{
					return m_format;
				}
			}

			property IntPtr Handle
			{
				IntPtr get()
				{
					return (IntPtr)m_format;
				}
			}

			property short FormatTag
			{
				short get()
				{
					return m_format->wFormatTag;
				}
			}

			property int SampleRate
			{
				int get()
				{
					return m_format->nSamplesPerSec;
				}
			}

			property short BitsPerSample
			{
				short get()
				{
					return m_format->wBitsPerSample;
				}
			}

			property short Channels
			{
				short get()
				{
					return m_format->nChannels;
				}
			}

			property short FrameSize
			{
				short get()
				{
					return m_format->nBlockAlign;
				}
			}

			property int ByteRate
			{
				int get()
				{
					return m_format->nAvgBytesPerSec;
				}
			}

		protected:
			WaveFormat() {}

			void Init(WAVEFORMATEX *format)
			{
				int size = sizeof(WAVEFORMATEX) + format->cbSize;
				m_format = (WAVEFORMATEX*)malloc(size);
				memcpy(m_format, format, size);
			}

		private:
			~WaveFormat()
			{
				if(m_format != 0)
				{
					delete m_format;
					m_format = 0;
				}
			}

			!WaveFormat()
			{
				this->~WaveFormat();
			}
		};

		public ref class WaveFormatPcm : WaveFormat
		{
		public:
			WaveFormatPcm(int sampleRate, int bitsPerSample, short channels)
			{
				WAVEFORMATEX format;
				format.wFormatTag = WAVE_FORMAT_PCM;
				format.nSamplesPerSec = sampleRate;
				format.wBitsPerSample = bitsPerSample;
				format.nChannels = channels;
				format.cbSize = 0;
				format.nBlockAlign = format.nChannels * format.wBitsPerSample / 8;
				format.nAvgBytesPerSec = format.nSamplesPerSec * format.nBlockAlign;

				this->Init(&format);
			}
		};

		public ref class WaveFormatGsm610 : WaveFormat
		{
		public:
			WaveFormatGsm610(int sampleRate)
			{
				//if(sampleRate != 8000 && sampleRate != 11025 && sampleRate != 22050)
				//{
				//	throw gcnew System::ArgumentException("Invalid sample rate for GSM 6.10");
				//}

				GSM610WAVEFORMAT format;
				format.wfx.wFormatTag = WAVE_FORMAT_GSM610;
				format.wfx.nChannels = 1;
				format.wfx.nSamplesPerSec = sampleRate;
				format.wfx.nAvgBytesPerSec = (sampleRate / 320) * 65;
				format.wfx.nBlockAlign = 65;
				format.wfx.wBitsPerSample = 0;
				format.wfx.cbSize = 2;
				format.wSamplesPerBlock = 320;

				this->Init((WAVEFORMATEX*)&format);
			}

			property short SamplesPerBlock
			{
				short get()
				{
					return ((GSM610WAVEFORMAT*)this->Data)->wSamplesPerBlock;
				}
			}
		};

		public ref class WaveFormatMp3 : WaveFormat
		{
			WaveFormatMp3(short channels, int sampleRate, int bytesPerSec, int blockSize)
			{
				MPEGLAYER3WAVEFORMAT format;
				format.wfx.wFormatTag = WAVE_FORMAT_MPEGLAYER3;
				format.wfx.nChannels = channels;
				format.wfx.nSamplesPerSec = sampleRate;
				format.wfx.nAvgBytesPerSec = bytesPerSec;
				format.wfx.nBlockAlign = 1;
				format.wfx.wBitsPerSample = 0;
				format.wfx.cbSize = MPEGLAYER3_WFX_EXTRA_BYTES;
				format.wID = MPEGLAYER3_ID_MPEG;
				format.fdwFlags = MPEGLAYER3_FLAG_PADDING_OFF;
				format.nBlockSize = blockSize;
				format.nFramesPerBlock = 1;
				format.nCodecDelay = 0;

				this->Init((WAVEFORMATEX*)&format);
			}

			property int FramesPerBlock
			{
				int get()
				{
					return ((MPEGLAYER3WAVEFORMAT*)this->Data)->nFramesPerBlock;
				}
			}
		};
	}
}
