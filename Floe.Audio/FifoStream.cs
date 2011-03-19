using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Floe.Audio
{
	public class FifoStream : Stream
	{
		private const int BlockSize = 8192;

		private LinkedList<byte[]> _blocks;
		private int _readIdx, _writeIdx;
		private ManualResetEventSlim _pulse;
		private bool _isDisposed;

		public FifoStream()
		{
			_blocks = new LinkedList<byte[]>();
			_pulse = new ManualResetEventSlim(false);
		}

		public override bool CanRead { get { return true; } }
		public override bool CanSeek { get { return false; } }
		public override bool CanWrite { get { return true; } }
		public override void Flush() { throw new NotImplementedException(); }
		public override long Length { get { throw new NotImplementedException(); } }
		public override long Position { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		public override long Seek(long offset, SeekOrigin origin) { throw new NotImplementedException(); }
		public override void SetLength(long value) { throw new NotImplementedException(); }

		public override int Read(byte[] buffer, int offset, int count)
		{
			_pulse.Wait();
			if (_isDisposed)
			{
				return 0;
			}
			int total = 0;
			lock (_blocks)
			{
				while (count > 0)
				{
					int written;
					written = Math.Min(count, (_blocks.First == _blocks.Last ? _writeIdx : BlockSize) - _readIdx);
					Array.Copy(_blocks.First.Value, _readIdx, buffer, offset, written);
					count -= written;
					offset += written;
					_readIdx += written;

					if (_readIdx >= BlockSize)
					{
						_blocks.RemoveFirst();
						_readIdx = 0;
					}
					total += written;

					if (_blocks.First == null ||
						(_blocks.First == _blocks.Last && _readIdx >= _writeIdx))
					{
						_pulse.Reset();
						break;
					}
				}
			}
			return total;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (count < 1)
			{
				return;
			}

			lock (_blocks)
			{
				while (count > 0)
				{
					if (_blocks.Last == null || _writeIdx >= BlockSize)
					{
						_blocks.AddLast(new byte[BlockSize]);
						_writeIdx = 0;
					}
					int written = Math.Min(count, BlockSize - _writeIdx);
					Array.Copy(buffer, offset, _blocks.Last.Value, _writeIdx, written);
					count -= written;
					offset += written;
					_writeIdx += written;
					_pulse.Set();
				}
			}
		}

		public override void Close()
		{
			base.Close();
			_isDisposed = true;
			_pulse.Set();
		}
	}
}
