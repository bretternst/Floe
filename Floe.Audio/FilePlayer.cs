using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using Floe.Interop;

namespace Floe.Audio
{
	public class FilePlayer : IDisposable
	{
		private const int WavBufferSamples = 800;
		private static readonly byte[] WavFileSignature = { 0x52, 0x49, 0x46, 0x46 }; // RIFF
		private static readonly byte[] Mp3FileSignature = { 0x49, 0x44, 0x33 }; // ID3

		private FileStream _stream;
		private WaveFormat _format;
		private AudioRenderClient _render;
		private byte[] _buffer;

		public event EventHandler Done;

		public FilePlayer(string fileName)
		{
			_stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			var sig = new byte[4];
			try
			{
				_stream.Read(sig, 0, 4);
				_stream.Seek(0, SeekOrigin.Begin);
				if (WavFileSignature.SequenceEqual(sig.Take(WavFileSignature.Length)))
				{
					this.ReadWav();
				}
				else if (Mp3FileSignature.SequenceEqual(sig.Take(Mp3FileSignature.Length)))
				{
					this.ReadMp3();
				}
				else
				{
					throw new FileFormatException("Unrecognized file format.");
				}
			}
			catch (EndOfStreamException)
			{
				throw new FileFormatException("Premature end of file.");
			}
		}

		private void ReadMp3()
		{
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
			int size = ReadFrameHeader(_stream, out channels, out sampleRate, out bitRate);
			_format = new WaveFormatMp3((short)channels, sampleRate, bitRate * 1000);
			_buffer = new byte[((WaveFormatMp3)_format).BlockSize + 1];
			_render = new AudioRenderClient(AudioDevice.DefaultRenderDevice, _buffer.Length, _buffer.Length, _format);
			_render.ReadPacket += ReadPacketMp3;
		}

		private void ReadWav()
		{
			var bytes = new byte[20];
			if (_stream.Read(bytes, 0, bytes.Length) < bytes.Length ||
				BitConverter.ToInt32(bytes, 4) == 0 ||
				BitConverter.ToInt32(bytes, 8) != 0x45564157 || // "WAVE"
				BitConverter.ToInt32(bytes, 12) != 0x20746d66) // "fmt "
			{
				throw new FileFormatException("Badly formed RIFF file.");
			}
			var fmtBytes = new byte[BitConverter.ToInt32(bytes, 16)];
			_stream.Read(fmtBytes, 0, fmtBytes.Length);
			_format = new WaveFormat(fmtBytes);

			while (_stream.Read(bytes, 0, 8) == 8)
			{
				var dataType = BitConverter.ToInt32(bytes, 0);
				var dataSize = BitConverter.ToInt32(bytes, 4);
				if (dataType == 0x61746164)
				{
					_buffer = new byte[WavBufferSamples * _format.FrameSize];
					_render = new AudioRenderClient(AudioDevice.DefaultRenderDevice, _buffer.Length, _buffer.Length, _format);
					_render.ReadPacket += ReadPacketWav;
					return;
				}
				_stream.Seek(dataSize, SeekOrigin.Current);
			}
			throw new FileFormatException("Could not find wave data.");
		}

		public void Start()
		{
			_render.Start();
		}

		public void Stop()
		{
			_render.Stop();
		}

		public void Dispose()
		{
			this.Stop();
			_stream.Dispose();
			_render.Dispose();
		}

		private void ReadPacketWav(object sender, ReadPacketEventArgs e)
		{
			int count = _stream.Read(_buffer, 0, _buffer.Length);
			e.Length = count;
			Marshal.Copy(_buffer, 0, e.Buffer, count);
			if (count == 0)
			{
				var handler = this.Done;
				if (handler != null)
				{
					handler(this, EventArgs.Empty);
				}
			}
		}

		private void ReadPacketMp3(object sender, ReadPacketEventArgs e)
		{
			if ((e.Length = ReadFrameHeader(_stream)) == 0)
			{
				var handler = this.Done;
				if (handler != null)
				{
					handler(this, EventArgs.Empty);
				}
				return;
			}
			_stream.Read(_buffer, 0, e.Length);
			Marshal.Copy(_buffer, 0, e.Buffer, e.Length);
		}

		public static void PlayAsync(string fileName, Action<object> callback = null, object state = null)
		{
			var player = new FilePlayer(fileName);
			player.Done += (sender, e) =>
				{
					if (callback != null)
					{
						callback(state);
					}
					player.Stop();
				};
			player.Start();
		}

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

		private static int ReadFrameHeader(Stream stream)
		{
			int channels, sampleRate, bitRate;
			return ReadFrameHeader(stream, out channels, out sampleRate, out bitRate);
		}

		private static int ReadFrameHeader(Stream stream, out int channels, out int sampleRate, out int bitRate)
		{
			channels = 0;
			sampleRate = 0;
			bitRate = 0;

			int b;
			while (true)
			{
				if ((b = stream.ReadByte()) == -1)
				{
					return 0;
				}
				else if (b != 0xff)
				{
					continue;
				}

				if ((b = stream.ReadByte()) == -1)
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

				if ((b = stream.ReadByte()) == -1)
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

				if ((b = stream.ReadByte()) == -1)
				{
					return 0;
				}
				channels = ((b & 0xc0) >> 6) == 3 ? 1 : 2;

				stream.Seek(-4, SeekOrigin.Current);
				return 144 * bitRate * 1000 / sampleRate + padding;
			}
		}
	}
}
