using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Floe.Audio
{
	[StructLayout(LayoutKind.Sequential, Pack=2)]
	public class WaveFormat
	{
		private WaveEncoding _formatTag;
		private AudioChannels _channels;
		private int _samplesPerSecond;
		private int _avgBytesPerSecond;
		private short _blockAlign;
		private BitsPerSample _bitsPerSample;
		private short _size;

		public WaveEncoding Encoding { get { return _formatTag; } }
		public AudioChannels Channels { get { return _channels; } }
		public int SampleRate { get { return _samplesPerSecond; } }
		public int BytesPerSecond { get { return _avgBytesPerSecond; } }
		public short BlockAlign { get { return _blockAlign; } }
		public BitsPerSample BitsPerSample { get { return _bitsPerSample; } }
		public int DataSize { get { return _size; } }

		public WaveFormat(AudioChannels channels, int samplesPerSecond, BitsPerSample bitsPerSample)
		{
			_formatTag = WaveEncoding.Pcm;
			_channels = channels;
			_samplesPerSecond = samplesPerSecond;
			_bitsPerSample = bitsPerSample;
			_blockAlign = (short)((int)channels * (int)bitsPerSample / 8);
			_avgBytesPerSecond = samplesPerSecond * (int)_blockAlign;
			_size = 0;
		}

		internal WaveFormat(BinaryReader reader, int size)
		{
			if (size < 16)
			{
				throw new WaveFormatException("WaveFormat data too short.");
			}

			_formatTag = (WaveEncoding)reader.ReadInt16();
			_channels = (AudioChannels)reader.ReadInt16();
			_samplesPerSecond = reader.ReadInt32();
			_avgBytesPerSecond = reader.ReadInt32();
			_blockAlign = reader.ReadInt16();
			_bitsPerSample = (BitsPerSample)reader.ReadInt16();

			if (size > 16)
			{
				_size = reader.ReadInt16();
			}

			if (size > 18)
			{
				throw new WaveFormatException("Structure is too big, use WaveFormatFull instead.");
			}
		}

		internal WaveFormat()
		{
		}
	}
}
