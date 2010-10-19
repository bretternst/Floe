using System;
using System.Collections.Generic;
using System.IO;
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

		private TcpClient _tcpClient;
		private Thread _socketThread;
		private Queue<IrcMessage> _writeQueue;
		private ManualResetEvent _writeWaitHandle;

		public event EventHandler Connected;
		public event EventHandler Disconnected;
        public event EventHandler Heartbeat;
		public event EventHandler<ErrorEventArgs> ConnectionError;
		public event EventHandler<IrcEventArgs> MessageReceived;
		public event EventHandler<IrcEventArgs> MessageSent;

		public IrcConnection(string server, int port, bool isSecure)
		{
			if (string.IsNullOrEmpty(server))
				throw new ArgumentNullException("server");
			if (port <= 0 || port > 65535)
				throw new ArgumentOutOfRangeException("port");

			_server = server;
			_port = port;
			_isSecure = isSecure;
			_writeQueue = new Queue<IrcMessage>();
		}

		public void Open()
		{
			if (_tcpClient != null && _tcpClient.Connected)
			{
				throw new InvalidOperationException("The connection is already open.");
			}

			_writeQueue.Clear();
			_socketThread = new Thread(new ThreadStart(this.SocketLoop));
			_socketThread.Start();
		}

		public void Close()
		{
			if (_socketThread == null || _socketThread.ThreadState != ThreadState.Running)
			{
				return;
			}

			this.QueueMessage(new IrcMessage(null));

			if (!_socketThread.Join(1000))
			{
				_socketThread.Abort();
			}
		}

		public void QueueMessage(string message)
		{
			QueueMessage(IrcMessage.Parse(message));
		}

		public void QueueMessage(IrcMessage message)
		{
			lock (_writeQueue)
			{
				_writeQueue.Enqueue(message);
			}
			if (_writeWaitHandle != null)
			{
				_writeWaitHandle.Set();
			}
		}

		public void Dispose()
		{
			if (_tcpClient != null && _tcpClient.Connected)
			{
				_tcpClient.Close();
			}
		}

		private void SocketLoop()
		{
			_tcpClient = new TcpClient();
			Stream stream = null;
			try
			{
				_tcpClient.Connect(_server, _port);
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
			}
			catch (Exception ex)
			{
				if (_tcpClient.Connected)
				{
					_tcpClient.Close();
				}
				_tcpClient = null;
				this.OnConnectionError(ex);
				this.OnDisconnected();
				return;
			}

			_writeWaitHandle = new ManualResetEvent(false);
			_writeQueue = new Queue<IrcMessage>();

			this.OnConnected();

			byte[] readBuffer = new byte[512], writeBuffer = new byte[Encoding.UTF8.GetMaxByteCount(512)];
			int count = 0, handleIdx = 0;
			var input = new StringBuilder();
			IrcMessage message;
			char last = '\u0000';
			IAsyncResult ar = null;

			try
			{
				while (_tcpClient.Connected)
				{
					if (handleIdx == 0)
					{
						ar = stream.BeginRead(readBuffer, 0, 512, null, null);
					}
					handleIdx = WaitHandle.WaitAny(new[] { ar.AsyncWaitHandle, _writeWaitHandle }, HeartbeatInterval);
					if (!_tcpClient.Connected)
					{
						break;
					}
					switch (handleIdx)
					{
						case 0:
							count = stream.EndRead(ar);
							if (count == 0)
							{
								_tcpClient.Close();
							}
							else
							{
								foreach (char c in Encoding.UTF8.GetChars(readBuffer, 0, count))
								{
									if (c == 0xa && last == 0xd)
									{
										if (input.Length > 0)
										{
											message = IrcMessage.Parse(input.ToString());
											try
											{
												this.OnMessageReceived(message);
											}
#if DEBUG
											catch (IrcException ex)
											{
												System.Diagnostics.Debug.WriteLine("Unhandled IrcException: {0}", ex.Message);
											}
#else
										catch(IrcException)
										{
										}
#endif
											input.Clear();
										}
									}
									else if (c != 0xd && c != 0xa)
									{
										input.Append(c);
									}
									last = c;
								}
							}
							break;
						case 1:
							lock (_writeQueue)
							{
								while (_writeQueue.Count > 0)
								{
									message = _writeQueue.Dequeue();
									if (message.Command == null)
									{
										_tcpClient.Close();
										break;
									}
									string output = message.ToString();
									count = Encoding.UTF8.GetBytes(output, 0, output.Length, writeBuffer, 0);
									count = Math.Min(510, count);
									writeBuffer[count] = 0xd;
									writeBuffer[count + 1] = 0xa;
									stream.Write(writeBuffer, 0, count + 2);

									this.OnMessageSent(message);
								}
							}
							_writeWaitHandle.Reset();
							break;
                        case WaitHandle.WaitTimeout:
							OnHeartbeat();
                            break;
					}
				}
			}
			catch (System.IO.IOException ex)
			{
				this.OnConnectionError(ex);
				if (_tcpClient.Connected)
				{
					_tcpClient.Close();
				}
			}
			this.OnDisconnected();
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

		private void OnConnectionError(Exception ex)
		{
			var handler = this.ConnectionError;
			if (handler != null)
			{
				handler(this, new ErrorEventArgs(ex));
			}
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
