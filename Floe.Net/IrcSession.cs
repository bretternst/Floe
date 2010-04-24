using System;
using System.Linq;
using System.Collections.Generic;

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

		public string Nickname { get; private set; }
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
		public event EventHandler<IrcEventArgs> RawMessageReceived;
		public event EventHandler<IrcEventArgs> RawMessageSent;
		public event EventHandler<IrcNickEventArgs> NickChanged;
		public event EventHandler<IrcDialogEventArgs> PrivateMessaged;
		public event EventHandler<IrcDialogEventArgs> Noticed;
		public event EventHandler<IrcQuitEventArgs> UserQuit;
		public event EventHandler<IrcChannelEventArgs> Joined;
		public event EventHandler<IrcChannelEventArgs> Parted;
		public event EventHandler<IrcChannelEventArgs> TopicChanged;
		public event EventHandler<IrcInviteEventArgs> Invited;
		public event EventHandler<IrcKickEventArgs> Kicked;
		public event EventHandler<IrcChannelModeEventArgs> ChannelModeChanged;
		public event EventHandler<IrcUserModeEventArgs> UserModeChanged;
		public event EventHandler<IrcInfoEventArgs> InfoReceived;
		public event EventHandler<CtcpEventArgs> CtcpCommandReceived;

		public IrcSession(string userName = "none", string hostName = "127.0.0.1", string fullname = "none")
		{
			this.State = IrcSessionState.Disconnected;
			this.UserName = userName;
			this.HostName = hostName;
			this.FullName = fullname;
		}

		public void Open(string server, int port, string Nickname)
		{
			if (this.State != IrcSessionState.Disconnected)
			{
				throw new InvalidOperationException("The IRC session is already active.");
			}

			if (string.IsNullOrEmpty(Nickname))
			{
				throw new ArgumentNullException("Nickname");
			}
			this.Nickname = Nickname;

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

		public bool IsSelf(IrcTarget target)
		{
			return target != null && target.Type == IrcTargetType.Nickname &&
				string.Compare(target.Name, this.Nickname, StringComparison.OrdinalIgnoreCase) == 0;
		}

		public void Send(IrcMessage message)
		{
			_conn.QueueMessage(message);
		}

		public void Send(string command, params string[] parameters)
		{
			_conn.QueueMessage(new IrcMessage(command, parameters));
		}

		public void Send(string command, IrcTarget target, params string[] parameters)
		{
			this.Send(command, (new[] { target.ToString() }).Union(parameters).ToArray());
		}

		public void SendCtcp(IrcTarget target, CtcpCommand command, bool isResponse)
		{
			this.Send(isResponse ? "NOTICE" : "PRIVMSG", target, command.ToString());
		}

		public void Nick(string newNickname)
		{
			this.Send("NICK", newNickname);
		}

		public void PrivateMessage(IrcTarget target, string text)
		{
			this.Send("PRIVMSG", target, text);
		}

		public void Notice(IrcTarget target, string text)
		{
			this.Send("NOTICE", target, text);
		}

		public void Quit(string text)
		{
			this.Send("QUIT", text);
		}

		public void Join(string channel)
		{
			this.Send("JOIN", channel);
		}

		public void Part(string channel)
		{
			this.Send("PART", channel);
		}

		public void Topic(string channel, string topic)
		{
			this.Send("TOPIC", channel, topic);
		}

		public void Invite(string channel, string nickname)
		{
			this.Send("INVITE", nickname, channel);
		}

		public void Kick(string channel, string nickname)
		{
			this.Send("KICK", channel, nickname);
		}

		public void Motd()
		{
			this.Send("MOTD");
		}

		public void Motd(string server)
		{
			this.Send("MOTD", server);
		}

		public void Who(string mask)
		{
			this.Send("WHO", mask);
		}

		public void WhoIs(string mask)
		{
			this.Send("WHOIS", mask);
		}

		public void WhoIs(string target, string mask)
		{
			this.Send("WHOIS", target, mask);
		}

		public void WhoWas(string nickname)
		{
			this.Send("WHOWAS", nickname);
		}

		public void Away(string text)
		{
			this.Send("AWAY", text);
		}

		public void UnAway()
		{
			this.Send("AWAY");
		}

		public void UserHost(params string[] nicknames)
		{
			this.Send("USERHOST", string.Join(" ", nicknames));
		}

		public void Mode(string channel, IEnumerable<IrcChannelMode> modes)
		{
			var enumerator = modes.GetEnumerator();
			var modeChunk = new List<IrcChannelMode>();
			int i = 0;
			while (enumerator.MoveNext())
			{
				modeChunk.Add(enumerator.Current);
				if (++i == 3)
				{
					this.Send("MODE", new IrcTarget(channel), IrcChannelMode.RenderModes(modeChunk));
					modeChunk.Clear();
					i = 0;
				}
			}
			if (modeChunk.Count > 0)
			{
				this.Send("MODE", new IrcTarget(channel), IrcChannelMode.RenderModes(modeChunk));
			}
		}

		public void Mode(string channel, string modes)
		{
			this.Mode(channel, IrcChannelMode.ParseModes(modes));
		}

		public void Mode(IEnumerable<IrcUserMode> modes)
		{
			this.Send("MODE", new IrcTarget(this.Nickname), IrcUserMode.RenderModes(modes));
		}

		public void Mode(string modes)
		{
			this.Mode(IrcUserMode.ParseModes(modes));
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
			var handler = this.RawMessageReceived;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		private void OnMessageSent(IrcEventArgs e)
		{
			var handler = this.RawMessageSent;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		private void OnNickChanged(IrcMessage message)
		{
			var handler = this.NickChanged;
			if (handler != null)
			{
				var args = new IrcNickEventArgs(message, this.Nickname);
				if (args.IsSelf)
				{
					this.Nickname = args.NewNickname;
				}
				handler(this, args);
			}
		}

		private void OnPrivateMessage(IrcMessage message)
		{
			if (message.Parameters.Count > 1 && CtcpCommand.IsCtcpCommand(message.Parameters[1]))
			{
				this.OnCtcpCommand(message);
			}
			else
			{
				var handler = this.PrivateMessaged;
				if (handler != null)
				{
					handler(this, new IrcDialogEventArgs(message));
				}
			}
		}

		private void OnNotice(IrcMessage message)
		{
			if (message.Parameters.Count > 1 && CtcpCommand.IsCtcpCommand(message.Parameters[1]))
			{
				this.OnCtcpCommand(message);
			}
			else
			{
				var handler = this.Noticed;
				if (handler != null)
				{
					handler(this, new IrcDialogEventArgs(message));
				}
			}
		}

		private void OnQuit(IrcMessage message)
		{
			var handler = this.UserQuit;
			if (handler != null)
			{
				handler(this, new IrcQuitEventArgs(message));
			}
		}

		private void OnJoin(IrcMessage message)
		{
			var handler = this.Joined;
			if (handler != null)
			{
				handler(this, new IrcChannelEventArgs(message, this.Nickname));
			}
		}

		private void OnPart(IrcMessage message)
		{
			var handler = this.Parted;
			if (handler != null)
			{
				handler(this, new IrcChannelEventArgs(message, this.Nickname));
			}
		}

		private void OnTopic(IrcMessage message)
		{
			var handler = this.TopicChanged;
			if (handler != null)
			{
				handler(this, new IrcChannelEventArgs(message, this.Nickname));
			}
		}

		private void OnInvite(IrcMessage message)
		{
			var handler = this.Invited;
			if (handler != null)
			{
				handler(this, new IrcInviteEventArgs(message));
			}
		}

		private void OnKick(IrcMessage message)
		{
			var handler = this.Kicked;
			if (handler != null)
			{
				handler(this, new IrcKickEventArgs(message, this.Nickname));
			}
		}

		private void OnMode(IrcMessage message)
		{
			if (message.Parameters.Count > 0)
			{
				if (IrcTarget.IsChannel(message.Parameters[0]))
				{
					var handler = this.ChannelModeChanged;
					if (handler != null)
					{
						handler(this, new IrcChannelModeEventArgs(message));
					}
				}
				else
				{
					var handler = this.UserModeChanged;
					if (handler != null)
					{
						handler(this, new IrcUserModeEventArgs(message, this.Nickname));
					}
				}
			}
		}

		private void OnOther(IrcMessage message)
		{
			int code;
			if (int.TryParse(message.Command, out code))
			{
				var handler = this.InfoReceived;
				if (handler != null)
				{
					handler(this, new IrcInfoEventArgs(message));
				}
			}
		}

		private void OnCtcpCommand(IrcMessage message)
		{
			var handler = this.CtcpCommandReceived;
			if (handler != null)
			{
				handler(this, new CtcpEventArgs(message));
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
						this.Nickname = e.Message.Parameters[0];
						this.State = IrcSessionState.Connected;
						break;
				}
			}
			else if (this.State == IrcSessionState.Connected)
			{
				switch (e.Message.Command)
				{
					case "PING":
						_conn.QueueMessage("PONG");
						break;
					case "NICK":
						this.OnNickChanged(e.Message);
						break;
					case "PRIVMSG":
						this.OnPrivateMessage(e.Message);
						break;
					case "NOTICE":
						this.OnNotice(e.Message);
						break;
					case "QUIT":
						this.OnQuit(e.Message);
						break;
					case "JOIN":
						this.OnJoin(e.Message);
						break;
					case "PART":
						this.OnPart(e.Message);
						break;
					case "TOPIC":
						this.OnTopic(e.Message);
						break;
					case "INVITE":
						this.OnInvite(e.Message);
						break;
					case "KICK":
						this.OnKick(e.Message);
						break;
					case "MODE":
						this.OnMode(e.Message);
						break;
					default:
						this.OnOther(e.Message);
						break;
				}
			}

			this.OnMessageReceived(e);
		}

		private void _conn_Connected(object sender, EventArgs e)
		{
			this.CtcpCommandReceived += new EventHandler<CtcpEventArgs>(IrcSession_CtcpCommand);
			_conn.QueueMessage(new IrcMessage("USER", this.UserName, this.HostName, "*", this.FullName));
			_conn.QueueMessage(new IrcMessage("NICK", this.Nickname));
		}

		private void _conn_Disconnected(object sender, EventArgs e)
		{
			this.State = IrcSessionState.Disconnected;
			this.CtcpCommandReceived -= new EventHandler<CtcpEventArgs>(IrcSession_CtcpCommand);
		}

		private void IrcSession_CtcpCommand(object sender, CtcpEventArgs e)
		{

		}
	}
}
