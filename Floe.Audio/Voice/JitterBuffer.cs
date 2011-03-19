using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace Floe.Audio
{
	class JitterBuffer : Stream
	{
		private const int JitterBufferSize = 16;

		private ConcurrentQueue<VoicePacket> _incoming;
		private VoicePacket[] _buffer;
		private int _numPackets, _baseSeq;
		private AutoResetEvent _pulse;
		private bool _isBuffering;

		public JitterBuffer()
		{
			_incoming = new ConcurrentQueue<VoicePacket>();
			_buffer = new VoicePacket[JitterBufferSize];
			_pulse = new AutoResetEvent(false);
			_isBuffering = true;
		}

		public void Enqueue(VoicePacket packet)
		{
			_incoming.Enqueue(packet);
			_pulse.Set();
		}

		public VoicePacket Dequeue()
		{
			while (true)
			{
				_pulse.WaitOne();
				VoicePacket newPacket;
				while (_incoming.TryDequeue(out newPacket))
				{
					this.Insert(newPacket);
				}

				if ((!_isBuffering && _numPackets > 0) ||
					(_isBuffering && _numPackets >= _buffer.Length / 2))
				{
					break;
				}
			}

			int i;
			for (i = 0; _buffer[i] == null; i++) ;
			if (i > 0)
			{
				this.RemoveFirst(i - 1);
			}

			var packet = _buffer[0];
			this.RemoveFirst(1);
			return packet;
		}

		private void Insert(VoicePacket packet)
		{
			// empty buffer, add packet to front
			if (_numPackets == 0)
			{
				_buffer[0] = packet;
				_baseSeq = packet.SequenceNumber;
				_numPackets++;
				return;
			}

			// packet is too late
			int idx = packet.SequenceNumber - _baseSeq;
			if (idx < 0)
			{
				return;
			}

			// packet is past the end of buffer, drop packets
			while (idx >= _buffer.Length)
			{
				this.RemoveFirst(Math.Min(idx - _buffer.Length + 1, _buffer.Length));
				idx = packet.SequenceNumber - _baseSeq;
			}

			_buffer[idx] = packet;
			_numPackets++;
		}

		private void RemoveFirst(int num)
		{
			for (int i = 0; i < _buffer.Length; i++)
			{
				if (i < num && _buffer[i] != null)
				{
					_numPackets--;
				}
				if (i < _buffer.Length - num)
				{
					_buffer[i] = _buffer[i + num];
				}
				else
				{
					_buffer[i] = null;
				}
			}
			_baseSeq += num;
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
