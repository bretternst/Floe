using System;
using System.Runtime.InteropServices;

using Floe.Audio;

namespace Floe.Voice
{
	class VoicePeer : IDisposable
	{
		private AudioRenderClient _client;
		private JitterBuffer _buffer;

		public VoicePeer()
		{
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
				new WaveFormatGsm610(VoiceSession.SampleRate), new WaveFormatPcm(VoiceSession.SampleRate, 16, 1));
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
