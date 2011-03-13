using System;
using System.Runtime.InteropServices;

using Floe.Interop;

namespace Floe.Audio
{
	class VoicePeer : IDisposable
	{
		private AudioRenderClient _client;
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

		public float Volume { get { return _client.Volume; } set { _client.Volume = value; } }

		public void Enqueue(int seqNumber, int timeStamp, byte[] payload)
		{
			_buffer.Enqueue(_pool.Create(seqNumber, timeStamp, payload));
		}

		private void InitAudio()
		{
			if (_client != null)
			{
				_client.ReadPacket -= client_ReadPacket;
			}
			_client = new AudioRenderClient(AudioDevice.DefaultRenderDevice,
				VoiceSession.GetPacketSize(_codec),
				VoiceSession.GetBufferSize(_codec, _quality, true),
				VoiceSession.GetConversions(_codec, _quality, true));
			_client.ReadPacket += client_ReadPacket;
			_client.Start();
		}

		private void client_ReadPacket(object sender, ReadPacketEventArgs e)
		{
			var packet = _buffer.Dequeue();
			if (packet != null)
			{
				Marshal.Copy(packet.Data, 0, e.Buffer, packet.Data.Length);
				e.Length = packet.Data.Length;
				packet.Dispose();
			}
		}

		public void Dispose()
		{
			_client.Stop();
			_client.Dispose();
		}
	}
}
