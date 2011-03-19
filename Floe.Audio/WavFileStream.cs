using System;
using System.IO;

using Floe.Interop;

namespace Floe.Audio
{
	public class WavFileStream : Stream
	{
		private Stream _stream;
		private WaveFormat _format;

		public WavFileStream(string fileName)
			: this(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
		}

		public WavFileStream(Stream stream)
		{
			_stream = stream;
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
					return;
				}
				_stream.Seek(dataSize, SeekOrigin.Current);
			}
			throw new FileFormatException("Could not find wave data.");
		}

		public WaveFormat Format { get { return _format; } }
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
			return _stream.Read(buffer, offset, count);
		}

		public override void Close()
		{
			_stream.Close();
		}
	}
}
