using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

using Floe.Interop;
using Floe.Net;

namespace Floe.Audio
{
	/// <summary>
	/// Defines a method signature for a handler that determines whether audio should be encoded and transmitted.
	/// </summary>
	/// <param name="peak">The peak audio level for the current buffer.</param>
	/// <returns>Returns true if transmission should occur, false otherwise.</returns>
	public delegate bool TransmitPredicate(float peak);

	/// <summary>
	/// Defines a method signature for a handler that determines whether to process audio received from a given endpoint.
	/// </summary>
	/// <param name="endpoint">The endpoint from which a packet was received.</param>
	/// <returns>Returns true if the packet should be processed, false otherwise.</returns>
	public delegate bool ReceivePredicate(IPEndPoint endpoint);

	/// <summary>
	/// Manages a voice session connecting to one or more peers. Recorded audio is encoded and sent to all peers, and received audio is played.
	/// This class uses GSM 6.10 to compress the audio stream.
	/// </summary>
	public sealed class VoiceSession : RtpClient, IDisposable
	{
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
		private TransmitPredicate _transmitPredicate;
		private ReceivePredicate _receivePredicate;

		/// <summary>
		/// Construct a new voice session.
		/// </summary>
		/// <param name="codec">The transmit codec.</param>
		/// <param name="quality">The transmit quality.</param>
		/// <param name="client">An optional already-bound UDP client to use. If this is null, then a new client will be constructed.</param>
		/// <param name="transmitCallback">An optional callback to determine whether to transmit each packet. An application may
		/// use logic such as PTT (push-to-talk) or an automatic peak level-based approach. By default, all packets are transmitted.</param>
		public VoiceSession(VoiceCodec codec, VoiceQuality quality, UdpClient client = null,
			TransmitPredicate transmitPredicate = null, ReceivePredicate receivePredicate = null)
			: base(GetPayloadType(codec), GetPacketSize(codec), client)
		{
			_codec = codec;
			_quality = quality;
			_packet = new byte[this.PayloadSize];
			_syncContext = SynchronizationContext.Current;
			_peers = new Dictionary<IPEndPoint, VoicePeer>();
			_timeIncrement = GetSamplesPerPacket(VoiceCodec.Gsm610);
			_pool = new VoicePacketPool();
			_transmitPredicate = transmitPredicate;
			_receivePredicate = receivePredicate;
			this.InitAudio();
		}

		/// <summary>
		/// Gets or sets the volume at which voice chat renders. This is a value between 0 and 1.
		/// </summary>
		public float RenderVolume
		{
			get { return _renderVolume; }
			set
			{
				if (_renderVolume != value)
				{
					_renderVolume = value;
					foreach (var peer in _peers.Values)
					{
						peer.Volume = value;
					}
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

		protected override void OnReceived(IPEndPoint endpoint, short payloadType, int seqNumber, int timeStamp, byte[] payload)
		{
			if (_receivePredicate == null || _receivePredicate(endpoint))
			{
				_peers[endpoint].Enqueue(seqNumber, timeStamp, payload);
			}
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
				_capture.Dispose();
			}
			_capture = new VoiceCaptureClient(AudioDevice.DefaultCaptureDevice, _codec, _quality, _transmitPredicate);
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

		internal static byte GetPayloadType(VoiceCodec codec)
		{
			switch (codec)
			{
				case VoiceCodec.Gsm610:
					return 3;
				default:
					throw new ArgumentException("Unsupported codec.");
			}
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
