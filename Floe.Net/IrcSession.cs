using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Floe.Net
{
	/// <summary>
	/// Describes all possible states of an IrcSession object.
	/// </summary>
	public enum IrcSessionState
	{
		/// <summary>
		/// The session is in the process of connecting. Either the server connection has not been established yet,
		/// or the user has not been registered.
		/// </summary>
		Connecting,

		/// <summary>
		/// The user has been registered with the IRC server and and has chosen a nickname. Commands can now be accepted.
		/// </summary>
		Connected,

		/// <summary>
		/// The session is not connected to any IRC server.
		/// </summary>
		Disconnected
	}

	/// <summary>
	/// Responsible for creating and maintaining a single IRC session, which consists of a connection to one IRC server. IRC activity is
	/// processed via this class and propagated to consuming objects through events. Methods are exposed to send commands to the IRC server.
	/// </summary>
	public sealed class IrcSession : IDisposable
	{
		private const int ReconnectWaitTime = 5000;

		private string _password;
		private bool _isInvisible;
		private IrcConnection _conn;
		private IrcSessionState _state;
		private List<IrcCodeHandler> _captures;
		private bool _isWaitingForActivity;

		/// <summary>
		/// Gets the server to which the session is connected or will connect.
		/// </summary>
		public string Server { get; private set; }

		/// <summary>
		/// Gets the server port.
		/// </summary>
		public int Port { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the session uses an encrypted (SSL) connection.
		/// </summary>
		public bool IsSecure { get; private set; }

		/// <summary>
		/// Gets the current nickname or the desired nickname if the session is not connected.
		/// </summary>
		public string Nickname { get; private set; }

		/// <summary>
		/// Gets the username reported to the server.
		/// </summary>
		public string Username { get; private set; }

		/// <summary>
		/// Gets the full name reported to the server.
		/// </summary>
		public string FullName { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether the session should automatically attempt to reconnect if it is disconnected.
		/// </summary>
		public bool AutoReconnect { get; set; }

		/// <summary>
		/// Gets the name of the IRC network to which the client is connected. By default, this will simply be the server name but
		/// may be updated when the network name is determined.
		/// </summary>
		public string NetworkName { get; private set; }

		/// <summary>
		/// Gets the current set of user modes that apply to the session.
		/// </summary>
		public char[] UserModes { get; private set; }

		/// <summary>
		/// Gets the current state of the session.
		/// </summary>
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

		/// <summary>
		/// Fires when the state of the session has changed.
		/// </summary>
		public event EventHandler<EventArgs> StateChanged;

		/// <summary>
		/// Fires when a connection error has occurred and the session must close.
		/// </summary>
		public event EventHandler<ErrorEventArgs> ConnectionError;

		/// <summary>
		/// Fires when any message has been received.
		/// </summary>
		public event EventHandler<IrcEventArgs> RawMessageReceived;

		/// <summary>
		/// Fires when any message has been sent.
		/// </summary>
		public event EventHandler<IrcEventArgs> RawMessageSent;

		/// <summary>
		/// Fires when another user has changed their nickname. Nick changes are only visible if the user is on a channel
		/// that the session is currently joined to.
		/// </summary>
		public event EventHandler<IrcNickEventArgs> NickChanged;

		/// <summary>
		/// Fires when the session nickname has changed. This may be a result of a nick change command or a forced nickname change.
		/// </summary>
		public event EventHandler<IrcNickEventArgs> SelfNickChanged;

		/// <summary>
		/// Fires when a private message has been received, either via a channel or directly from another user (a PM).
		/// </summary>
		public event EventHandler<IrcMessageEventArgs> PrivateMessaged;

		/// <summary>
		/// Fires when a notice message has been received, either via a channel or directly from another user.
		/// </summary>
		public event EventHandler<IrcMessageEventArgs> Noticed;

		/// <summary>
		/// Fires when another user has quit.
		/// </summary>
		public event EventHandler<IrcQuitEventArgs> UserQuit;

		/// <summary>
		/// Fires when another user has joined a channel.
		/// </summary>
		public event EventHandler<IrcJoinEventArgs> Joined;

		/// <summary>
		/// Fires when the session joins a channel.
		/// </summary>
		public event EventHandler<IrcJoinEventArgs> SelfJoined;

		/// <summary>
		/// Fires when another user has left a channel.
		/// </summary>
		public event EventHandler<IrcPartEventArgs> Parted;

		/// <summary>
		/// Fires when the session has left a channel.
		/// </summary>
		public event EventHandler<IrcPartEventArgs> SelfParted;

		/// <summary>
		/// Fires when the topic of a channel has been changed.
		/// </summary>
        public event EventHandler<IrcTopicEventArgs> TopicChanged;

		/// <summary>
		/// Fires when the session has been invited to a channel.
		/// </summary>
		public event EventHandler<IrcInviteEventArgs> Invited;

		/// <summary>
		/// Fires when a user has been kicked from a channel.
		/// </summary>
		public event EventHandler<IrcKickEventArgs> Kicked;

		/// <summary>
		/// Fires when the session has been kicked from a channel.
		/// </summary>
		public event EventHandler<IrcKickEventArgs> SelfKicked;

		/// <summary>
		/// Fires when a channel's modes have been changed.
		/// </summary>
		public event EventHandler<IrcChannelModeEventArgs> ChannelModeChanged;

		/// <summary>
		/// Fires when the session's user modes have been changed.
		/// </summary>
		public event EventHandler<IrcUserModeEventArgs> UserModeChanged;

		/// <summary>
		/// Fires when a miscellaneous numeric message was received from the server.
		/// </summary>
		public event EventHandler<IrcInfoEventArgs> InfoReceived;

		/// <summary>
		/// Fires when a CTCP command has been received from another user.
		/// </summary>
		public event EventHandler<CtcpEventArgs> CtcpCommandReceived;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public IrcSession()
		{
			this.State = IrcSessionState.Disconnected;
			this.UserModes = new char[0];
		}

		/// <summary>
		/// Opens the IRC session and attempts to connect to a server.
		/// </summary>
		/// <param name="server">The hostname or IP representation of a server.</param>
		/// <param name="port">The IRC port.</param>
		/// <param name="isSecure">True to use an encrypted (SSL) connection, false to use plain text.</param>
		/// <param name="nickname">The desired nickname.</param>
		/// <param name="userName">The username that will be shown to other users.</param>
		/// <param name="fullname">The full name that will be shown to other users.</param>
		/// <param name="autoReconnect">Indicates whether to automatically reconnect upon disconnection.</param>
		/// <param name="password">The optional password to supply while logging in.</param>
		/// <param name="invisible">Determines whether the +i flag will be set by default.</param>
		public void Open(string server, int port, bool isSecure, string nickname,
			string userName, string fullname, bool autoReconnect, string password = null, bool invisible = false)
		{
			if (string.IsNullOrEmpty(nickname))
			{
				throw new ArgumentNullException("Nickname");
			}
			this.Nickname = nickname;
			this.Server = server;
			this.Port = port;
			_password = password;
			this.IsSecure = isSecure;
			this.Username = userName;
			this.FullName = fullname;
			_isInvisible = invisible;
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
				_conn.Close();
			}

			_captures = new List<IrcCodeHandler>();
			this.State = IrcSessionState.Connecting;
			_conn = new IrcConnection(server, port, isSecure);
			_conn.Connected += new EventHandler(_conn_Connected);
			_conn.Disconnected += new EventHandler(_conn_Disconnected);
			_conn.Heartbeat += new EventHandler(_conn_Heartbeat);
			_conn.MessageReceived += new EventHandler<IrcEventArgs>(_conn_MessageReceived);
			_conn.MessageSent += new EventHandler<IrcEventArgs>(_conn_MessageSent);
			_conn.ConnectionError += new EventHandler<ErrorEventArgs>(_conn_ConnectionError);
			_conn.Open();
		}

		public void Dispose()
		{
			_conn.Close();
		}

		public bool IsSelf(IrcTarget target)
		{
			return target != null && !target.IsChannel &&
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

		public void Quote(string rawText)
		{
			this.Send(new IrcMessage(rawText));
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
				_conn.Close();
			}
		}

		public void Join(string channel)
		{
			this.Send("JOIN", channel);
		}

		public void Join(string channel, string key)
		{
			this.Send("JOIN", channel, key);
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

		public void Mode(string channel, string modeSpec)
		{
			this.Mode(channel, IrcChannelMode.ParseModes(modeSpec));
		}

		public void Mode(IEnumerable<IrcUserMode> modes)
		{
			this.Send("MODE", new IrcTarget(this.Nickname), IrcUserMode.RenderModes(modes));
		}

		public void Mode(string modeSpec)
		{
			this.Mode(IrcUserMode.ParseModes(modeSpec));
		}

		public void Mode(IrcTarget target)
		{
			if (target.IsChannel)
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

        public bool IsSelf(string nick)
        {
            return string.Compare(this.Nickname, nick, StringComparison.OrdinalIgnoreCase) == 0;
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
				Thread.Sleep(ReconnectWaitTime);

				if (this.State == IrcSessionState.Disconnected)
				{
					this.State = IrcSessionState.Connecting;
					_conn.Open();
				}
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
			var args = new IrcNickEventArgs(message);
			var handler = this.NickChanged;
			if (this.IsSelf(args.OldNickname))
			{
				this.Nickname = args.NewNickname;
				handler = this.SelfNickChanged;
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
					handler(this, new IrcMessageEventArgs(message));
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
					handler(this, new IrcMessageEventArgs(message));
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
			var args = new IrcJoinEventArgs(message);
			if (this.IsSelf(args.Who.Nickname))
			{
				handler = this.SelfJoined;
			}
			if (handler != null)
			{
				handler(this, args);
			}
		}

		private void OnPart(IrcMessage message)
		{
			var handler = this.Parted;
			var args = new IrcPartEventArgs(message);
			if (this.IsSelf(args.Who.Nickname))
			{
				handler = this.SelfParted;
			}
			if (handler != null)
			{
				handler(this, args);
			}
		}

		private void OnTopic(IrcMessage message)
		{
			var handler = this.TopicChanged;
			if (handler != null)
			{
				handler(this, new IrcTopicEventArgs(message));
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
			var args = new IrcKickEventArgs(message);
			if (this.IsSelf(args.KickeeNickname))
			{
				handler = this.SelfKicked;
			}
			if (handler != null)
			{
				handler(this, args);
			}
		}

		private void OnMode(IrcMessage message)
		{
			if (message.Parameters.Count > 0)
			{
				if (IrcTarget.IsChannelName(message.Parameters[0]))
				{
					var handler = this.ChannelModeChanged;
					if (handler != null)
					{
						handler(this, new IrcChannelModeEventArgs(message));
					}
				}
				else
				{
					var e = new IrcUserModeEventArgs(message);
					this.UserModes = (from m in e.Modes.Where((newMode) => newMode.Set).Select((newMode) => newMode.Mode).Union(this.UserModes).Distinct()
									  where !e.Modes.Any((newMode) => !newMode.Set && newMode.Mode == m)
									  select m).ToArray();

                    var handler = this.UserModeChanged;
					if (handler != null)
					{
						handler(this, new IrcUserModeEventArgs(message));
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
				if (e.Code == IrcCode.RPL_WELCOME)
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

			_isWaitingForActivity = false;

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
			if (!string.IsNullOrEmpty(_password))
			{
				_conn.QueueMessage(new IrcMessage("PASS", _password));
			}
			_conn.QueueMessage(new IrcMessage("USER", this.Username, _isInvisible ? "4" : "0", "*", this.FullName));
			_conn.QueueMessage(new IrcMessage("NICK", this.Nickname));
		}

		private void _conn_Disconnected(object sender, EventArgs e)
		{
			this.State = IrcSessionState.Disconnected;
		}

		private void _conn_Heartbeat(object sender, EventArgs e)
		{
			if (_isWaitingForActivity)
			{
				_conn.Close();
			}
			else
			{
				_isWaitingForActivity = true;
				this.Send("PING", this.Server);
			}
        }
	}
}
