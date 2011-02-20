using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Floe.Audio
{
	public class WaveReader : Stream, IDisposable
	{
		private Stream _innerStream;
		private int _headerSize;
		private int _dataSize;

		public WaveReader(Stream stream)
		{
			_innerStream = stream;
			var br = new BinaryReader(stream);

			try
			{
				if (br.ReadInt32() != 0x46464952 || // "RIFF"
					(_headerSize = br.ReadInt32()) == 0 || // total size
					br.ReadInt32() != 0x45564157 || // "WAVE"
					br.ReadInt32() != 0x20746d66) // "fmt "
				{
					throw new WaveFormatException("Not a valid wave file.");
				}

				int fmtSize = br.ReadInt32();
				this.Format = new WaveFormat(br, fmtSize);

				while (true)
				{
					if (br.ReadInt32() == 0x61746164)
					{
						_dataSize = br.ReadInt32();
						break;
					}
					br.ReadBytes(br.ReadInt32());
				}
			}
			catch (EndOfStreamException)
			{
				throw new WaveFormatException("Not a valid wave file.");
			}

			_headerSize = (_headerSize + 8) - _dataSize;
		}

		public WaveReader(string filePath)
			: this(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
		}

		public WaveFormat Format { get; private set; }

		#region Stream interface

		public override bool CanRead { get { return _innerStream.CanRead; } }
		public override bool CanSeek { get { return _innerStream.CanSeek; } }
		public override bool CanTimeout { get { return _innerStream.CanTimeout; } }
		public override bool CanWrite { get { return false; } }
		public override long Length { get { return _dataSize; } }

		public override long Position
		{
			get { return _innerStream.Position - _headerSize; }
			set { _innerStream.Position = _innerStream.Position + _headerSize; }
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					return _innerStream.Seek(offset + _headerSize, origin);
				case SeekOrigin.End:
					return _innerStream.Seek(_headerSize + _dataSize + offset, SeekOrigin.Begin);
				default:
					return _innerStream.Seek(offset, origin);
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _innerStream.Read(buffer, offset, count);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			_innerStream.Dispose();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override void Flush()
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
