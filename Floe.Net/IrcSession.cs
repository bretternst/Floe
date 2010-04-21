using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.Net
{
	public enum IrcSessionState
	{
		Connecting,
		Connected,
		Disconnecting,
		Disconnected
	}

	public sealed class IrcSession : IDisposable
	{
		private IrcConnection _conn;
		private IrcSessionState _state;

		public string NickName { get; private set; }
		public string UserName { get; private set; }
		public string HostName { get; private set; }
		public string FullName { get; private set; }

		public IrcSessionState State
		{
			get
			{
				return _state;
			}
			private set
			{
				_state = value;
				this.OnStateChanged();
			}
		}

		public event EventHandler<EventArgs> StateChanged;
		public event EventHandler<IrcEventArgs> SelfNickChanged;
		public event EventHandler<IrcEventArgs> OtherNickChanged;
		public event EventHandler<IrcEventArgs> MessageReceived;
		public event EventHandler<IrcEventArgs> MessageSent;

		public IrcSession(string userName = "none", string hostName = "127.0.0.1", string fullname = "none")
		{
			this.State = IrcSessionState.Disconnected;
			this.UserName = userName;
			this.HostName = hostName;
			this.FullName = fullname;
		}

		public void Open(string server, int port, string nickName)
		{
			if (this.State != IrcSessionState.Disconnected)
			{
				throw new InvalidOperationException("The IRC session is already active.");
			}

			if (string.IsNullOrEmpty(nickName))
			{
				throw new ArgumentNullException("nickName");
			}
			this.NickName = nickName;

			this.State = IrcSessionState.Connecting;
			_conn = new IrcConnection(server, port);
			_conn.Connected += new EventHandler(_conn_Connected);
			_conn.Disconnected += new EventHandler(_conn_Disconnected);
			_conn.MessageReceived += new EventHandler<IrcEventArgs>(_conn_MessageReceived);
			_conn.MessageSent += new EventHandler<IrcEventArgs>(_conn_MessageSent);
			_conn.Open();
		}

		public void Close()
		{
			if (this.State != IrcSessionState.Connected)
			{
				throw new InvalidOperationException("The IRC session is not connected.");
			}

			_conn.Close();
			this.State = IrcSessionState.Disconnecting;
		}

		public void Dispose()
		{
			this.Close();
		}

		private void OnStateChanged()
		{
			var handler = this.StateChanged;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		private void OnMessageReceived(IrcEventArgs e)
		{
			var handler = this.MessageReceived;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		private void OnMessageSent(IrcEventArgs e)
		{
			var handler = this.MessageSent;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		private void _conn_MessageSent(object sender, IrcEventArgs e)
		{
			this.OnMessageSent(e);
		}

		private void _conn_MessageReceived(object sender, IrcEventArgs e)
		{
			if (this.State == IrcSessionState.Connecting)
			{
				switch (e.Message.Command)
				{
					case "NICK":
						this.NickName = e.Message.Parameters[0];
						this.State = IrcSessionState.Connected;
						break;
				}
			}
			else if (this.State == IrcSessionState.Connected)
			{
				if (e.Message.Command == "NICK")
				{
					if (e.Message.From is IrcPeer && ((IrcPeer)e.Message.From).NickName == this.NickName)
					{
						this.NickName = e.Message.Parameters[0];
					}
				}
			}

			this.OnMessageReceived(e);
		}

		private void _conn_Connected(object sender, EventArgs e)
		{
			_conn.QueueMessage(new IrcMessage("USER", this.UserName, this.HostName, "*", this.FullName));
			_conn.QueueMessage(new IrcMessage("NICK", this.NickName));
		}

		private void _conn_Disconnected(object sender, EventArgs e)
		{
			this.State = IrcSessionState.Disconnected;
		}
	}
}
