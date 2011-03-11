using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
		private bool _findExternalAddress;
		private SynchronizationContext _syncContext;
		private Timer _reconnectTimer;

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
		/// Gets the internal IP address of the computer running the session. This is the private IP address that is used behind
		/// a NAT firewall.
		/// </summary>
		public IPAddress InternalAddress { get; private set; }

		/// <summary>
		/// Gets the external IP address of the computer running the session. The IRC server is queried to retrieve the address or hostname.
		/// If a hostname is returned, the IP address is retrieved via DNS. If no external address can be found, the local IP address
		/// is provided.
		/// </summary>
		public IPAddress ExternalAddress { get; private set; }

		/// <summary>
		/// Gets or sets proxy information, identifying the SOCKS5 proxy server to use when connecting to a server.
		/// </summary>
		public ProxyInfo Proxy { get; set; }

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
			_syncContext = SynchronizationContext.Current;

			_conn = new IrcConnection();
			_conn.Connected += new EventHandler(_conn_Connected);
			_conn.Disconnected += new EventHandler(_conn_Disconnected);
			_conn.Heartbeat += new EventHandler(_conn_Heartbeat);
			_conn.MessageReceived += new EventHandler<IrcEventArgs>(_conn_MessageReceived);
			_conn.MessageSent += new EventHandler<IrcEventArgs>(_conn_MessageSent);
			_conn.Error += new EventHandler<ErrorEventArgs>(_conn_ConnectionError);
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
		/// <param name="findExternalAddress">Determines whether to find the external IP address by querying the IRC server upon connect.</param>
		public void Open(string server, int port, bool isSecure, string nickname,
			string userName, string fullname, bool autoReconnect, string password = null, bool invisible = false, bool findExternalAddress = true,
			ProxyInfo proxy = null)
		{
			if (string.IsNullOrEmpty(nickname))
			{
				throw new ArgumentNullException("Nickname");
			}
			_password = password;
			_isInvisible = invisible;
			_findExternalAddress = findExternalAddress;
			this.Nickname = nickname;
			this.Server = server;
			this.Port = port;
			this.IsSecure = isSecure;
			this.Username = userName;
			this.FullName = fullname;
			this.NetworkName = this.Server;
			this.UserModes = new char[0];
			this.AutoReconnect = autoReconnect;
			this.Proxy = proxy;

			_captures = new List<IrcCodeHandler>();
			_conn.Open(server, port, isSecure, this.Proxy);
			this.State = IrcSessionState.Connecting;
		}

		/// <summary>
		/// Disposes the session, closing any open connection.
		/// </summary>
		public void Dispose()
		{
			if (_conn != null)
			{
				_conn.Close();
			}
		}

		/// <summary>
		/// Determine whether the specified target refers to this session by comparing the nickname to the session's current nickname.
		/// </summary>
		/// <param name="target">The target to evaluate.</param>
		/// <returns>True if the target refers to this session, false otherwise.</returns>
		public bool IsSelf(IrcTarget target)
		{
			return target != null && !target.IsChannel &&
				string.Compare(target.Name, this.Nickname, StringComparison.OrdinalIgnoreCase) == 0;
		}

		/// <summary>
		/// Determines whether the specified nickname matches the session's current nickname.
		/// </summary>
		/// <param name="nick">The nickname to evaluate.</param>
		/// <returns>True if the nickname matches the session's current nickname, false otherwise.</returns>
		public bool IsSelf(string nick)
		{
			return string.Compare(this.Nickname, nick, StringComparison.OrdinalIgnoreCase) == 0;
		}

		/// <summary>
		/// Send a message to the server.
		/// </summary>
		/// <param name="message">The message to send.</param>
		public void Send(IrcMessage message)
		{
			if (this.State != IrcSessionState.Disconnected)
			{
				_conn.QueueMessage(message);
			}
		}

		/// <summary>
		/// Send a message to the server.
		/// </summary>
		/// <param name="command">The name of the command.</param>
		/// <param name="parameters">The optional command parameters.</param>
		public void Send(string command, params string[] parameters)
		{
			if (this.State != IrcSessionState.Disconnected)
			{
				_conn.QueueMessage(new IrcMessage(command, parameters));
			}
		}

		/// <summary>
		/// Send a message to the server.
		/// </summary>
		/// <param name="command">The name of the command.</param>
		/// <param name="target">The target of the command.</param>
		/// <param name="parameters">The optional command parameters.</param>
		public void Send(string command, IrcTarget target, params string[] parameters)
		{
			this.Send(command, (new[] { target.ToString() }).Union(parameters).ToArray());
		}

		/// <summary>
		/// Send a CTCP message to another client.
		/// </summary>
		/// <param name="target">The user to which the CTCP command will be delivered.</param>
		/// <param name="command">The CTCP command to send.</param>
		/// <param name="isResponse">Indicates whether the CTCP message is a response to a command that was received. This parameter
		/// is important for preventing an infinite back-and-forth loop between two clients.</param>
		public void SendCtcp(IrcTarget target, CtcpCommand command, bool isResponse)
		{
			this.Send(isResponse ? "NOTICE" : "PRIVMSG", target, command.ToString());
		}

		/// <summary>
		/// Send the raw text to the server.
		/// </summary>
		/// <param name="rawText">The raw text to send. This should be in the format of a standard IRC message, per RFC 2812.</param>
		public void Quote(string rawText)
		{
			this.Send(new IrcMessage(rawText));
		}

		/// <summary>
		/// Change the nickname.
		/// </summary>
		/// <param name="newNickname">The new nickname.</param>
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

		/// <summary>
		/// Send a private message to a user or channel.
		/// </summary>
		/// <param name="target">The user or channel that the message will be delivered to.</param>
		/// <param name="text">The message text.</param>
		public void PrivateMessage(IrcTarget target, string text)
		{
			this.Send("PRIVMSG", target, text);
		}

		/// <summary>
		/// Send a notice to a user or channel.
		/// </summary>
		/// <param name="target">The user or channel that the notice will be delivered to.</param>
		/// <param name="text">The notice text.</param>
		public void Notice(IrcTarget target, string text)
		{
			this.Send("NOTICE", target, text);
		}

		/// <summary>
		/// Quit from the server and close the connection.
		/// </summary>
		/// <param name="text">The optional quit text.</param>
		public void Quit(string text)
		{
			this.AutoReconnect = false;
			if (this.State != IrcSessionState.Disconnected)
			{
				this.Send("QUIT", text);
				_conn.Close();
			}
		}

		/// <summary>
		/// Join a channel.
		/// </summary>
		/// <param name="channel">The name of the channel to join.</param>
		public void Join(string channel)
		{
			this.Send("JOIN", channel);
		}

		/// <summary>
		/// Join a channel.
		/// </summary>
		/// <param name="channel">The name of the channel to join.</param>
		/// <param name="key">The key required to join the channel.</param>
		public void Join(string channel, string key)
		{
			this.Send("JOIN", channel, key);
		}

		/// <summary>
		/// Part (leave) a channel.
		/// </summary>
		/// <param name="channel">The channel to leave.</param>
		public void Part(string channel)
		{
			this.Send("PART", channel);
		}

		/// <summary>
		/// Change the topic on a channel. The session must have the appropriate permissions on the channel.
		/// </summary>
		/// <param name="channel">The channel on which to set a new topic.</param>
		/// <param name="topic">The topic text.</param>
		public void Topic(string channel, string topic)
		{
			this.Send("TOPIC", channel, topic);
		}

		/// <summary>
		/// Request the existing topic for a channel.
		/// </summary>
		/// <param name="channel">The channel on which the topic should be retrieved.</param>
		public void Topic(string channel)
		{
			this.Send("TOPIC", channel);
		}

		/// <summary>
		/// Invite another user to a channel. The session must have the appropriate permissions on the channel.
		/// </summary>
		/// <param name="channel">The channel to which the user will be invited.</param>
		/// <param name="nickname">The nickname of the user to invite.</param>
		public void Invite(string channel, string nickname)
		{
			this.Send("INVITE", nickname, channel);
		}

		/// <summary>
		/// Kick a user from a channel. The session must have ops in the channel.
		/// </summary>
		/// <param name="channel">The channel from which to kick the user.</param>
		/// <param name="nickname">The nickname of the user to kick.</param>
		public void Kick(string channel, string nickname)
		{
			this.Send("KICK", channel, nickname);
		}

		/// <summary>
		/// Kick a user from a channel. The session must have ops in the channel.
		/// </summary>
		/// <param name="channel">The channel from which to kick the user.</param>
		/// <param name="nickname">The nickname of the user to kick.</param>
		/// <param name="text">The kick text, typically describing the reason for kicking a user.</param>
		public void Kick(string channel, string nickname, string text)
		{
			this.Send("KICK", channel, nickname, text);
		}

		/// <summary>
		/// Request the server MOTD (message of the day).
		/// </summary>
		public void Motd()
		{
			this.Send("MOTD");
		}

		/// <summary>
		/// Request a server MOTD (message of the day).
		/// </summary>
		/// <param name="server">The name of the server from which to request the MOTD.</param>
		public void Motd(string server)
		{
			this.Send("MOTD", server);
		}

		/// <summary>
		/// Execute the WHO command, retrieving basic information on users.
		/// </summary>
		/// <param name="mask">The wildcard to search for, matching nickname, hostname, server, and full name.</param>
		public void Who(string mask)
		{
			this.Send("WHO", mask);
		}

		/// <summary>
		/// Retrieve information about a user.
		/// </summary>
		/// <param name="target">The nickname of the user to retrieve information about. Wildcards may or may not be supported.</param>
		public void WhoIs(string mask)
		{
			this.Send("WHOIS", mask);
		}

		/// <summary>
		/// Retrieve information about a user.
		/// </summary>
		/// <param name="target">The sever to which the request should be routed (or the nickname of the user to route the request to
		/// his server).</param>
		/// <param name="target">The nickname of the user to retrieve information about. Wildcards may or may not be supported.</param>
		public void WhoIs(string target, string mask)
		{
			this.Send("WHOIS", target, mask);
		}

		/// <summary>
		/// Retrieve information about a user who has previously logged off. This will typically indicate when the user was last seen.
		/// </summary>
		/// <param name="nickname">The nickname of the user.</param>
		public void WhoWas(string nickname)
		{
			this.Send("WHOWAS", nickname);
		}

		/// <summary>
		/// Mark this session "away" so that users receive an automated response when sending a query.
		/// </summary>
		/// <param name="text">The text to send to users who query the session.</param>
		public void Away(string text)
		{
			this.Send("AWAY", text);
		}

		/// <summary>
		/// Mark the session as no longer "away".
		/// </summary>
		public void UnAway()
		{
			this.Send("AWAY");
		}

		/// <summary>
		/// Retrieve very basic user and host information about one or more users.
		/// </summary>
		/// <param name="nicknames">The nicknames for which to retrieve information.</param>
		public void UserHost(params string[] nicknames)
		{
			this.Send("USERHOST", nicknames);
		}

		/// <summary>
		/// Set or unset modes for a channel.
		/// </summary>
		/// <param name="channel">The channel on which to set modes.</param>
		/// <param name="modes">The list modes to set or unset.</param>
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

		/// <summary>
		/// Set or unset modes for a channel.
		/// </summary>
		/// <param name="channel">The channel on which to set modes.</param>
		/// <param name="modeSpec">The mode specification in the format +/-[modes] [parameters].</param>
		/// <remarks>
		/// Examples of the modeSpec parameter:
		///   +nst
		///   +i-ns
		///   -i+l 500
		///   +bb a@b.c x@y.z
		/// </remarks>
		public void Mode(string channel, string modeSpec)
		{
			this.Mode(channel, IrcChannelMode.ParseModes(modeSpec));
		}

		/// <summary>
		/// Set or unset modes for the session.
		/// </summary>
		/// <param name="modes">The collection of modes to set or unset.</param>
		public void Mode(IEnumerable<IrcUserMode> modes)
		{
			this.Send("MODE", new IrcTarget(this.Nickname), IrcUserMode.RenderModes(modes));
		}

		/// <summary>
		/// Set or unset modes for the session.
		/// </summary>
		/// <param name="modeSpec">The mode specification in the format +/-[modes] [parameters].</param>
		/// <remarks>
		/// Examples of modeSpec parameter:
		/// +im
		/// +iw-m
		/// -mw
		/// </remarks>
		public void Mode(string modeSpec)
		{
			this.Mode(IrcUserMode.ParseModes(modeSpec));
		}

		/// <summary>
		/// Retrieve the modes for the specified channel.
		/// </summary>
		/// <param name="channel">The channel for which to retrieve modes.</param>
		public void Mode(IrcTarget channel)
		{
			if (channel.IsChannel)
			{
				this.Send("MODE", channel);
			}
		}

		/// <summary>
		/// Retrieve a list of channels matching the specified mask.
		/// </summary>
		/// <param name="mask">The channel name or names to list (supports wildcards).</param>
		/// <param name="target">The name of the server to query.</param>
		public void List(string mask, string target)
		{
			this.Send("LIST", mask, target);
		}

		/// <summary>
		/// Retrieve a list of channels matching the specified mask.
		/// </summary>
		/// <param name="mask">The channel name or names to list (supports wildcards).</param>
		public void List(string mask)
		{
			this.Send("LIST", mask);
		}

		/// <summary>
		/// Retrieves a list of all channels.
		/// </summary>
		public void List()
		{
			this.Send("LIST");
		}

		/// <summary>
		/// Add a handler to capture a specific IRC code. This can be called from components that issue a command and are expecting
		/// some result code to be sent in the future.
		/// </summary>
		/// <param name="capture">An object encapsulating the handler and its options.</param>
		/// <remarks>
		/// A handler can prevent other components from processing a message. For example,
		/// a component that retrieves the hostname of a user with the USERHOST command can handle the response to prevent a client
		/// from displaying the result.
		/// </remarks>
		public void AddHandler(IrcCodeHandler capture)
		{
			lock (_captures)
			{
				_captures.Add(capture);
			}
		}

		/// <summary>
		/// Remove a handler.
		/// </summary>
		/// <param name="capture">The handler to remove. This must be the same object that was added previously.</param>
		/// <returns>Returns true if the handler was removed, false if it had not been added.</returns>
		public bool RemoveHandler(IrcCodeHandler capture)
		{
			lock (_captures)
			{
				return _captures.Remove(capture);
			}
		}

		private void RaiseEvent<T>(EventHandler<T> evt, T e) where T : EventArgs
		{
			if (evt != null)
			{
				evt(this, e);
			}
		}

		private void OnStateChanged()
		{
			if (this.State == IrcSessionState.Connected && _findExternalAddress)
			{
				this.AddHandler(new IrcCodeHandler((e) =>
					{
						e.Handled = true;
						if (e.Message.Parameters.Count < 2)
						{
							return true;
						}

						var parts = e.Message.Parameters[1].Split('@');
						if (parts.Length > 0)
						{
							IPAddress external;
							if (!IPAddress.TryParse(parts[1], out external))
							{
								Dns.BeginGetHostEntry(parts[1], (ar) =>
									{
										try
										{
											var host = Dns.EndGetHostEntry(ar);
											if (host.AddressList.Length > 0)
											{
												this.ExternalAddress = host.AddressList[0];
											}
										}
										catch { }
									}, null);
							}
							else
							{
								this.ExternalAddress = external;
							}
						}
						return true;
					}, IrcCode.RPL_USERHOST));
				this.UserHost(this.Nickname);
			}

			this.RaiseEvent(this.StateChanged, EventArgs.Empty);

			if (this.State == IrcSessionState.Disconnected && this.AutoReconnect)
			{
				if (_reconnectTimer != null)
				{
					_reconnectTimer.Dispose();
				}
				_reconnectTimer = new Timer(new TimerCallback((obj) =>
				{
					if (_syncContext != null)
					{
						_syncContext.Post((o) => ((Action)o)(), (Action)this.OnReconnect);
					}
					else
					{
						this.OnReconnect();
					}
				}), null, ReconnectWaitTime, System.Threading.Timeout.Infinite);
			}
		}

		private void OnReconnect()
		{
			if (this.State == IrcSessionState.Disconnected)
			{
				this.State = IrcSessionState.Connecting;
				_conn.Open(this.Server, this.Port, this.IsSecure, this.Proxy);
			}
		}

		private void OnConnectionError(ErrorEventArgs e)
		{
			this.RaiseEvent(this.ConnectionError, e);
		}

		private void OnMessageReceived(IrcEventArgs e)
		{
			_isWaitingForActivity = false;

#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				System.Diagnostics.Debug.WriteLine(string.Format("RECV: {0}", e.Message.ToString()));
			}
#endif

			this.RaiseEvent(this.RawMessageReceived, e);
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

		private void OnMessageSent(IrcEventArgs e)
		{
			this.RaiseEvent(this.RawMessageSent, e);

#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				System.Diagnostics.Debug.WriteLine(string.Format("SEND: {0}", e.Message.ToString()));
			}
#endif
		}

		private void OnNickChanged(IrcMessage message)
		{
			var e = new IrcNickEventArgs(message);
			var handler = this.NickChanged;
			if (this.IsSelf(e.OldNickname))
			{
				this.Nickname = e.NewNickname;
				handler = this.SelfNickChanged;
			}
			this.RaiseEvent(handler, e);
		}

		private void OnPrivateMessage(IrcMessage message)
		{
			if (message.Parameters.Count > 1 && CtcpCommand.IsCtcpCommand(message.Parameters[1]))
			{
				this.OnCtcpCommand(message);
			}
			else
			{
				this.RaiseEvent(this.PrivateMessaged, new IrcMessageEventArgs(message));
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
				this.RaiseEvent(this.Noticed, new IrcMessageEventArgs(message));
			}
		}

		private void OnQuit(IrcMessage message)
		{
			this.RaiseEvent(this.UserQuit, new IrcQuitEventArgs(message));
		}

		private void OnJoin(IrcMessage message)
		{
			var handler = this.Joined;
			var e = new IrcJoinEventArgs(message);
			if (this.IsSelf(e.Who.Nickname))
			{
				handler = this.SelfJoined;
			}
			this.RaiseEvent(handler, e);
		}

		private void OnPart(IrcMessage message)
		{
			var handler = this.Parted;
			var e = new IrcPartEventArgs(message);
			if (this.IsSelf(e.Who.Nickname))
			{
				handler = this.SelfParted;
			}
			this.RaiseEvent(handler, e);
		}

		private void OnTopic(IrcMessage message)
		{
			this.RaiseEvent(this.TopicChanged, new IrcTopicEventArgs(message));
		}

		private void OnInvite(IrcMessage message)
		{
			this.RaiseEvent(this.Invited, new IrcInviteEventArgs(message));
		}

		private void OnKick(IrcMessage message)
		{
			var handler = this.Kicked;
			var e = new IrcKickEventArgs(message);
			if (this.IsSelf(e.KickeeNickname))
			{
				handler = this.SelfKicked;
			}
			this.RaiseEvent(handler, e);
		}

		private void OnMode(IrcMessage message)
		{
			if (message.Parameters.Count > 0)
			{
				if (IrcTarget.IsChannelName(message.Parameters[0]))
				{
					this.RaiseEvent(this.ChannelModeChanged, new IrcChannelModeEventArgs(message));
				}
				else
				{
					var e = new IrcUserModeEventArgs(message);
					this.UserModes = (from m in e.Modes.Where((newMode) => newMode.Set).Select((newMode) => newMode.Mode).Union(this.UserModes).Distinct()
									  where !e.Modes.Any((newMode) => !newMode.Set && newMode.Mode == m)
									  select m).ToArray();

					this.RaiseEvent(this.UserModeChanged, new IrcUserModeEventArgs(message));
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

				if (_captures.Count > 0)
				{
					lock (_captures)
					{
						var capture = _captures.Where((c) => c.Codes.Contains(e.Code)).FirstOrDefault();
						if (capture != null)
						{
							if (capture.Handler(e))
							{
								_captures.Remove(capture);
							}
							if (e.Handled)
							{
								return;
							}
						}
					}
				}

				this.RaiseEvent(this.InfoReceived, e);
			}
		}

		private void OnCtcpCommand(IrcMessage message)
		{
			this.RaiseEvent(this.CtcpCommandReceived, new CtcpEventArgs(message));
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
		}

		private void _conn_Connected(object sender, EventArgs e)
		{
			this.InternalAddress = this.ExternalAddress =
				Dns.GetHostEntry(string.Empty).AddressList
				.Where((ip) => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault();

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
