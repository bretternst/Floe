using System;

using Floe.Interop;

namespace Floe.Audio
{
	class VoicePeer : IDisposable
	{
		private WaveOut _waveOut;
		private JitterBuffer _buffer;
		private VoiceCodec _codec;
		private VoiceQuality _quality;
		private VoicePacketPool _pool;

		public VoicePeer(VoiceCodec codec, VoiceQuality quality, VoicePacketPool pool)
		{
			_codec = codec;
			_quality = quality;
			_buffer = new JitterBuffer();
			_pool = pool;
			this.InitAudio();
		}

		public float Volume { get { return _waveOut.Volume; } set { _waveOut.Volume = value; } }

		public void Enqueue(int seqNumber, int timeStamp, byte[] payload)
		{
			_buffer.Enqueue(_pool.Create(seqNumber, timeStamp, payload));
		}

		private void InitAudio()
		{
			_waveOut = new WaveOut(_buffer, VoiceSession.GetFormat(_codec, _quality), VoiceSession.GetBufferSize(_codec));
			_waveOut.Start();
		}

		public void Dispose()
		{
			_waveOut.Dispose();
		}
	}
}
