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
		private static readonly int[] Mp3BitRates = { 0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 0 };
		private static readonly int[] Mp3SampleRates = { 44100, 48000, 32000, 0 };

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

			var id3 = new byte[(bytes[6] << 21) | (bytes[7] << 14) | (bytes[8] << 7) | bytes[9]];
			_stream.Read(id3, 0, id3.Length);

			_stream.Read(bytes, 0, 4);
			if (bytes[0] != 0xff || (bytes[1] & 0xfe) != 0xfa) // MPEG-1 layer 3
			{
				throw new FileFormatException("Only MPEG-1 Layer 3 is supported.");
			}
			bool isProtected = (bytes[1] & 0x01) > 0;
			int bitRate = Mp3BitRates[bytes[2] >> 4];
			if (bitRate == 0)
			{
				throw new FileFormatException("Unsupported bit rate.");
			}
			int sampleRate = Mp3SampleRates[(bytes[2] & 0xc) >> 2];
			if (sampleRate == 0)
			{
				throw new FileFormatException("Unsupportd sample rate.");
			}
			bool isPadded = (bytes[2] & 0x2) > 0;
			int mode = bytes[3] >> 6;
			_format = new WaveFormatMp3((short)(mode == 2 ? 1 : 2), sampleRate, bitRate * 1000);
			_buffer = new byte[((WaveFormatMp3)_format).BlockSize + 1];
			_stream.Seek(-4, SeekOrigin.Current);
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
			int bitRate = 0, sampleRate = 0;
			if (_stream.Read(_buffer, 0, 4) < 4 ||
				_buffer[0] != 0xff || (_buffer[1] & 0xfe) != 0xfa ||
				(bitRate = Mp3BitRates[_buffer[2] >> 4]) == 0 ||
				(sampleRate = Mp3SampleRates[(_buffer[2] & 0xc) >> 2]) == 0)
			{
				var handler = this.Done;
				if (handler != null)
				{
					handler(this, EventArgs.Empty);
				}
			}
			bool isProtected = (_buffer[1] & 0x01) > 0;
			bool isPadded = (_buffer[2] & 0x2) > 0;
			e.Length = 144 * bitRate * 1000 / sampleRate + (isPadded ? 1 : 0);
			_stream.Read(_buffer, 4, e.Length - 4 + (isProtected ? 16 : 0));
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

		private static void ReverseBytes(byte[] bytes, int start, int end)
		{
			while (start < end)
			{
				byte tmp = bytes[start];
				bytes[start++] = bytes[end];
				bytes[end--] = tmp;
			}
		}
	}
}
