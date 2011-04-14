using System;
using System.Collections.Concurrent;

namespace Floe.Audio
{
	class VoicePacketPool
	{
		private ConcurrentStack<VoicePacket> _pool = new ConcurrentStack<VoicePacket>();

		public VoicePacketPool()
		{
			_pool = new ConcurrentStack<VoicePacket>();
		}

		public VoicePacket Create(int seqNumber, int timeStamp, byte[] payload, int count)
		{
			VoicePacket packet;
			if (!_pool.TryPop(out packet))
			{
				packet = new VoicePacket(this);
			}
			packet.Init(seqNumber, timeStamp, payload, count);
			return packet;
		}

		public void Recycle(VoicePacket packet)
		{
			_pool.Push(packet);
		}
	}
}
