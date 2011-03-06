using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

using Floe.Interop;
using Floe.Net;

namespace Floe.Voice
{
	/// <summary>
	/// Manages a voice session connecting to one or more peers. Recorded audio is encoded and sent to all peers, and received audio is played.
	/// This class uses GSM 6.10 to compress the audio stream.
	/// </summary>
	public sealed class VoiceSession : RtpClient, IDisposable
	{
		internal const int PacketSize = 65;
		internal const int SamplesPerPacket = 320;
		internal const int SampleRate = 21760;
		private const int PayloadType = 3;

		private VoiceCodec _codec;
		private VoiceQuality _quality;
		private SynchronizationContext _syncContext;
		private AudioCaptureClient _capture;
		private int _timeStamp;
		private byte[] _packet;
		private Dictionary<IPEndPoint, VoicePeer> _peers;
		private bool _isDisposed = false;

		/// <summary>
		/// Construct a new voice session.
		/// </summary>
		/// <param name="client">An optional already-bound UDP client to use. If this is null, then a new client will be constructed.</param>
		public VoiceSession(VoiceCodec codec, VoiceQuality quality, UdpClient client = null)
			: base(PayloadType, PacketSize, client)
		{
			_codec = codec;
			_quality = quality;
			this.InitAudio();
			_packet = new byte[PacketSize];
			_syncContext = SynchronizationContext.Current;
			_peers = new Dictionary<IPEndPoint, VoicePeer>();
		}

		/// <summary>
		/// Fires whenever an error occurs that shuts down the voice session.
		/// </summary>
		public event EventHandler<ErrorEventArgs> Error;

		/// <summary>
		/// Open the voice session and begin sending/receiving data.
		/// </summary>
		public override void Open()
		{
			base.Open();
			_capture.Start();
		}

		/// <summary>
		/// Close the voice session.
		/// </summary>
		public override void Close()
		{
			base.Close();
			_capture.Stop();
		}

		public void AddPeer(VoiceCodec codec, VoiceQuality quality, IPEndPoint endpoint)
		{
			base.AddPeer(endpoint);
			_peers.Add(endpoint, new VoicePeer(codec, quality));
		}

		public new void RemovePeer(IPEndPoint endpoint)
		{
			if (base.RemovePeer(endpoint))
			{
				var peer = _peers[endpoint];
				_peers.Remove(endpoint);
				peer.Dispose();
			}
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
			_capture = new AudioCaptureClient(AudioDevice.DefaultCaptureDevice, PacketSize,
				SamplesPerPacket * 2 * (44100 / SampleRate),
				VoiceSession.GetConversions(_codec, _quality, false));
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

		internal static WaveFormat[] GetConversions(VoiceCodec codec, VoiceQuality quality, bool isRender)
		{
			switch (codec)
			{
				case VoiceCodec.Gsm610:
					int sampleRate = 0;
					switch (quality)
					{
						case VoiceQuality.Low:
							sampleRate = 8000;
							break;
						case VoiceQuality.Medium:
							sampleRate = 11200;
							break;
						case VoiceQuality.High:
							sampleRate = 21760;
							break;
						case VoiceQuality.Ultra:
							sampleRate = 43840;
							break;
						default:
							throw new ArgumentException("Unsupported sample rate.");
					}
					return isRender ?
						new WaveFormat[] { new WaveFormatGsm610(sampleRate), new WaveFormatPcm(sampleRate, 16, 1) } :
						new WaveFormat[] { new WaveFormatPcm(sampleRate, 16, 1), new WaveFormatGsm610(sampleRate) };
				default:
					throw new ArgumentException("Unsupported codec.");
			}
		}
	}
}
