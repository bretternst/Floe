using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		private const int BufferSize = 2048;
		private const int MinPort = 1024;

		private TcpListener _listener;
		private TcpClient _tcpClient;
		private Thread _socketThread;
		private ManualResetEvent _endHandle;
		private Action<Action> _callback;
		private long _bytesTransferred;
		private NetworkStream _stream;

		public EventHandler Connected;
		public EventHandler Disconnected;
		public EventHandler<ErrorEventArgs> Error;

		public IPAddress Address { get; private set; }
		public int Port { get; private set; }
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

		public DccOperation(Action<Action> callback = null)
		{
			_callback = callback;
			_endHandle = new ManualResetEvent(false);
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

		public bool Write(byte[] data, int offset, int size)
		{
			var ar = _stream.BeginWrite(data, offset, size, null, null);
			int index = WaitHandle.WaitAny(new[] { ar.AsyncWaitHandle, _endHandle });
			switch (index)
			{
				case 0:
					_stream.EndWrite(ar);
					return true;
				default:
					return false;
			}
		}

		public void Close()
		{
			_endHandle.Set();
		}

		/// <summary>
		/// Closes an active DCC session or cancels the listener.
		/// </summary>
		public void Dispose()
		{
			if (_tcpClient != null)
			{
				try
				{
					_tcpClient.Close();
				}
				catch { }
			}
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

		protected void Dispatch<T>(Action<T> handler, T arg)
		{
			if (_callback != null)
			{
				_callback(() => handler(arg));
			}
			else
			{
				handler(arg);
			}
		}

		protected void Dispatch(Action handler)
		{
			if (_callback != null)
			{
				_callback(handler);
			}
			else
			{
				handler();
			}
		}

		protected void RaiseEvent<T>(EventHandler<T> evt, T arg) where T : EventArgs
		{
			if (evt != null)
			{
				if (_callback != null)
				{
					_callback(() => evt(this, arg));
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
				if (_callback != null)
				{
					_callback(() => evt(this, EventArgs.Empty));
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

			try
			{
				while (_tcpClient.Connected)
				{
					var ar = _stream.BeginRead(readBuffer, 0, BufferSize, null, null);
					int idx = WaitHandle.WaitAny(new[] { ar.AsyncWaitHandle, _endHandle });
					switch (idx)
					{
						case 0:
							int count = _stream.EndRead(ar);
							if (count <= 0)
							{
								return;
							}
							this.OnReceived(readBuffer, count);
							break;
						case 1:
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
