using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Floe.Net
{
	/// <summary>
	/// A base class providing facilities for communicating to one or more peers via the RTP v2 protocol (RTP uses UDP). Classes
	/// that derive from this class will implement their own specific logic utilizing RTP (for example, voice chat).
	/// </summary>
	/// <remarks>
	/// Each packet sent will be sent to all peers. Each peer is identified by a unique key string. It is not guaranteed
	/// that packets will be received in the correct order, nor that all packets will be received at all.
	/// </remarks>
	public abstract class RtpClient
	{
		private UdpClient _client;
		private Dictionary<IPEndPoint, string> _addressToKey;
		private Dictionary<string, IPEndPoint> _keyToAddress;

		public RtpClient(IPEndPoint publicEndPoint)
		{
			_client = new UdpClient(0);
		}

//		public IPEndPoint PublicEndPoint { get { return _publicEndPoint; } }
		public IPEndPoint LocalEndPoint { get { return (IPEndPoint)_client.Client.LocalEndPoint; } }

		public void AddPeer(string key, IPEndPoint endpoint)
		{
		}
	}
}
