using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

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
		private Dispatcher _dispatcher;

		public EventHandler Connected;
		public EventHandler Disconnected;
		public EventHandler<ErrorEventArgs> Error;

		public IPAddress Address { get; private set; }
		public int Port { get; private set; }

		protected NetworkStream Stream { get; private set; }

		public DccOperation(Dispatcher dispatcher = null)
		{
			_dispatcher = dispatcher;
		}

		/// <summary>
		/// Open a DCC session that listens on the specified port. Once a connection is established, there is no difference
		/// between this session and one that started with an outgoing connection.
		/// </summary>
		/// <param name="startPort">The lowest available port to listen on.</param>
		/// <param name="startPort">The highest available port to listen on.</param>
		/// <param name="dispatcher">An optional dispatcher used to route events to the main thread.</param>
		public int Listen(int lowPort, int highPort, Dispatcher dispatcher = null)
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
					if (ar.AsyncWaitHandle.WaitOne(ListenTimeout))
					{
						try
						{
							_tcpClient = _listener.EndAcceptTcpClient((IAsyncResult)ar);
							return;
						}
						catch (SocketException ex)
						{
							this.OnError(ex);
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
			_socketThread.IsBackground = true;
			_socketThread.Start();
			return lowPort;
		}

		/// <summary>
		/// Open a DCC connection that connets to another host.
		/// </summary>
		/// <param name="address">The remote IP address.</param>
		/// <param name="port">The remote port.</param>
		/// <param name="dispatcher">An optional dispatcher used to route events to the main thread.</param>
		public void Connect(IPAddress address, int port, Dispatcher dispatcher = null)
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
		}

		/// <summary>
		/// Closes an active DCC session or cancels the listener.
		/// </summary>
		public void Dispose()
		{
			if (_listener != null)
			{
				try
				{
					_listener.Stop();
				}
				catch { }
			}
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
			this.Stream = _tcpClient.GetStream();
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
			if (_dispatcher != null)
			{
				_dispatcher.BeginInvoke(handler, arg);
			}
			else
			{
				handler(arg);
			}
		}

		protected void Dispatch(Action handler)
		{
			if (_dispatcher != null)
			{
				_dispatcher.BeginInvoke(handler);
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
				evt(this, arg);
			}
		}

		protected void RaiseEvent(EventHandler evt)
		{
			if (evt != null)
			{
				evt(this, EventArgs.Empty);
			}
		}

		private void SocketLoop()
		{
			var readBuffer = new byte[BufferSize];

			while (_tcpClient.Connected)
			{
				var ar = this.Stream.BeginRead(readBuffer, 0, BufferSize, null, null);
				ar.AsyncWaitHandle.WaitOne();
				int count = this.Stream.EndRead(ar);
				if (count == 0)
				{
					break;
				}

				this.OnReceived(readBuffer, count);
			}
			_tcpClient.Close();
			return;
		}
	}
}
