using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;

namespace Floe.Net
{
	internal class IrcConnection : IDisposable
	{
		private static Regex _sendFilter = new Regex("[\u000a\u000d]", RegexOptions.Compiled);

		private string _server;
		private int _port;

		private TcpClient _tcpClient;
		private StreamWriter _wstream;
		private Thread _readThread;
		private Thread _writeThread;
		private Queue<IrcMessage> _writeQueue;
		private AutoResetEvent _writeWaitHandle;
		private int _writePace = 250;

		public int WritePace { get { return _writePace; } set { _writePace = value; } }

		public event EventHandler Connected;
		public event EventHandler Disconnected;
		public event EventHandler<ErrorEventArgs> Error;
		public event EventHandler<IrcEventArgs> MessageReceived;
		public event EventHandler<IrcEventArgs> MessageSent;

		public IrcConnection(string server, int port)
		{
			if (string.IsNullOrEmpty(server))
				throw new ArgumentNullException("server");
			if (port <= 0 || port > 65535)
				throw new ArgumentOutOfRangeException("port");

			_server = server;
			_port = port;
			_writeQueue = new Queue<IrcMessage>();
		}

		public void Open()
		{
			if (_tcpClient != null)
			{
				throw new InvalidOperationException("The connection is already open.");
			}

			_readThread = new Thread(new ThreadStart(this.ReadLoop));
			_readThread.Start();
		}

		public void Close()
		{
			this.Reset();
			this.OnDisconnected();
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
			_writeWaitHandle.Set();
		}

		public void Dispose()
		{
			this.Reset();
		}

		private void SendMessage(IrcMessage message)
		{
			string msg = _sendFilter.Replace(message.ToString(), "\uffff");
			_wstream.WriteLine(msg);
			_wstream.Flush();
			this.OnMessageSent(message);
		}

		private void RecvMessage(string message)
		{
			IrcMessage msg = null;
			if (!string.IsNullOrEmpty(message))
			{
				msg = IrcMessage.Parse(message);
				this.OnMessageReceived(msg);
			}
		}

		private void ReadLoop()
		{
			_tcpClient = new TcpClient();
			try
			{
				_tcpClient.Connect(_server, _port);
			}
			catch (Exception ex)
			{
				this.OnError(ex);
				_tcpClient = null;
				return;
			}

			var rstream = _tcpClient.GetStream();
			rstream.ReadTimeout = 100;
			_wstream = new StreamWriter(rstream, Encoding.ASCII);

			_writeWaitHandle = new AutoResetEvent(false);
			_writeThread = new Thread(new ThreadStart(WriteLoop));
			_writeThread.Start();

			this.OnConnected();

			byte[] buffer = new byte[512];
			var message = new StringBuilder();

			char last = '\u0000';
			while (_tcpClient.Connected)
			{
				if (_tcpClient.Client.Poll(-1, SelectMode.SelectRead))
				{
					int count = rstream.Read(buffer, 0, 512);
					if (count == 0)
					{
						break;
					}
					foreach (char c in Encoding.ASCII.GetChars(buffer, 0, count))
					{
						if (c == 0xa && last == 0xd)
						{
							RecvMessage(message.ToString());
							message.Clear();
						}
						else if (c != 0xd && c != 0xa)
						{
							message.Append(c);
						}
						last = c;
					}
				}
			}
			this.OnDisconnected();
		}

		private void WriteLoop()
		{
			while (_tcpClient.Connected && _tcpClient.Client.Poll(1, SelectMode.SelectWrite))
			{
				_writeWaitHandle.WaitOne(100);
				lock (_writeQueue)
				{
					if (_writeQueue.Count > 0)
					{
						var message = _writeQueue.Dequeue();
						this.SendMessage(message);
					}
				}
				Thread.Sleep(_writePace);
				if (_writeQueue.Count > 0)
				{
					_writeWaitHandle.Set();
				}
			}
		}

		private void Reset()
		{
			if (_tcpClient != null && _tcpClient.Connected)
			{
				_tcpClient.Close();
			}
			_readThread = _writeThread = null;
			_writeQueue.Clear();
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
			Reset();
			var handler = this.Disconnected;
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
