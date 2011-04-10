using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace Floe.Audio
{
	class JitterBuffer : Stream
	{
		private ConcurrentQueue<VoicePacket> _incoming;

		public JitterBuffer()
		{
			_incoming = new ConcurrentQueue<VoicePacket>();
		}

		public void Enqueue(VoicePacket packet)
		{
			_incoming.Enqueue(packet);
		}

		public VoicePacket Dequeue()
		{
			return null;
		}

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
			var packet = this.Dequeue();
			if (count < packet.Data.Length)
			{
				throw new InvalidOperationException("Buffer is too small.");
			}
			count = packet.Data.Length;
			Array.Copy(packet.Data, 0, buffer, offset, count);
			packet.Dispose();
			return count;
		}
	}
}
