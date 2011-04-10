using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using Floe.Interop;
using Floe.Net;

namespace Floe.Audio
{
	/// <summary>
	/// Manages a voice session connecting to one or more peers. Recorded audio is encoded and sent to all peers, and received audio is played.
	/// This class uses GSM 6.10 to compress the audio stream.
	/// </summary>
	public sealed class VoiceClient : RtpClient, IDisposable
	{
		private VoiceIn _voiceIn;
		private Dictionary<IPEndPoint, VoicePeer> _peers;
		private float _renderVolume = 1f;
		private VoicePacketPool _pool;
		private ReceivePredicate _receivePredicate;

		/// <summary>
		/// Construct a new voice session.
		/// </summary>
		/// <param name="codec">The transmit codec.</param>
		/// <param name="quality">The transmit quality (usually the sample rate).</param>
		/// <param name="client">An optional already-bound UDP client to use. If this is null, then a new client will be constructed.</param>
		/// <param name="transmitCallback">An optional callback to determine whether to transmit each packet. An application may
		/// use logic such as PTT (push-to-talk) or an automatic peak level-based approach. By default, all packets are transmitted.</param>
		public VoiceClient(CodecInfo codec, UdpClient client = null,
			TransmitPredicate transmitPredicate = null, ReceivePredicate receivePredicate = null)
			: base((byte)codec.PayloadType, codec.EncodedBufferSize, client)
		{
			_peers = new Dictionary<IPEndPoint, VoicePeer>();
			_pool = new VoicePacketPool();
			_receivePredicate = receivePredicate;
			_voiceIn = new VoiceIn(codec, this, transmitPredicate);
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

		public event EventHandler<ErrorEventArgs> Error;

		/// <summary>
		/// Open the voice session and begin sending/receiving data.
		/// </summary>
		public override void Open()
		{
			base.Open();
			_voiceIn.Start();
		}

		/// <summary>
		/// Close the voice session.
		/// </summary>
		public override void Close()
		{
			base.Close();
			_voiceIn.Close();
		}

		/// <summary>
		/// Add a peer to the session.
		/// </summary>
		/// <param name="codec">The peer's audio codec.</param>
		/// <param name="quality">The peer's audio quality (usually the sample rate).</param>
		/// <param name="endpoint">The peer's public endpoint.</param>
		public void AddPeer(VoiceCodec codec, int quality, IPEndPoint endpoint)
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
			var handler = this.Error;
			if (handler != null)
			{
				handler(this, new ErrorEventArgs(ex));
			}
		}

		/// <summary>
		/// Dispose the voice session.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();
			_voiceIn.Dispose();
			foreach (var peer in _peers.Values)
			{
				peer.Dispose();
			}
		}

		~VoiceClient()
		{
			this.Dispose();
		}
	}
}
