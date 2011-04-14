using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Floe.Interop;

namespace Floe.Audio
{
	/// <summary>
	/// Specifies a codec to use for transmitting voice. Currently, only GSM 6.10 is supported.
	/// </summary>
	public enum VoiceCodec
	{
		/// <summary>
		/// Use the GSM 6.10 codec.
		/// </summary>
		Gsm610
	}

	public class CodecInfo
	{
		private const int Gsm610PayloadType = 3;
		private const int Gsm610SamplesPerBlock = 320;
		private const int Gsm610BytesPerBlock = 65;
		private const int MinBufferLength = 25; // milliseconds

		public int PayloadType { get; private set; }
		public int EncodedBufferSize { get; private set; }
		public int DecodedBufferSize { get; private set; }
		public int SampleRate { get; private set; }
		public int SamplesPerPacket { get; private set; }
		public WaveFormat EncodedFormat { get; private set; }
		public WaveFormat DecodedFormat { get; private set; }

		public CodecInfo(VoiceCodec codec, int sampleRate)
		{
			switch (codec)
			{
				case VoiceCodec.Gsm610:
					this.PayloadType = Gsm610PayloadType;
					this.SampleRate = sampleRate;

					int blocksPerPacket = 1;
					while ((blocksPerPacket * Gsm610SamplesPerBlock * 1000) / sampleRate <= MinBufferLength)
					{
						blocksPerPacket++;
					}

					this.SamplesPerPacket = Gsm610SamplesPerBlock * blocksPerPacket;
					this.EncodedBufferSize = Gsm610BytesPerBlock * blocksPerPacket;
					this.DecodedBufferSize = this.SamplesPerPacket * 2;
					this.EncodedFormat = new WaveFormatGsm610(sampleRate);
					this.DecodedFormat = new WaveFormatPcm(sampleRate, 16, 1);
					break;
				default:
					throw new ArgumentException("Unsupported codec.");
			}
		}

		public AudioConverter GetEncoder()
		{
			return new AudioConverter(this.DecodedBufferSize, this.DecodedFormat, this.EncodedFormat);
		}

		public AudioConverter GetDecoder()
		{
			return new AudioConverter(this.EncodedBufferSize, this.EncodedFormat, this.DecodedFormat);
		}
	}
}
