using System;
using System.Collections.Concurrent;

namespace Floe.Voice
{
	class VoicePacket : IDisposable
	{
		private static ConcurrentStack<VoicePacket> _pool = new ConcurrentStack<VoicePacket>();

		public int SequenceNumber { get; private set; }
		public int TimeStamp { get; private set; }
		public byte[] Data { get; private set; }

		public static VoicePacket Create(int seqNumber, int timeStamp, byte[] payload)
		{
			VoicePacket packet;
			if (!_pool.TryPop(out packet))
			{
				packet = new VoicePacket();
			}
			packet.SequenceNumber = seqNumber;
			packet.TimeStamp = timeStamp;
			Array.Copy(payload, packet.Data, payload.Length);
			return packet;
		}

		public static void Free()
		{
			_pool.Clear();
		}

		private VoicePacket()
		{
			this.Data = new byte[VoiceSession.PacketSize];
		}

		public void Dispose()
		{
			_pool.Push(this);
		}
	}
}
