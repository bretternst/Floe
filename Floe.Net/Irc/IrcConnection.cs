using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Floe.Net
{
	internal sealed class IrcConnection : IDisposable
	{
        private const int HeartbeatInterval = 300000;
		private string _server;
		private int _port;
		private bool _isSecure;
		private ProxyInfo _proxy;

		private TcpClient _tcpClient;
		private Thread _socketThread;
		private ConcurrentQueue<IrcMessage> _writeQueue;
		private ManualResetEvent _writeWaitHandle;
		private ManualResetEvent _endWaitHandle;
		private SynchronizationContext _syncContext;

		public event EventHandler Connected;
		public event EventHandler Disconnected;
        public event EventHandler Heartbeat;
		public event EventHandler<ErrorEventArgs> Error;
		public event EventHandler<IrcEventArgs> MessageReceived;
		public event EventHandler<IrcEventArgs> MessageSent;

		public IPAddress ExternalAddress { get { return ((IPEndPoint)_tcpClient.Client.LocalEndPoint).Address; } }

		public IrcConnection()
		{
			_syncContext = SynchronizationContext.Current;
		}

		public void Open(string server, int port, bool isSecure, ProxyInfo proxy = null)
		{
			if (string.IsNullOrEmpty(server))
				throw new ArgumentNullException("server");
			if (port <= 0 || port > 65535)
				throw new ArgumentOutOfRangeException("port");

			if (_socketThread != null)
			{
				this.Close();
			}

			_server = server;
			_port = port;
			_isSecure = isSecure;
			_proxy = proxy;
			_writeQueue = new ConcurrentQueue<IrcMessage>();
			_writeWaitHandle = new ManualResetEvent(false);
			_endWaitHandle = new ManualResetEvent(false);
			_socketThread = new Thread(new ThreadStart(this.SocketMain));
			_socketThread.IsBackground = true;
			_socketThread.Start();
		}

		public void Close()
		{
			if (_socketThread != null)
			{
				_endWaitHandle.Set();
				this.OnDisconnected();
				_socketThread = null;
			}
		}

		public void QueueMessage(string message)
		{
			QueueMessage(IrcMessage.Parse(message));
		}

		public void QueueMessage(IrcMessage message)
		{
			if (_writeQueue == null)
			{
				throw new InvalidOperationException("The connection is not open.");
			}

			_writeQueue.Enqueue(message);
			if (_writeWaitHandle != null)
			{
				_writeWaitHandle.Set();
			}
		}

		public void Dispose()
		{
			this.Close();
		}

		private void SocketMain()
		{
			try
			{
				this.SocketLoop();
			}
			catch (IOException ex)
			{
				this.Dispatch(this.OnError, ex);
			}
			catch (SocketException ex)
			{
				this.Dispatch(this.OnError, ex);
			}
			catch (SocksException ex)
			{
				this.Dispatch(this.OnError, ex);
			}
			if (_tcpClient != null && _tcpClient.Connected)
			{
				_tcpClient.Close();
			}
		}

		private void SocketLoop()
		{
			Stream stream = null;

			if (_proxy != null && !string.IsNullOrEmpty(_proxy.ProxyHostname))
			{
				var proxy = new SocksTcpClient(_proxy);
				var ar = proxy.BeginConnect(_server, _port, null, null);
				if (WaitHandle.WaitAny(new[] { ar.AsyncWaitHandle, _endWaitHandle }) == 1)
				{
					return;
				}
				_tcpClient = proxy.EndConnect(ar);
			}
			else
			{
				_tcpClient = new TcpClient();
				var ar = _tcpClient.BeginConnect(_server, _port, null, null);
				if (WaitHandle.WaitAny(new[] { ar.AsyncWaitHandle, _endWaitHandle }) == 1)
				{
					return;
				}
				_tcpClient.EndConnect(ar);
			}
			stream = _tcpClient.GetStream();

			if (_isSecure)
			{
				var sslStream = new SslStream(stream, true,
					(sender, cert, chain, sslPolicyErrors) =>
					{
						// Just accept all server certs for now; we'll take advantage of the encryption
						// but not the authentication unless users ask for it.
						return true;
					});
				sslStream.AuthenticateAsClient(_server);
				stream = sslStream;
			}

			this.Dispatch(this.OnConnected);

			byte[] readBuffer = new byte[512], writeBuffer = new byte[Encoding.UTF8.GetMaxByteCount(512)];
			int count = 0;
			bool gotCR = false;
			var input = new List<byte>(512);
			IrcMessage outgoing = null;
			IAsyncResult arr = null, arw = null;

			while (_tcpClient.Connected)
			{
				if (arr == null)
				{
					arr = stream.BeginRead(readBuffer, 0, 512, null, null);
				}
				_writeWaitHandle.Reset();
				if (arw == null && _writeQueue.TryDequeue(out outgoing))
				{
					string output = outgoing.ToString();
					count = Encoding.UTF8.GetBytes(output, 0, output.Length, writeBuffer, 0);
					count = Math.Min(510, count);
					writeBuffer[count] = 0xd;
					writeBuffer[count + 1] = 0xa;
					arw = stream.BeginWrite(writeBuffer, 0, count + 2, null, null);
				}
				int idx = WaitHandle.WaitAny(
					new[] {
						arr.AsyncWaitHandle,
						arw != null ? arw.AsyncWaitHandle : _writeWaitHandle,
						_endWaitHandle },
					HeartbeatInterval);

				switch (idx)
				{
					case 0:
						count = stream.EndRead(arr);
						arr = null;
						if (count == 0)
						{
							_tcpClient.Close();
						}
						else
						{
							for (int i = 0; i < count; i++)
							{
								switch (readBuffer[i])
								{
									case 0xa:
										if (gotCR)
										{
											var incoming = IrcMessage.Parse(Encoding.UTF8.GetString(input.ToArray()));
											this.Dispatch(this.OnMessageReceived, incoming);
											input.Clear();
										}
										break;
									case 0xd:
										break;
									default:
										input.Add(readBuffer[i]);
										break;
								}
								gotCR = readBuffer[i] == 0xd;
							}
						}
						break;
					case 1:
						if (arw != null)
						{
							stream.EndWrite(arw);
							arw = null;
							this.Dispatch(this.OnMessageSent, outgoing);
						}
						break;
					case 2:
						if (arw != null)
						{
							stream.EndWrite(arw);
						}
						return;
					case WaitHandle.WaitTimeout:
						this.Dispatch(this.OnHeartbeat);
						break;
				}
			}

			this.Dispatch(this.OnDisconnected);
		}

		private void Dispatch<T>(Action<T> action, T arg)
		{
			if (_syncContext != null)
			{
				_syncContext.Post((o) => action((T)o), arg);
			}
			else
			{
				action(arg);
			}
		}

		private void Dispatch(Action action)
		{
			if (_syncContext != null)
			{
				_syncContext.Post((o) => ((Action)o)(), action);
			}
			else
			{
				action();
			}
		}

		private void OnConnected()
		{
			var handler = this.Connected;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		private void OnDisconnected()
		{
			var handler = this.Disconnected;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

        private void OnHeartbeat()
        {
            var handler = this.Heartbeat;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

		private void OnError(Exception ex)
		{
			var handler = this.Error;
			if (handler != null)
			{
				handler(this, new ErrorEventArgs(ex));
			}
			this.Close();
		}

		private void OnMessageReceived(IrcMessage message)
		{
			var handler = this.MessageReceived;
			if (handler != null)
			{
				handler(this, new IrcEventArgs(message));
			}
		}

		private void OnMessageSent(IrcMessage message)
		{
			var handler = this.MessageSent;
			if (handler != null)
			{
				handler(this, new IrcEventArgs(message));
			}
		}
	}
}
