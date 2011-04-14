using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Floe.Interop;

namespace Floe.Audio
{
	class JitterBuffer : Stream
	{
		private const int MaxBufferSize = 200;
		private const int Delay = 1; // number of spans

		private AudioConverter _decoder;
		private ConcurrentQueue<VoicePacket> _incoming;
		private LinkedList<VoicePacket> _buffer;
		private int _timestamp, _span, _lostCount, _delay;
		private bool _reset;

		public JitterBuffer(CodecInfo codec)
		{
			_span = codec.SamplesPerPacket;
			_decoder = codec.GetDecoder();
			_incoming = new ConcurrentQueue<VoicePacket>();
			_buffer = new LinkedList<VoicePacket>();

			this.Reset();
		}

		public void Enqueue(VoicePacket packet)
		{
			_incoming.Enqueue(packet);
		}

		public VoicePacket Dequeue()
		{
			VoicePacket packet;
			while (_incoming.TryDequeue(out packet))
			{
				this.Insert(packet);
			}

			return this.GetPacket();
		}

		private void Insert(VoicePacket packet)
		{
			if (!_reset)
			{
				var node = _buffer.First;
				while (node != null)
				{
					if (packet.TimeStamp + _span <= _timestamp)
					{
						node.Value.Dispose();
						_buffer.RemoveFirst();
					}
					node = node.Next;
				}
			}

			if (_lostCount > 20)
			{
				this.Reset();
			}

			if (_reset || packet.TimeStamp + _span >= _timestamp)
			{
				if (_buffer.Count == MaxBufferSize)
				{
					_buffer.First.Value.Dispose();
					_buffer.RemoveFirst();
				}
				var node = _buffer.First;
				while (node != null && node.Value.TimeStamp <= packet.TimeStamp)
				{
					node = node.Next;
				}
				if (node == null)
				{
					_buffer.AddLast(packet);
				}
				else
				{
					_buffer.AddBefore(node, packet);
				}
			}
		}

		private VoicePacket GetPacket()
		{
			if (_reset)
			{
				var oldest = _buffer.First;
				if (oldest == null)
				{
					return null;
				}
				else
				{
					_reset = false;
					_timestamp = oldest.Value.TimeStamp;
				}
			}

			if (_delay > 0)
			{
				_delay--;
				return null;
			}

			var node = _buffer.First;
			if (node != null && node.Value.TimeStamp > _timestamp + _span)
			{
				node = null;
			}

			if (node != null)
			{
				_lostCount = 0;
				var packet = node.Value;
				_buffer.Remove(node);
				_timestamp = node.Value.TimeStamp + _span;
				return packet;
			}

			_lostCount++;
			_timestamp += _span;
			return null;
		}

		public void Reset()
		{
			while (_buffer.Count > 0)
			{
				var packet = _buffer.First.Value;
				packet.Dispose();
				_buffer.RemoveFirst();
			}
			_lostCount = 0;
			_reset = true;
			_delay = Delay;
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
			if (packet == null)
			{
				Array.Clear(buffer, 0, count);
			}
			else
			{
				count = _decoder.Convert(packet.Data, packet.Data.Length, buffer);
				packet.Dispose();
			}
			return count;
		}
	}
}
