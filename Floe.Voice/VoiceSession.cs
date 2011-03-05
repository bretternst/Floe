using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

using Floe.Audio;
using Floe.Net;

namespace Floe.Voice
{
	public sealed class VoiceSession : RtpClient, IDisposable
	{
		internal const int PacketSize = 65;
		internal const int SamplesPerPacket = 320;
		internal const int SampleRate = 17600;
		private const int PayloadType = 3;

		private SynchronizationContext _syncContext;
		private AudioCaptureClient _capture;
		private int _timeStamp;
		private byte[] _packet;
		private Dictionary<IPEndPoint, VoicePeer> _peers;
		private bool _isDisposed = false;

		public VoiceSession(UdpClient client = null)
			: base(PayloadType, PacketSize, client)
		{
			this.InitAudio();
			_packet = new byte[PacketSize];
			_syncContext = SynchronizationContext.Current;
			_peers = new Dictionary<IPEndPoint, VoicePeer>();
		}

		public event EventHandler<ErrorEventArgs> Error;

		public override void Open()
		{
			base.Open();
			_capture.Start();
		}

		protected override void OnReceived(IPEndPoint peer, short payloadType, int seqNumber, int timeStamp, byte[] payload)
		{
			_peers[peer].Enqueue(seqNumber, timeStamp, payload);
			//Console.WriteLine(string.Format("peer={0} type={1} seq={2} time={3} payload={4}",
			//    peer.ToString(), payloadType, seqNumber, timeStamp, payload.Length));
		}

		protected override void OnError(Exception ex)
		{
			this.RaiseEvent(this.Error, new ErrorEventArgs(ex));
		}

		protected override void OnPeerAdded(IPEndPoint endpoint)
		{
			_peers.Add(endpoint, new VoicePeer());
		}

		protected override void OnPeerRemoved(IPEndPoint endpoint)
		{
			var peer = _peers[endpoint];
			_peers.Remove(endpoint);
			peer.Dispose();
		}

		private void capture_WritePacket(object sender, WritePacketEventArgs e)
		{
			Marshal.Copy(e.Buffer, _packet, 0, PacketSize);
			this.Send(_timeStamp, _packet);
			_timeStamp += SamplesPerPacket;
		}

		private void InitAudio()
		{
			if (_capture != null)
			{
				_capture.WritePacket -= capture_WritePacket;
			}
			_capture = new AudioCaptureClient(AudioDevice.DefaultCaptureDevice, PacketSize, SamplesPerPacket * 16,
				new WaveFormatPcm(SampleRate, 16, 1), new WaveFormatGsm610(SampleRate));
			_capture.WritePacket += capture_WritePacket;
		}

		private void RaiseEvent<T>(EventHandler<T> evt, T e) where T : EventArgs
		{
			var action = new Action(() =>
			{
				if (evt != null)
				{
					evt(this, e);
				}
			});
			if (_syncContext != null)
			{
				_syncContext.Post((o) => action(), null);
			}
			else
			{
				action();
			}
		}

		public override void Dispose()
		{
			if (!_isDisposed)
			{
				base.Dispose();
				_capture.Dispose();
				foreach (var peer in _peers.Values)
				{
					peer.Dispose();
				}
				VoicePacket.Free();
				_isDisposed = true;
			}
		}

		~VoiceSession()
		{
			this.Dispose();
		}
	}
}
