using System;
using System.Collections.Concurrent;

namespace Floe.Voice
{
	class JitterBuffer
	{
		private const int JitterBufferSize = 16;

		private ConcurrentQueue<VoicePacket> _incoming;
		private VoicePacket[] _buffer;
		private int _numPackets, _baseSeq;

		public JitterBuffer()
		{
			_incoming = new ConcurrentQueue<VoicePacket>();
			_buffer = new VoicePacket[JitterBufferSize];
		}

		public void Enqueue(VoicePacket packet)
		{
			_incoming.Enqueue(packet);
		}

		public VoicePacket Dequeue()
		{
			VoicePacket newPacket;
			while (_incoming.TryDequeue(out newPacket))
			{
				this.Insert(newPacket);
			}

			// buffer must be at least half full to play
			if (_numPackets < _buffer.Length / 2)
			{
				return null;
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
	}
}
