using System;
using System.IO;

using Floe.Interop;

namespace Floe.Audio
{
	public class Mp3FileStream : Stream
	{
		private static readonly short[, ,] MpegBitRates = new short[2, 3, 16]
		{
			{
				{ 0, 32, 64, 96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448, 0 },
				{ 0, 32, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 384, 0 },
				{ 0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 0 }
			},
			{
				{ 0, 32, 48, 56, 64, 80, 96, 112, 128, 144, 160, 176, 192, 224, 256, 0 },
				{ 0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 0 },
				{ 0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 0 }
			}
		};

		private static readonly int[,] MpegSampleRates = new int[3, 4]
		{
			{
				44100, 48000, 32000, 0
			},
			{
				22050, 24000, 16000, 0
			},
			{
				11025, 12000, 8000, 0
			}
		};

		private Stream _stream;
		private WaveFormatMp3 _format;

		public Mp3FileStream(string fileName)
			: this(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
		}

		public Mp3FileStream(Stream stream)
		{
			_stream = stream;
			var bytes = new byte[10];
			_stream.Read(bytes, 0, 10);
			if (bytes[3] != 0x03 || bytes[4] != 0x00) // v2.3.0
			{
				throw new FileFormatException("Unsupported ID3 version.");
			}
			if (bytes[5] != 0x00)
			{
				throw new FileFormatException(string.Format("Unsupported ID3 flags ({0})", bytes[5]));
			}

			_stream.Seek((bytes[6] << 21) | (bytes[7] << 14) | (bytes[8] << 7) | bytes[9], SeekOrigin.Current);

			int channels, bitRate, sampleRate;
			int size = this.ReadFrameHeader(out channels, out sampleRate, out bitRate);
			_format = new WaveFormatMp3((short)channels, sampleRate, bitRate * 1000);
		}

		public WaveFormatMp3 Format { get { return _format; } }
		public override bool CanRead { get { return true; } }
		public override bool CanSeek { get { return false; } }
		public override bool CanWrite { get { return false; } }
		public override void Flush() { throw new NotImplementedException(); }
		public override long Length { get { throw new NotImplementedException(); } }
		public override long Position { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		public override long Seek(long offset, SeekOrigin origin) { throw new NotImplementedException(); }
		public override void SetLength(long value) { throw new NotImplementedException(); }
		public override void Write(byte[] buffer, int offset, int count) { throw new NotImplementedException(); }

		public override int Read(byte[] buffer, int offset, int count)
		{
			int blockSize = this.ReadFrameHeader();
			int numRead = _stream.Read(buffer, offset, Math.Min(count, blockSize));
			if (blockSize > count)
			{
				_stream.Seek(blockSize - count, SeekOrigin.Current);
			}
			return numRead;
		}

		public override void Close()
		{
			_stream.Close();
		}

		private int ReadFrameHeader()
		{
			int channels, sampleRate, bitRate;
			return this.ReadFrameHeader(out channels, out sampleRate, out bitRate);
		}

		private int ReadFrameHeader(out int channels, out int sampleRate, out int bitRate)
		{
			channels = 0;
			sampleRate = 0;
			bitRate = 0;

			int b;
			while (true)
			{
				if ((b = _stream.ReadByte()) == -1)
				{
					return 0;
				}
				else if (b != 0xff)
				{
					continue;
				}

				if ((b = _stream.ReadByte()) == -1)
				{
					return 0;
				}
				if ((b & 0xe0) != 0xe0)
				{
					continue;
				}

				int version = (b & 0x18) >> 3;
				if (version == 1)
				{
					continue;
				}
				switch (version)
				{
					case 0:
						version = 2;
						break;
					case 2:
						version = 1;
						break;
					case 3:
						version = 0;
						break;
				}
				int layer = (b & 0x6) >> 1;
				if (layer == 0)
				{
					continue;
				}
				switch (layer)
				{
					case 1:
						layer = 2;
						break;
					case 2:
						layer = 1;
						break;
					case 3:
						layer = 0;
						break;
				}

				if ((b = _stream.ReadByte()) == -1)
				{
					return 0;
				}
				bitRate = MpegBitRates[version > 0 ? 1 : 0, layer, (b >> 4)];
				if (bitRate == 0)
				{
					continue;
				}

				sampleRate = MpegSampleRates[version, (b & 0xc) >> 2];
				if (sampleRate == 0)
				{
					continue;
				}
				int padding = (b & 0x2) > 0 ? 1 : 0;

				if ((b = _stream.ReadByte()) == -1)
				{
					return 0;
				}
				channels = ((b & 0xc0) >> 6) == 3 ? 1 : 2;

				_stream.Seek(-4, SeekOrigin.Current);
				return 144 * bitRate * 1000 / sampleRate + padding;
			}
		}
	}
}
