using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Floe.Net
{
	/// <summary>
	/// Provides basic functionality for a DCC session, including management of the underlying TCP stream. Other classes
	/// should override this class to implement specific DCC operations (send, chat, etc.)
	/// </summary>
	/// <remarks>
	/// This class can be used to make an outgoing DCC connection or listen for an incoming DCC connection.
	/// </remarks>
	public abstract class DccOperation : IDisposable
	{
		private const int ConnectTimeout = 60 * 1000;
		private const int ListenTimeout = 5 * 60 * 1000;
		private const int BufferSize = 4096;
		private const int MinPort = 1024;

		private TcpListener _listener;
		private TcpClient _tcpClient;
		private Thread _socketThread;
		private ManualResetEvent _endHandle;
		private ConcurrentQueue<Tuple<byte[], int, int>> _writeQueue;
		private ManualResetEvent _writeHandle;
		private SynchronizationContext _syncContext;
		private long _bytesTransferred;
		private NetworkStream _stream;

		public EventHandler Connected;
		public EventHandler Disconnected;
		public EventHandler<ErrorEventArgs> Error;

		/// <summary>
		/// Gets the remote address.
		/// </summary>
		public IPAddress Address { get; private set; }

		/// <summary>
		/// Gets the remote port.
		/// </summary>
		public int Port { get; private set; }

		/// <summary>
		/// Gets the number of bytes transferred. This is typically only relevant for a file transfer operation.
		/// </summary>
		public long BytesTransferred
		{
			get
			{
				return Interlocked.Read(ref _bytesTransferred);
			}
			set
			{
				Interlocked.Exchange(ref _bytesTransferred, value);
			}
		}

		protected DccOperation()
		{
			_syncContext = SynchronizationContext.Current;
			_endHandle = new ManualResetEvent(false);
			_writeHandle = new ManualResetEvent(false);
			_writeQueue = new ConcurrentQueue<Tuple<byte[], int, int>>();
		}

		/// <summary>
		/// Open a DCC session that listens on the specified port. Once a connection is established, there is no difference
		/// between this session and one that started with an outgoing connection.
		/// </summary>
		/// <param name="startPort">The lowest available port to listen on.</param>
		/// <param name="startPort">The highest available port to listen on.</param>
		/// <returns>Returns the actual port number the session is listening on.</returns>
		public int Listen(int lowPort, int highPort)
		{
			if (lowPort > ushort.MaxValue || lowPort < MinPort)
			{
				throw new ArgumentException("lowPort");
			}
			if (highPort > ushort.MaxValue || highPort < lowPort)
			{
				throw new ArgumentException("highPort");
			}

			while(true)
			{
				_listener = new TcpListener(IPAddress.Any, lowPort);
				try
				{
					_listener.Start();
					break;
				}
				catch (SocketException)
				{
					if (++lowPort > ushort.MaxValue)
					{
						throw new InvalidOperationException("No available ports.");
					}
				}
			}

			_socketThread = new Thread(new ThreadStart(() =>
				{
					var ar = _listener.BeginAcceptTcpClient(null, null);
					int index = WaitHandle.WaitAny(new[] { ar.AsyncWaitHandle, _endHandle }, ListenTimeout);
					switch (index)
					{
						case 0:
							try
							{
								_tcpClient = _listener.EndAcceptTcpClient((IAsyncResult)ar);
							}
							catch (SocketException)
							{
								break;
							}
							finally
							{
								_listener.Stop();
							}

							var endpoint = (IPEndPoint)_tcpClient.Client.RemoteEndPoint;
							this.Address = endpoint.Address;
							this.Port = endpoint.Port;
							this.OnConnected();

							try
							{
								this.SocketLoop();
							}
							catch (Exception ex)
							{
								this.OnError(ex);
							}
							break;

						case 1:
							_listener.Stop();
							break;

						case WaitHandle.WaitTimeout:
							_listener.Stop();
							this.OnError(new TimeoutException());
							break;
					}
				}));
			_socketThread.IsBackground = true;
			_socketThread.Start();
			return lowPort;
		}

		/// <summary>
		/// Open a DCC connection that connets to another host.
		/// </summary>
		/// <param name="address">The remote IP address.</param>
		/// <param name="port">The remote port.</param>
		public void Connect(IPAddress address, int port)
		{
			if (port <= 0 || port > ushort.MaxValue)
			{
				throw new ArgumentException("port");
			}
			this.Address = address;
			this.Port = port;

			_tcpClient = new TcpClient();
			_socketThread = new Thread(new ThreadStart(() =>
				{
					var ar = _tcpClient.BeginConnect(address, port, null, null);
					if (ar.AsyncWaitHandle.WaitOne(ConnectTimeout))
					{
						try
						{
							_tcpClient.EndConnect(ar);
							this.OnConnected();
						}
						catch (SocketException ex)
						{
							this.OnError(ex);
							return;
						}

						try
						{
							this.SocketLoop();
						}
						catch(Exception ex)
						{
							this.OnError(ex);
						}
					}
					else
					{
						this.OnError(new TimeoutException());
					}
				}));
			_socketThread.Start();
		}

		/// <summary>
		/// Closes an active DCC session or cancels the listener.
		/// </summary>
		public void Dispose()
		{
			this.Close();
			if (_tcpClient != null)
			{
				_tcpClient.Close();
			}
			if (_listener != null)
			{
				_listener.Stop();
			}
		}

		protected void QueueWrite(byte[] data, int offset, int size)
		{
			_writeQueue.Enqueue(new Tuple<byte[], int, int>(data, offset, size));
			_writeHandle.Set();
		}

		protected void Close()
		{
			_endHandle.Set();
		}

		protected virtual void OnConnected()
		{
			_stream = _tcpClient.GetStream();
			this.RaiseEvent(this.Connected);
		}

		protected virtual void OnDisconnected()
		{
			this.RaiseEvent(this.Disconnected);
		}

		protected virtual void OnError(Exception ex)
		{
			this.RaiseEvent(this.Error, new ErrorEventArgs(ex));
		}

		protected virtual void OnReceived(byte[] buffer, int count)
		{
		}

		protected virtual void OnSent(byte[] buffer, int offset, int count)
		{
		}

		protected void RaiseEvent<T>(EventHandler<T> evt, T arg) where T : EventArgs
		{
			if (evt != null)
			{
				if (_syncContext != null)
				{
					_syncContext.Post((o) => evt(this, (T)o), arg);
				}
				else
				{
					evt(this, arg);
				}
			}
		}

		protected void RaiseEvent(EventHandler evt)
		{
			if (evt != null)
			{
				if (_syncContext != null)
				{
					_syncContext.Post((o) => evt(this, EventArgs.Empty), null);
				}
				else
				{
					evt(this, EventArgs.Empty);
				}
			}
		}

		private void SocketLoop()
		{
			var readBuffer = new byte[BufferSize];
			Tuple<byte[], int, int> outgoing = null;
			IAsyncResult arr = null, arw = null;

			try
			{
				while (_tcpClient.Connected)
				{
					if (arr == null)
					{
						arr = _stream.BeginRead(readBuffer, 0, BufferSize, null, null);
					}
					_writeHandle.Reset();
					if (arw == null && _writeQueue.TryDequeue(out outgoing))
					{
						if (outgoing.Item1 == null)
						{
							break;
						}
						arw = _stream.BeginWrite(outgoing.Item1, outgoing.Item2, outgoing.Item3, null, null);
					}
					int idx = WaitHandle.WaitAny(
						new[] {
							arr.AsyncWaitHandle,
							arw != null ? arw.AsyncWaitHandle : _writeHandle,
							_endHandle
						});
					switch (idx)
					{
						case 0:
							int count = _stream.EndRead(arr);
							arr = null;
							if (count <= 0)
							{
								return;
							}
							this.OnReceived(readBuffer, count);
							break;
						case 1:
							if (arw != null)
							{
								_stream.EndWrite(arw);
								arw = null;
								this.OnSent(outgoing.Item1, outgoing.Item2, outgoing.Item3);
							}
							break;
						case 2:
							if (arw != null)
							{
								_stream.EndWrite(arw);
							}
							return;
					}
				}
			}
			catch (Exception ex)
			{
				this.OnError(ex);
			}
			finally
			{
				_tcpClient.Close();
				this.OnDisconnected();
			}
			return;
		}
	}
}
