using System;
using System.Collections.Concurrent;

namespace Floe.Voice
{
	class VoicePacketPool
	{
		private ConcurrentStack<VoicePacket> _pool = new ConcurrentStack<VoicePacket>();

		public VoicePacketPool()
		{
			_pool = new ConcurrentStack<VoicePacket>();
		}

		public VoicePacket Create(int seqNumber, int timeStamp, byte[] payload)
		{
			VoicePacket packet;
			if (!_pool.TryPop(out packet))
			{
				packet = new VoicePacket(this);
			}
			packet.Init(seqNumber, timeStamp, payload);
			return packet;
		}

		public void Recycle(VoicePacket packet)
		{
			_pool.Push(packet);
		}
	}

	class VoicePacket : IDisposable
	{
		private VoicePacketPool _pool;

		public int SequenceNumber { get; private set; }
		public int TimeStamp { get; private set; }
		public byte[] Data { get; private set; }

		internal VoicePacket(VoicePacketPool pool)
		{
			_pool = pool;
		}

		internal void Init(int seqNumber, int timeStamp, byte[] payload)
		{
			this.SequenceNumber = seqNumber;
			this.TimeStamp = timeStamp;
			if (this.Data == null || this.Data.Length != payload.Length)
			{
				this.Data = new byte[payload.Length];
			}
			Array.Copy(payload, this.Data, payload.Length);
		}

		public void Dispose()
		{
			_pool.Recycle(this);
		}
	}
}
