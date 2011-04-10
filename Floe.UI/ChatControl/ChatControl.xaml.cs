using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Floe.Net;
using System.Threading;

namespace Floe.UI
{
	public partial class ChatControl : Floe.UI.ChatPage
	{
		#region Nested types

		private class CommandException : Exception
		{
			public CommandException(string message)
				: base(message)
			{
			}
		}

		#endregion

		private const double MinNickListWidth = 50.0;

		private LinkedList<string> _history;
		private LinkedListNode<string> _historyNode;
		private LogFileHandle _logFile;
		private ChatLine _markerLine;
		private VoiceControl _voiceControl;
		private Timer _delayTimer;

		public ChatControl(ChatPageType type, IrcSession session, IrcTarget target)
			: base(type, session, target, type == ChatPageType.Server ? "server" : 
			(type == ChatPageType.DccChat ? "dcc-chat" : string.Format("{0}.{1}", session.NetworkName, target.Name).ToLowerInvariant()))
		{
			_history = new LinkedList<string>();
			_nickList = new NicknameList();

			InitializeComponent();

			var state = App.Settings.Current.Windows.States[this.Id];
			if (this.Type == ChatPageType.DccChat)
			{
				this.Header = string.Format("[CHAT] {0}", this.Target.Name);
			}
			else if (this.Type == ChatPageType.Chat || this.Type == ChatPageType.Server)
			{
				this.Header = this.Target == null ? "Server" : this.Target.ToString();
				this.SubscribeEvents();

				if (!this.IsServer)
				{
					_logFile = App.OpenLogFile(this.Id);
					var logLines = new List<ChatLine>();
					while (_logFile.Buffer.Count > 0)
					{
						var cl = _logFile.Buffer.Dequeue();
						cl.Marker = _logFile.Buffer.Count == 0 ? ChatMarker.OldMarker : ChatMarker.None;
						logLines.Add(cl);
					}
					boxOutput.AppendBulkLines(logLines);
				}

				if (this.IsChannel)
				{
					colNickList.MinWidth = MinNickListWidth;
					colNickList.Width = new GridLength(state.NickListWidth);

					this.Write("Join", string.Format("Now talking on {0}", this.Target.Name));
					this.Session.AddHandler(new IrcCodeHandler((e) =>
						{
							if (e.Message.Parameters.Count > 2 &&
								this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
							{
								_channelModes = e.Message.Parameters[2].ToCharArray().Where((c) => c != '+').ToArray();
								this.SetTitle();
							}
							e.Handled = true;
							return true;
						}, IrcCode.RPL_CHANNELMODEIS));
					this.Session.Mode(this.Target);
					splitter.IsEnabled = true;

					var nameHandler = new IrcCodeHandler((e) =>
						{
							if (e.Message.Parameters.Count >= 3)
							{
								var to = new IrcTarget(e.Message.Parameters[e.Message.Parameters.Count - 2]);
								if (this.Target.Equals(to))
								{
									_nickList.AddRange(e.Message.Parameters[e.Message.Parameters.Count - 1].Split(' ').
										Where((n) => n.Length > 0));
								}
							}
							e.Handled = true;
							return false;
						}, IrcCode.RPL_NAMEREPLY);
					this.Session.AddHandler(nameHandler);
					this.Session.AddHandler(new IrcCodeHandler((e) =>
						{
							this.Session.RemoveHandler(nameHandler);
							e.Handled = true;
							return true;
						}, IrcCode.RPL_ENDOFNAMES));
				}
				else if (this.IsNickname)
				{
					_prefix = this.Target.Name;
				}
			}
			else
			{
				throw new ArgumentException("Page type is not supported.");
			}

			boxOutput.ColumnWidth = state.ColumnWidth;

			this.Loaded += new RoutedEventHandler(ChatControl_Loaded);
			this.Unloaded += new RoutedEventHandler(ChatControl_Unloaded);
			this.PrepareContextMenus();
			boxOutput.ContextMenu = this.GetDefaultContextMenu();
		}

		public bool IsChannel { get { return this.Type == ChatPageType.Chat && this.Target.IsChannel; } }
		public bool IsNickname { get { return this.Type == ChatPageType.Chat && !this.Target.IsChannel; } }
		public string Perform { get; set; }

		public static readonly DependencyProperty IsConnectedProperty =
			DependencyProperty.Register("IsConnected", typeof(bool), typeof(ChatControl));
		public bool IsConnected
		{
			get { return (bool)this.GetValue(IsConnectedProperty); }
			set { this.SetValue(IsConnectedProperty, value); }
		}

		public static readonly DependencyProperty SelectedLinkProperty =
			DependencyProperty.Register("SelectedLink", typeof(string), typeof(ChatControl));
		public string SelectedLink
		{
			get { return (string)this.GetValue(SelectedLinkProperty); }
			set { this.SetValue(SelectedLinkProperty, value); }
		}

		public void Connect(Floe.Configuration.ServerElement server)
		{
			this.Session.AutoReconnect = false;
			this.Perform = server.OnConnect;
			this.Connect(server.Hostname, server.Port, server.IsSecure, server.AutoReconnect, server.Password);
		}

		public void Connect(string server, int port, bool useSsl, bool autoReconnect, string password)
		{
			this.Session.Open(server, port, useSsl,
				!string.IsNullOrEmpty(this.Session.Nickname) ?
					this.Session.Nickname : App.Settings.Current.User.Nickname,
				App.Settings.Current.User.Username,
				App.Settings.Current.User.FullName,
				autoReconnect,
				password,
				App.Settings.Current.User.Invisible,
				App.Settings.Current.Dcc.FindExternalAddress,
				App.ProxyInfo);
		}

		private void ParseInput(string text)
		{
			this.Execute(text, (Keyboard.Modifiers & ModifierKeys.Shift) > 0);
		}

		private void Write(string styleKey, int nickHashCode, string nick, string text, bool attn)
		{
			var cl = new ChatLine(styleKey, nickHashCode, nick, text, ChatMarker.None);

			if (_hasDeactivated)
			{
				_hasDeactivated = false;
				if (_markerLine != null)
				{
					_markerLine.Marker &= ~ChatMarker.NewMarker;
				}
				_markerLine = cl;
				cl.Marker = ChatMarker.NewMarker;
			}

			if (attn)
			{
				cl.Marker |= ChatMarker.Attention;
			}

			if (this.VisualParent == null)
			{
				if (this.IsNickname || this.Type == ChatPageType.DccChat)
				{
					// Activity in PM window
					this.NotifyState = NotifyState.Alert;
				}
				else if (!string.IsNullOrEmpty(nick) && this.NotifyState != NotifyState.Alert)
				{
					// Chat activity in channel
					this.NotifyState = NotifyState.ChatActivity;
				}
				else if (this.NotifyState == NotifyState.None)
				{
					// Other activity in channel / server
					this.NotifyState = NotifyState.NoiseActivity;
				}
			}

			boxOutput.AppendLine(cl);
			if (_logFile != null)
			{
				_logFile.WriteLine(cl);
			}
		}

		private void Write(string styleKey, IrcPeer peer, string text, bool attn)
		{
			this.Write(styleKey, string.Format("{0}@{1}", peer.Username, peer.Hostname).GetHashCode(),
				this.GetNickWithLevel(peer.Nickname), text, attn);
			if (!boxOutput.IsAutoScrolling)
			{
				App.DoEvent("beep");
			}
		}

		private void Write(string styleKey, string text)
		{
			this.Write(styleKey, 0, null, text, false);
		}

		private void SetInputText(string text)
		{
			txtInput.Text = text;
			txtInput.SelectionStart = text.Length;
			_nickCandidates = null;
		}

		private void SetTitle()
		{
			string userModes = this.Session.UserModes.Length > 0 ?
				string.Format("+{0}", string.Join("", (from c in this.Session.UserModes select c.ToString()).ToArray())) : "";
			string channelModes = _channelModes.Length > 0 ?
				string.Format("+{0}", string.Join("", (from c in _channelModes select c.ToString()).ToArray())) : "";

			switch (this.Type)
			{
				case ChatPageType.DccChat:
					this.Title = string.Format("{0} - {1} - DCC chat with {2}", App.Product, this.Session.Nickname, this.Target.Name);
					break;

				case ChatPageType.Server:
					if (this.Session.State == IrcSessionState.Disconnected)
					{
						this.Title = string.Format("{0} - Not Connected", App.Product);
					}
					else
					{
						this.Title = string.Format("{0} - {1} ({2}) on {3}", App.Product, this.Session.Nickname,
							userModes, this.Session.NetworkName);
					}
					break;

				default:
					if (this.Target.IsChannel)
					{
						this.Title = string.Format("{0} - {1} ({2}) on {3} - {4} ({5}) - {6}", App.Product, this.Session.Nickname,
							userModes, this.Session.NetworkName, this.Target.ToString(), channelModes, _topic);
					}
					else
					{
						this.Title = string.Format("{0} - {1} ({2}) on {3} - {4}", App.Product, this.Session.Nickname,
							userModes, this.Session.NetworkName, _prefix);
					}
					break;
			}
		}

		private void SubmitInput()
		{
			string text = txtInput.Text;
			txtInput.Clear();
			if (_history.Count == 0 || _history.First.Value != text)
			{
				_history.AddFirst(text);
			}
			while (_history.Count > App.Settings.Current.Buffer.InputHistory)
			{
				_history.RemoveLast();
			}
			_historyNode = null;
			this.ParseInput(text);
		}

		private ContextMenu GetDefaultContextMenu()
		{
			if (this.IsServer)
			{
				var menu = this.Resources["cmServer"] as ContextMenu;
				var item = menu.Items[0] as MenuItem;
				if (item != null)
				{
					item.Items.Refresh();
					item.IsEnabled = item.Items.Count > 0;
				}
				return menu;
			}
			else
			{
				return this.Resources["cmChannel"] as ContextMenu;
			}
		}

		public override void Dispose()
		{
			var state = App.Settings.Current.Windows.States[this.Id];
			state.ColumnWidth = boxOutput.ColumnWidth;

			if (this.Type == ChatPageType.DccChat && _dcc != null)
			{
				_dcc.Dispose();
				this.DeletePortForwarding();
			}
			else
			{
				if (this.IsChannel)
				{
					state.NickListWidth = colNickList.ActualWidth;
				}
				this.UnsubscribeEvents();
				if (_logFile != null)
				{
					_logFile.Dispose();
				}
			}
			if (_voiceControl != null)
			{
				_voiceControl.Dispose();
			}
		}

		private void DoPerform(int startIndex)
		{
			var commands = this.Perform.Split(Environment.NewLine.ToCharArray()).Where((s) => s.Trim().Length > 0).Select((s) => s.Trim()).ToArray();
			for (int i = startIndex; i < commands.Length; i++)
			{
				if (commands[i].StartsWith("/DELAY", StringComparison.InvariantCultureIgnoreCase))
				{
					int time;
					var parts = commands[i].Split(' ');
					if (parts.Length < 2 || !int.TryParse(parts[1], out time))
					{
						time = 1;
					}
					_delayTimer = new Timer((o) =>
					{
						this.Dispatcher.BeginInvoke((Action)(() =>
						{
							this.DoPerform(i + 1);
						}));
					}, null, time * 1000, Timeout.Infinite);
					return;
				}
				else
				{
					this.Execute(commands[i], false);
				}
			}
		}
	}
}
