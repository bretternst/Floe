using System;

namespace Floe.Audio
{
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
