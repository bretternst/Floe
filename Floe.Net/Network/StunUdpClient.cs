using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Floe.Net
{
	/// <summary>
	/// Encapsulates a STUN-related error.
	/// </summary>
	[Serializable]
	public class StunException : Exception
	{
		public StunException()
		{
		}

		public StunException(string message)
			: base(message)
		{
		}
	}

	/// <summary>
	/// Provides facilities for creating a UDP socket binding where the public endpoint for that binding is known. This endpoint can be communicated to
	/// peers so that they know how to communicate with this host. The endpoint is discovered using a STUN v2 server.
	/// </summary>
	public class StunUdpClient
	{
		private const int StunTimeout = 1000;
		private const int MaxTries = 3;
		private const int HeaderLength = 20;
		private const int StunPort = 3478;
		private static readonly byte[] StunCookie = { 0x21, 0x12, 0xa4, 0x42 };

		private class AsyncResult : IAsyncResult
		{
			public object AsyncState { get; private set; }
			public WaitHandle AsyncWaitHandle { get { return this.Event; } }
			public bool CompletedSynchronously { get { return false; } }
			public bool IsCompleted { get { return this.AsyncWaitHandle.WaitOne(0); } }
			public EventWaitHandle Event { get; private set; }
			public Exception Exception { get; set; }
			public IPEndPoint PublicEndPoint { get; set; }
			public UdpClient Client { get; set; }
			public AsyncCallback Callback { get; private set; }

			public AsyncResult(AsyncCallback callback, object state)
			{
				this.Event = new ManualResetEvent(false);
				this.Callback = callback;
				this.AsyncState = state;
			}
		}

		private string[] _stunServers;

		/// <summary>
		/// Construct a new StunUdpClient;
		/// </summary>
		/// <param name="stunServers">The list of stun servers to query. This may be a list of hostnames or IP addresses and may also specify a port separated by a colon.</param>
		public StunUdpClient(params string[] stunServers)
		{
			if (stunServers.Length < 1)
			{
				throw new ArgumentException("At least one STUN server must be specified.");
			}
			_stunServers = stunServers;
		}

		/// <summary>
		/// Begins an asynchronous operation to construct a UDP client and query a STUN server for the corresponding public endpoint to be used with that client.
		/// </summary>
		/// <param name="callback">A handler that will be invoked when the operation is complete.</param>
		/// <param name="state">Application-defined state information to attach to the asynchronous operation.</param>
		/// <returns></returns>
		public IAsyncResult BeginGetClient(AsyncCallback callback = null, object state = null)
		{
			var ar = new AsyncResult(callback, state);
			Task.Factory.StartNew(GetEndPoint, ar);
			return ar;
		}

		/// <summary>
		/// Complete an operation to get the user's public IP address and port number.
		/// </summary>
		/// <param name="ar">The IAsyncResult object provided by BeginGetClient.</param>
		/// <param name="publicEndPoint">Returns the public endpoint that peers may connect to.</param>
		/// <returns>Returns the UDP client associated with the public endpoint.</returns>
		public UdpClient EndGetClient(IAsyncResult asyncResult, out IPEndPoint publicEndPoint)
		{
			var ar = asyncResult as AsyncResult;
			if (ar == null)
			{
				throw new InvalidOperationException("IAsyncResult is not from this operation.");
			}
			ar.AsyncWaitHandle.WaitOne();
			if (ar.Exception != null)
			{
				throw ar.Exception;
			}
			publicEndPoint = ar.PublicEndPoint;
			return ar.Client;
		}

		private void GetEndPoint(object obj)
		{
			var ar = (AsyncResult)obj;

			try
			{
				Loop(ar);
			}
			catch (Exception ex)
			{
				ar.Exception = ex;
				ar.Client.Close();
			}
			finally
			{
				ar.Event.Set();
				if (ar.Callback != null)
				{
					ar.Callback(ar);
				}
			}
		}

		private void Loop(AsyncResult ar)
		{
			ar.Client = new UdpClient(0);
			var bytes = new byte[HeaderLength];
			bytes[1] = 0x01; // MessageType = Request
			Array.Copy(StunCookie, 0, bytes, 4, 4); // Magic Cookie
			var rand = new Random();

			int tries = MaxTries;
			while (--tries >= 0)
			{
				var idBytes = new byte[12];
				rand.NextBytes(idBytes);
				Array.Copy(idBytes, 0, bytes, 8, 12); // Transaction ID

				bool isAnyValid = false;
				foreach (string server in _stunServers)
				{
					if (string.IsNullOrEmpty(server))
					{
						continue;
					}

					try
					{
						var parts = server.Split(':');
						int port = StunPort;
						if (parts.Length > 1 && !int.TryParse(parts[1], out port))
						{
							continue;
						}
						var remote = Dns.GetHostEntry(parts[0]);
						ar.Client.Send(bytes, HeaderLength, new IPEndPoint(remote.AddressList[0], port));
						isAnyValid = true;
					}
					catch (SocketException)
					{
						continue;
					}
				}
				if (!isAnyValid)
				{
					throw new StunException("None of the provided STUN servers could be located.");
				}

				var startTime = DateTime.Now;
				var arr = ar.Client.BeginReceive(null, null);
				while (true)
				{
					if (arr.AsyncWaitHandle.WaitOne(Math.Max(1, StunTimeout - (int)(DateTime.Now - startTime).TotalMilliseconds)))
					{
						var dummy = new IPEndPoint(new IPAddress(0), 0);
						var response = ar.Client.EndReceive(arr, ref dummy);
						if (response.Length > HeaderLength &&
							response[0] == 0x01 && response[1] == 0x01 && // MessageType = Response
							response.Skip(4).Take(4).SequenceEqual(StunCookie) && // Magic Cookie
							response.Skip(8).Take(12).SequenceEqual(idBytes)) // Transaction ID
						{
							int idx = HeaderLength;
							while (idx + 24 < response.Length)
							{
								int attrType = (ushort)response[idx] << 8 | (ushort)response[idx + 1];
								int attrLength = (ushort)response[idx + 2] << 8 | (ushort)response[idx + 3];
								if (attrType == 0x01 && attrLength >= 8) // MAPPED_ADDRESS
								{
									int port = (ushort)response[idx + 6] << 8 | (ushort)response[idx + 7];
									var address = new IPAddress(response.Skip(idx + 8).Take(attrLength - 4).ToArray());
									ar.PublicEndPoint = new IPEndPoint(address, port);
									return;
								}
								idx += 32 + attrLength;
							}
						}
					}
					else
					{
						break;
					}
				}
			}
			throw new StunException("No valid STUN response was received.");
		}
	}
}
