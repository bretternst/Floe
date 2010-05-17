using System;
using System.Collections.Generic;
using System.Linq;

namespace Floe.Net
{
	public enum IrcSessionState
	{
		Connecting,
		Connected,
		Disconnected
	}

	public sealed class IrcSession : IDisposable
	{
		private IrcConnection _conn;
		private IrcSessionState _state;
		private List<IrcCodeHandler> _captures;

		public string Server { get; private set; }
		public int Port { get; private set; }
		public string Nickname { get; private set; }
		public string Username { get; private set; }
		public string Hostname { get; private set; }
		public string FullName { get; private set; }
		public bool AutoReconnect { get; set; }
		public string NetworkName { get; private set; }
		public char[] UserModes { get; private set; }

		public IrcSessionState State
		{
			get
			{
				return _state;
			}
			private set
			{
				if (_state != value)
				{
					_state = value;
					this.OnStateChanged();
				}
			}
		}

		public event EventHandler<EventArgs> StateChanged;
		public event EventHandler<ErrorEventArgs> ConnectionError;
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

		public IrcSession()
		{
			this.State = IrcSessionState.Disconnected;
			this.UserModes = new char[0];
		}

		public void Open(string server, int port, string nickname,
			string userName, string hostName, string fullname, bool autoReconnect)
		{
			if (string.IsNullOrEmpty(nickname))
			{
				throw new ArgumentNullException("Nickname");
			}
			this.Nickname = nickname;
			this.Server = server;
			this.Port = port;
			this.Username = userName;
			this.Hostname = hostName;
			this.FullName = fullname;
			this.NetworkName = this.Server;
			this.UserModes = new char[0];
			this.AutoReconnect = autoReconnect;

			if (_conn != null)
			{
				_conn.Connected -= new EventHandler(_conn_Connected);
				_conn.Disconnected -= new EventHandler(_conn_Disconnected);
				_conn.MessageReceived -= new EventHandler<IrcEventArgs>(_conn_MessageReceived);
				_conn.MessageSent -= new EventHandler<IrcEventArgs>(_conn_MessageSent);
				_conn.ConnectionError -= new EventHandler<ErrorEventArgs>(_conn_ConnectionError);
				_conn.WaitForClose();
			}

			_captures = new List<IrcCodeHandler>();
			this.State = IrcSessionState.Connecting;
			_conn = new IrcConnection(server, port);
			_conn.Connected += new EventHandler(_conn_Connected);
			_conn.Disconnected += new EventHandler(_conn_Disconnected);
			_conn.MessageReceived += new EventHandler<IrcEventArgs>(_conn_MessageReceived);
			_conn.MessageSent += new EventHandler<IrcEventArgs>(_conn_MessageSent);
			_conn.ConnectionError += new EventHandler<ErrorEventArgs>(_conn_ConnectionError);
			_conn.Open();
		}

		public void Dispose()
		{
			_conn.WaitForClose();
		}

		public bool IsSelf(IrcTarget target)
		{
			return target != null && target.Type == IrcTargetType.Nickname &&
				string.Compare(target.Name, this.Nickname, StringComparison.OrdinalIgnoreCase) == 0;
		}

		public void Send(IrcMessage message)
		{
			if (this.State != IrcSessionState.Disconnected)
			{
				_conn.QueueMessage(message);
			}
		}

		public void Send(string command, params string[] parameters)
		{
			if (this.State != IrcSessionState.Disconnected)
			{
				_conn.QueueMessage(new IrcMessage(command, parameters));
			}
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
			if (this.State != IrcSessionState.Disconnected)
			{
				this.Send("NICK", newNickname);
			}
			if (this.State != IrcSessionState.Connected)
			{
				this.Nickname = newNickname;
			}
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
			this.AutoReconnect = false;
			if (this.State != IrcSessionState.Disconnected)
			{
				this.Send("QUIT", text);
				_conn.WaitForClose();
			}
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

		public void Topic(string channel)
		{
			this.Send("TOPIC", channel);
		}

		public void Invite(string channel, string nickname)
		{
			this.Send("INVITE", nickname, channel);
		}

		public void Kick(string channel, string nickname)
		{
			this.Send("KICK", channel, nickname);
		}

		public void Kick(string channel, string nickname, string text)
		{
			this.Send("KICK", channel, nickname, text);
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
			this.Send("USERHOST", nicknames);
		}

		public void Mode(string channel, IEnumerable<IrcChannelMode> modes)
		{
			if (!modes.Any())
			{
				this.Send("MODE", new IrcTarget(channel));
				return;
			}

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

		public void Mode(IrcTarget target)
		{
			if (target.Type == IrcTargetType.Channel)
			{
				this.Send("MODE", target);
			}
		}

		public void List(string channels, string target)
		{
			this.Send("LIST", channels, target);
		}

		public void List(string channels)
		{
			this.Send("LIST", channels);
		}

		public void AddHandler(IrcCodeHandler capture)
		{
			lock (_captures)
			{
				_captures.Add(capture);
			}
		}

		public bool RemoveHandler(IrcCodeHandler capture)
		{
			lock (_captures)
			{
				return _captures.Remove(capture);
			}
		}

		private void OnStateChanged()
		{
			var handler = this.StateChanged;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}

			if (this.State == IrcSessionState.Disconnected && this.AutoReconnect)
			{
				this.State = IrcSessionState.Connecting;
				_conn.Open();
			}
		}

		private void OnConnectionError(ErrorEventArgs e)
		{
			var handler = this.ConnectionError;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		private void OnMessageReceived(IrcEventArgs e)
		{
			var handler = this.RawMessageReceived;
			if (handler != null)
			{
				handler(this, e);
			}

#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				System.Diagnostics.Debug.WriteLine(string.Format("RECV: {0}", e.Message.ToString()));
			}
#endif
		}

		private void OnMessageSent(IrcEventArgs e)
		{
			var handler = this.RawMessageSent;
			if (handler != null)
			{
				handler(this, e);
			}

#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				System.Diagnostics.Debug.WriteLine(string.Format("SEND: {0}", e.Message.ToString()));
			}
#endif
		}

		private void OnNickChanged(IrcMessage message)
		{
			var args = new IrcNickEventArgs(message, this.Nickname);
			var handler = this.NickChanged;
			if (args.IsSelf)
			{
				this.Nickname = args.NewNickname;
			}
			if (handler != null)
			{
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
					var e = new IrcUserModeEventArgs(message, this.Nickname);
					if (e.IsSelf)
					{
						this.UserModes = (from m in e.Modes.Where((newMode) => newMode.Set).Select((newMode) => newMode.Mode).Union(this.UserModes).Distinct()
										  where !e.Modes.Any((newMode) => !newMode.Set)
										  select m).ToArray();
					}

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
				var e = new IrcInfoEventArgs(message);
				if (e.Code == IrcCode.Welcome)
				{
					if (e.Text.StartsWith("Welcome to the "))
					{
						var parts = e.Text.Split(' ');
						this.NetworkName = parts[3];
					}
					this.State = IrcSessionState.Connected;
				}

				foreach (var capture in _captures)
				{
					if (capture.Code == e.Code && capture.Handler(message))
					{
						if (capture.AutoRemove)
						{
							_captures.Remove(capture);
						}
						return;
					}
				}

				var handler = this.InfoReceived;
				if (handler != null)
				{
					handler(this, e);
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

		private void _conn_ConnectionError(object sender, ErrorEventArgs e)
		{
			this.OnConnectionError(e);
		}

		private void _conn_MessageSent(object sender, IrcEventArgs e)
		{
			this.OnMessageSent(e);
		}

		private void _conn_MessageReceived(object sender, IrcEventArgs e)
		{
			this.OnMessageReceived(e);

			if (e.Handled)
			{
				return;
			}

			switch (e.Message.Command)
			{
				case "PING":
					if (e.Message.Parameters.Count > 0)
					{
						_conn.QueueMessage("PONG " + e.Message.Parameters[0]);
					}
					else
					{
						_conn.QueueMessage("PONG");
					}
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

		private void _conn_Connected(object sender, EventArgs e)
		{
			_conn.QueueMessage(new IrcMessage("USER", this.Username, this.Hostname, "*", this.FullName));
			_conn.QueueMessage(new IrcMessage("NICK", this.Nickname));
		}

		private void _conn_Disconnected(object sender, EventArgs e)
		{
			this.State = IrcSessionState.Disconnected;
		}
	}
}
