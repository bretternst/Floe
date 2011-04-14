using System;

namespace Floe.Audio
{
	class VoicePacket : IDisposable
	{
		private VoicePacketPool _pool;

		public int SequenceNumber { get; private set; }
		public int TimeStamp { get; set; }
		public byte[] Data { get; private set; }

		internal VoicePacket(VoicePacketPool pool)
		{
			_pool = pool;
		}

		internal void Init(int seqNumber, int timeStamp, byte[] payload, int count)
		{
			this.SequenceNumber = seqNumber;
			this.TimeStamp = timeStamp;
			if (this.Data == null || this.Data.Length != count)
			{
				this.Data = new byte[count];
			}
			Array.Copy(payload, this.Data, count);
		}

		public void Dispose()
		{
			_pool.Recycle(this);
		}
	}
}
