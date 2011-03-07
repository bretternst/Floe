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
		private const int PayloadType = 3;

		private VoiceCodec _codec;
		private VoiceQuality _quality;
		private SynchronizationContext _syncContext;
		private AudioCaptureClient _capture;
		private int _timeStamp;
		private byte[] _packet;
		private Dictionary<IPEndPoint, VoicePeer> _peers;
		private bool _isDisposed = false;
		private float _renderVolume = 1f;
		private int _timeIncrement;
		private VoicePacketPool _pool;

		/// <summary>
		/// Construct a new voice session.
		/// </summary>
		/// <param name="client">An optional already-bound UDP client to use. If this is null, then a new client will be constructed.</param>
		public VoiceSession(VoiceCodec codec, VoiceQuality quality, UdpClient client = null)
			: base(PayloadType, GetPacketSize(codec), client)
		{
			_codec = codec;
			_quality = quality;
			this.InitAudio();
			_packet = new byte[this.PayloadSize];
			_syncContext = SynchronizationContext.Current;
			_peers = new Dictionary<IPEndPoint, VoicePeer>();
			_timeIncrement = GetSamplesPerPacket(VoiceCodec.Gsm610);
			_pool = new VoicePacketPool();
		}

		/// <summary>
		/// Gets or sets the volume at which voice chat renders. This is a value between 0 and 1.
		/// </summary>
		public float RenderVolume
		{
			get { return _renderVolume; }
			set
			{
				_renderVolume = value;
				foreach (var peer in _peers.Values)
				{
					peer.Volume = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the volume at which audio is recorded and sent to peers. This is a value between 0 and 1.
		/// </summary>
		public float CaptureVolume { get { return _capture.Volume; } set { _capture.Volume = value; } }

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

		/// <summary>
		/// Add a peer to the session.
		/// </summary>
		/// <param name="codec">The peer's audio codec.</param>
		/// <param name="quality">The peer's audio quality.</param>
		/// <param name="endpoint">The peer's public endpoint.</param>
		public void AddPeer(VoiceCodec codec, VoiceQuality quality, IPEndPoint endpoint)
		{
			base.AddPeer(endpoint);
			var peer = new VoicePeer(codec, quality, _pool);
			peer.Volume = _renderVolume;
			_peers.Add(endpoint, peer);
		}

		/// <summary>
		/// Remove a peer from the session.
		/// </summary>
		/// <param name="endpoint">The peer's public endpoint.</param>
		public new void RemovePeer(IPEndPoint endpoint)
		{
			if (base.RemovePeer(endpoint))
			{
				var peer = _peers[endpoint];
				_peers.Remove(endpoint);
				peer.Dispose();
			}
		}

		/// <summary>
		/// Sets a peer's mute status.
		/// </summary>
		/// <param name="endpoint">The peer's public endpoint.</param>
		/// <param name="isMuted">True to mute the peer, false to un-mute.</param>
		public void SetPeerMute(IPEndPoint endpoint, bool isMuted)
		{
			if (_peers.ContainsKey(endpoint))
			{
				_peers[endpoint].IsMuted = isMuted;
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
			Marshal.Copy(e.Buffer, _packet, 0, this.PayloadSize);
			this.Send(_timeStamp, _packet);
			_timeStamp += _timeIncrement;
		}

		private void InitAudio()
		{
			if (_capture != null)
			{
				_capture.WritePacket -= capture_WritePacket;
			}
			_capture = new AudioCaptureClient(AudioDevice.DefaultCaptureDevice, this.PayloadSize,
				GetBufferSize(_codec, _quality, false),
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

		/// <summary>
		/// Dispose the voice session.
		/// </summary>
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
				_isDisposed = true;
			}
		}

		~VoiceSession()
		{
			this.Dispose();
		}

		internal static int GetPacketSize(VoiceCodec codec)
		{
			switch (codec)
			{
				case VoiceCodec.Gsm610:
					return 65;
				default:
					throw new ArgumentException("Unsupported codec.");
			}
		}

		internal static int GetSamplesPerPacket(VoiceCodec codec)
		{
			switch (codec)
			{
				case VoiceCodec.Gsm610:
					return 320;
				default:
					throw new ArgumentException("Unsupported codec.");
			}
		}

		internal static int GetSampleRate(VoiceCodec codec, VoiceQuality quality)
		{
			switch (codec)
			{
				case VoiceCodec.Gsm610:
					switch (quality)
					{
						case VoiceQuality.Low:
							return 8000;
						case VoiceQuality.Medium:
							return 11200;
						case VoiceQuality.High:
							return 21760;
						case VoiceQuality.Ultra:
							return 43840;
						default:
							throw new ArgumentException("Unsupported sample rate.");
					}
				default:
					throw new ArgumentException("Unsupported codec.");
			}
		}

		internal static int GetBufferSize(VoiceCodec codec, VoiceQuality quality, bool isRender)
		{
			return isRender ? GetPacketSize(codec) * 2 : GetSamplesPerPacket(codec) * 2 * (44100 / GetSampleRate(codec, quality));
		}

		internal static WaveFormat[] GetConversions(VoiceCodec codec, VoiceQuality quality, bool isRender)
		{
			switch (codec)
			{
				case VoiceCodec.Gsm610:
					int sampleRate = GetSampleRate(codec, quality);
					return isRender ?
						new WaveFormat[] { new WaveFormatGsm610(sampleRate), new WaveFormatPcm(sampleRate, 16, 1) } :
						new WaveFormat[] { new WaveFormatPcm(sampleRate, 16, 1), new WaveFormatGsm610(sampleRate) };
				default:
					throw new ArgumentException("Unsupported codec.");
			}
		}
	}
}
