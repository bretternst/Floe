using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

	public struct StunInfo
	{
		private int _localPort;
		private IPEndPoint _publicEndPoint;

		internal StunInfo(int localPort, IPEndPoint publicEndPoint)
		{
			_localPort = localPort;
			_publicEndPoint = publicEndPoint;
		}

		public int LocalPort { get { return _localPort; } }
		public IPEndPoint PublicEndPoint { get { return _publicEndPoint; } }
	}

	/// <summary>
	/// Provides facilities for discovering a user's public IP address and public UDP port associated with an internal address and port. This class uses the STUN
	/// protocol to query a STUN server for the needed information. Once a public IP address and port are discovered, they can be delivered to peers who will then
	/// communicate directly to that endpoint.
	/// </summary>
	public class StunUdpClient
	{
		private const int StunTimeout = 1000;
		private const int Tries = 3;
		private const int HeaderLength = 20;
		private const int StunPort = 3478;

		private class AsyncResult : IAsyncResult
		{
			public object AsyncState { get; private set; }
			public WaitHandle AsyncWaitHandle { get { return this.Event; } }
			public bool CompletedSynchronously { get { return false; } }
			public bool IsCompleted { get { return this.AsyncWaitHandle.WaitOne(0); } }
			public EventWaitHandle Event { get; private set; }
			public Exception Exception { get; set; }
			public StunInfo Info { get; set; }
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
		/// Begins an asynchronous operation to query the specified STUN servers for the user's public IP address and port for a given UDP port number.
		/// </summary>
		/// <param name="callback">A handler that will be invoked when the operation is complete.</param>
		/// <param name="state">Application-defined state information to attach to the asynchronous operation.</param>
		/// <returns></returns>
		public IAsyncResult BeginGetEndPoint(AsyncCallback callback = null, object state = null)
		{
			var ar = new AsyncResult(callback, state);
			Task.Factory.StartNew(GetEndPoint, ar);
			return ar;
		}

		/// <summary>
		/// Complete an operation to get the user's public IP address and port number.
		/// </summary>
		/// <param name="ar">The IAsyncResult returned from BeginGetEndPoint.</param>
		/// <returns>Returns the user's public IP address and port.</returns>
		public StunInfo EndGetEndPoint(IAsyncResult ar)
		{
			var result = ar as AsyncResult;
			if (result == null)
			{
				throw new InvalidOperationException("IAsyncResult is not from this operation.");
			}
			ar.AsyncWaitHandle.WaitOne();
			if (result.Exception != null)
			{
				throw result.Exception;
			}
			return result.Info;
		}

		private void GetEndPoint(object obj)
		{
			var ar = (AsyncResult)obj;
			var client = new UdpClient(0);
			var bytes = new byte[HeaderLength]
			{
				 0x00, 0x01, // Message Type
				 0x0, 0x0, // Message Length
				 0x21, 0x12, 0xa4, 0x42, // Magic Cookie
				 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, // Transaction ID
			};

			var idBytes = new byte[12];
			var rand = new Random();
			rand.NextBytes(idBytes);
			Array.Copy(idBytes, 0, bytes, 8, 12);

			try
			{
				int tries = Tries;
				while (--tries >= 0)
				{
					bool isAnyValid = false;
					foreach (string server in _stunServers)
					{
						try
						{
							var parts = server.Split(':');
							int port = StunPort;
							if (parts.Length > 1 && !int.TryParse(parts[1], out port))
							{
								continue;
							}
							var remote = Dns.GetHostEntry(parts[0]);
							client.Send(bytes, HeaderLength, new IPEndPoint(remote.AddressList[0], port));
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

					var arr = client.BeginReceive(null, null);
					if (arr.AsyncWaitHandle.WaitOne(StunTimeout))
					{
						var dummy = new IPEndPoint(new IPAddress(0), 0);
						var response = client.EndReceive(arr, ref dummy);
						if (response.Length > HeaderLength && response.Skip(8).Take(12).SequenceEqual(idBytes))
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
									ar.Info = new StunInfo(((IPEndPoint)client.Client.LocalEndPoint).Port, new IPEndPoint(address, port));
									return;
								}
								idx += 32 + attrLength;
							}
						}
					}
					else
					{
						throw new StunException("No STUN response was received.");
					}
				}
			}
			catch (Exception ex)
			{
				ar.Exception = ex;
			}
			finally
			{
				client.Close();
				ar.Event.Set();
				if (ar.Callback != null)
				{
					ar.Callback(ar);
				}
			}
		}
	}
}
