using System;
using System.Runtime.InteropServices;

using Floe.Interop;

namespace Floe.Voice
{
	class VoicePeer : IDisposable
	{
		private AudioRenderClient _client;
		private JitterBuffer _buffer;
		private VoiceCodec _codec;
		private VoiceQuality _quality;

		public VoicePeer(VoiceCodec codec, VoiceQuality quality)
		{
			_codec = codec;
			_quality = quality;
			_buffer = new JitterBuffer();
			this.InitAudio();
		}

		public void Enqueue(int seqNumber, int timeStamp, byte[] payload)
		{
			_buffer.Enqueue(VoicePacket.Create(seqNumber, timeStamp, payload));
		}

		private void InitAudio()
		{
			if (_client != null)
			{
				_client.ReadPacket -= client_ReadPacket;
			}
			_client = new AudioRenderClient(AudioDevice.DefaultRenderDevice, VoiceSession.PacketSize, VoiceSession.PacketSize * 4,
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
				e.HasData = true;
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
