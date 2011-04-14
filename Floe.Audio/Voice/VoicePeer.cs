using System;

using Floe.Interop;

namespace Floe.Audio
{
	class VoicePeer : IDisposable
	{
		private WaveOut _waveOut;
		private JitterBuffer _buffer;
		private CodecInfo _codec;
		private VoicePacketPool _pool;

		public VoicePeer(VoiceCodec codec, int quality, VoicePacketPool pool)
		{
			_codec = new CodecInfo(codec, quality);
			_buffer = new JitterBuffer(_codec);
			_pool = pool;
			this.InitAudio();
		}

		public float Volume { get { return _waveOut.Volume; } set { _waveOut.Volume = value; } }

		public void Enqueue(int seqNumber, int timeStamp, byte[] payload, int count)
		{
			_buffer.Enqueue(_pool.Create(seqNumber, timeStamp, payload, count));
		}

		private void InitAudio()
		{
			_waveOut = new WaveOut(_buffer, _codec.DecodedFormat, _codec.DecodedBufferSize);
			_waveOut.Start();
		}

		public void Dispose()
		{
			_waveOut.Dispose();
		}
	}
}
