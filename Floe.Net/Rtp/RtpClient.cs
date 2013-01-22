using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Floe.Net
{
	/// <summary>
	/// A base class providing facilities for communicating to one or more peers via the RTP v2 protocol (RTP uses UDP). Classes
	/// that derive from this class will implement their own specific logic utilizing RTP (for example, voice chat).
	/// </summary>
	/// <remarks>
	/// Each packet sent will be sent to all peers. Any packet received from an unrecognized peer will be discarded.
	/// </remarks>
	public abstract class RtpClient : IDisposable
	{
		private const int HeaderSize = 12;
		private const int KeepAliveInterval = 10000;
		private const int MaxPayloadSize = 1024;

		private UdpClient _client;
		private HashSet<IPEndPoint> _peers;
		private Dictionary<IPEndPoint, int> _seq;
		private ManualResetEvent _endEvent, _readyEvent;
		private Thread _thread;
		private byte[] _payload;
		private int _payloadSize;
		private IAsyncResult[] _sendResults;
		private WaitHandle[] _sendHandles;
		private byte[] _sendBuffer;
		private uint _seqNumber;
		private byte[] _ssrc, _keepalive;
		private byte _payloadType;
		private IPEndPoint _keepAliveTarget;

		/// <summary>
		/// Constructs a new RcpClient using the specified UdpClient for communication.
		/// </summary>
		/// <param name="payloadType">An RTP payload type identifier.</param>
		/// <param name="payloadSize">The size of the payload for both send and receive.</param>
		/// <param name="keepAliveTarget">An address to send keepalive packets to when no peers are connected.</param>
		/// <param name="client">An optional already-bound UdpClient to use for communication.</param>
		public RtpClient(byte payloadType, int payloadSize, IPEndPoint keepAliveTarget, UdpClient client = null)
		{
			_payloadType = (byte)(payloadType & 0x7f);
			_client = client ?? new UdpClient(0);
			_peers = new HashSet<IPEndPoint>();
			_payload = new byte[MaxPayloadSize];
			_payloadSize = payloadSize;
			_ssrc = new byte[4];
			_seq = new Dictionary<IPEndPoint, int>();
			new Random().NextBytes(_ssrc);
			_keepAliveTarget = keepAliveTarget;
			_keepalive = new byte[] { 0xff, 0xff };
		}

		/// <summary>
		/// Gets the local endpoint that the client is bound to.
		/// </summary>
		public IPEndPoint LocalEndPoint { get { return (IPEndPoint)_client.Client.LocalEndPoint; } }

		/// <summary>
		/// Gets the payload size of outgoing packets.
		/// </summary>
		public int PayloadSize { get { return _payloadSize; } }

		/// <summary>
		/// Begin an RTP session.
		/// </summary>
		public virtual void Open()
		{
			_endEvent = new ManualResetEvent(false);
			_readyEvent = new ManualResetEvent(false);
			_thread = new Thread(new ThreadStart(ThreadProc));
			_thread.Start();
			_readyEvent.WaitOne();
		}

		/// <summary>
		/// Close the RTP session.
		/// </summary>
		public virtual void Close()
		{
			if (_thread != null)
			{
				_endEvent.Set();
				_thread.Join();
			}
		}

		/// <summary>
		/// Send a packet to all peers. If there is a problem with the send, the OnError method is called.
		/// </summary>
		/// <param name="timeStamp">The packet's timestamp.</param>
		/// <param name="payload">The packet's payload.</param>
		public virtual void Send(int timeStamp, byte[] payload)
		{
			if (_peers.Count < 1)
			{
				return;
			}

			if (_sendHandles == null || _sendHandles.Length != _peers.Count)
			{
				_sendHandles = new WaitHandle[_peers.Count];
				_sendResults = new IAsyncResult[_peers.Count];
			}
			if(_sendBuffer == null)
			{
				_sendBuffer = new byte[HeaderSize + _payloadSize];
			}

			_sendBuffer[0] = 0x80;
			_sendBuffer[1] = _payloadType;
			_sendBuffer[2] = (byte)(_seqNumber >> 8);
			_sendBuffer[3] = (byte)(_seqNumber);
			_sendBuffer[4] = (byte)(timeStamp >> 24);
			_sendBuffer[5] = (byte)(timeStamp >> 16);
			_sendBuffer[6] = (byte)(timeStamp >> 8);
			_sendBuffer[7] = (byte)(timeStamp);
			Array.Copy(_ssrc, 0, _sendBuffer, 8, 4);
			Array.Copy(payload, 0, _sendBuffer, 12, _payloadSize);

			_seqNumber++;

			int i = 0;
			foreach (var peer in _peers)
			{
				_sendResults[i] = _client.Client.BeginSendTo(_sendBuffer, 0, _sendBuffer.Length, SocketFlags.None, peer, null, null);
				_sendHandles[i] = _sendResults[i].AsyncWaitHandle;
				i++;
			}
			WaitHandle.WaitAll(_sendHandles);
			try
			{
				foreach (var ar in _sendResults)
				{
					_client.Client.EndSendTo(ar);
				}
			}
			catch (SocketException ex)
			{
				this.OnError(ex);
			}
		}

		private void SendKeepAlive(IPEndPoint endpoint)
		{
			_client.Client.BeginSendTo(_keepalive, 0, _keepalive.Length, SocketFlags.None, endpoint, (ar) =>
				{
					try
					{
						_client.Client.EndSendTo(ar);
					}
					catch (SocketException ex)
					{
						this.OnError(ex);
					}
				}, null);
		}

		/// <summary>
		/// Dispose and close this object.
		/// </summary>
		public virtual void Dispose()
		{
			this.Close();
		}

		/// <summary>
		/// Add a new peer. Packets will be delivered to and accepted from this peer.
		/// </summary>
		/// <param name="endpoint">The public endpoint of the peer to add.</param>
		protected void AddPeer(IPEndPoint endpoint)
		{
			if (!_peers.Contains(endpoint))
			{
				_peers.Add(endpoint);
			}
			this.SendKeepAlive(endpoint);
		}

		/// <summary>
		/// Remove a peer. Packets will no longer be delivered to or accepted from this peer.
		/// </summary>
		/// <param name="endpoint">The public endpoint of the peer to add.</param>
		/// <returns>Returns true if the peer was removed, or false if the peer had not been added.</returns>
		protected bool RemovePeer(IPEndPoint endpoint)
		{
			return _peers.Remove(endpoint);
		}

		/// <summary>
		/// When overridden in a dervied class, this method provides handling for a received packet.
		/// This method is called from a worker thread.
		/// </summary>
		/// <param name="peer">The peer from which the packet was received.</param>
		/// <param name="payloadType">The numeric payload type.</param>
		/// <param name="seqNumber">The sequence number generated by the peer, used to detect packet loss.</param>
		/// <param name="timeStamp">The packet's timestamp.</param>
		/// <param name="source">The packet's source stream identifier.</param>
		/// <param name="payload">The packet's payload.</param>
		/// <param name="count">The size of the payload in bytes.</param>
		protected abstract void OnReceived(IPEndPoint peer, short payloadType, int seqNumber, int timeStamp, byte[] payload, int count);

		/// <summary>
		/// When overridden in a dervied class, this method handles errors that occur during asynchronous operations.
		/// This method is called from a worker thread.
		/// </summary>
		/// <param name="ex">The exception that occurred.</param>
		protected abstract void OnError(Exception ex);

		private void ThreadProc()
		{
			try
			{
				this.Loop();
			}
			catch(SocketException ex)
			{
				this.OnError(ex);
			}
		}

		private void Loop()
		{
			var handles = new WaitHandle[] { null, _endEvent };
			var buffer = new byte[HeaderSize + MaxPayloadSize];

			while (true)
			{
				EndPoint sender = new IPEndPoint(IPAddress.Any, 0);
				var arr = _client.Client.BeginReceiveFrom(buffer, 0, HeaderSize + MaxPayloadSize, SocketFlags.None, ref sender, null, null);
				handles[0] = arr.AsyncWaitHandle;
				_readyEvent.Set();
				switch (WaitHandle.WaitAny(handles, KeepAliveInterval))
				{
					case 0:
						int count = 0;
						try
						{
							count = _client.Client.EndReceiveFrom(arr, ref sender);
						}
						catch (SocketException ex)
						{
							// Ignore packets that are too large.
							if (ex.ErrorCode == 10040)
							{
								continue;
							}
							else
							{
								throw;
							}
						}
						var endpoint = (IPEndPoint)sender;
						if (_peers.Contains(endpoint))
						{
							this.ReadPacket(endpoint, buffer, count);
						}
						break;
					case 1:
						return;
					case WaitHandle.WaitTimeout:
						if (_peers.Count == 0)
						{
							this.SendKeepAlive(_keepAliveTarget);
						}
						else
						{
							foreach (var peer in _peers)
							{
								this.SendKeepAlive(peer);
							}
						}
						break;
				}
			}
		}

		private void ReadPacket(IPEndPoint peer, byte[] buffer, int count)
		{
			if (buffer[0] != 0x80 || (buffer[1] & 0x80) != 0 || count <= HeaderSize)
			{
				return;
			}

			short payloadType = (short)(buffer[1] & 0x7f);
			ushort seq = (ushort)((buffer[2] << 8) | buffer[3]);
			int timeStamp = (int)((buffer[4] << 24) | (buffer[5] << 16) | (buffer[6] << 8) | buffer[7]);
			int source = (int)((buffer[8] << 24) | (buffer[9] << 16) | (buffer[10] << 8) | buffer[11]);
			Array.Copy(buffer, 12, _payload, 0, count - HeaderSize);

			// The RTP protocol only allocates 16 bits for the sequence number, which means it may "wrap around".
			// Detect that and append an upper 16 bits.
			if (!_seq.ContainsKey(peer))
			{
				_seq.Add(peer, 0);
			}

			int oldSeq = _seq[peer];
			ushort seqLow = (ushort)(oldSeq);
			ushort seqHigh = (ushort)(oldSeq >> 16);
			int newSeq;

			if (seqHigh > 0 && seqLow < 0x4000 && seq > 0x7fff)
			{
				newSeq = ((seqHigh - 1) << 16) | seq;
			}
			else
			{
				if (seqLow > 0x7fff && seq < 0x4000)
				{
					seqHigh++;
				}
				newSeq = (seqHigh << 16) | seq;
			}

			if (newSeq > oldSeq)
			{
				_seq[peer] = newSeq;
			}

			this.OnReceived(peer, payloadType, newSeq, timeStamp, _payload, count - HeaderSize);
		}

		~RtpClient()
		{
			this.Close();
		}
	}
}
