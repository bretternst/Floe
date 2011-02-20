using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatControl : ChatPage
	{
		private char[] _channelModes = new char[0];
		private string _topic = "", _prefix;
		private bool _hasDeactivated = false, _usingAlternateNick = false;
		private Window _window;

		private void Session_StateChanged(object sender, EventArgs e)
		{
			var state = this.Session.State;
			this.IsConnected = state != IrcSessionState.Disconnected;

			if (state == IrcSessionState.Disconnected)
			{
				this.Write("Error", "Disconnected");
			}

			if (this.IsServer)
			{
				switch (state)
				{
					case IrcSessionState.Connecting:
						_usingAlternateNick = false;
						this.Header = this.Session.NetworkName;
						this.Write("Client", string.Format(
							"Connecting to {0}:{1}", this.Session.Server, this.Session.Port));
						break;
					case IrcSessionState.Connected:
						this.Header = this.Session.NetworkName;
						App.DoEvent("connect");
						if (this.Perform != null)
						{
							foreach (var cmd in this.Perform.Split(Environment.NewLine.ToCharArray()).Where((s) => s.Trim().Length > 0))
							{
								this.Execute(cmd, false);
							}
						}
						break;
				}
				this.SetTitle();
			}
		}

		private void Session_ConnectionError(object sender, ErrorEventArgs e)
		{
			if (this.IsServer)
			{
				this.Write("Error", string.IsNullOrEmpty(e.Exception.Message) ? e.Exception.GetType().Name : e.Exception.Message);
			}
		}

		private void Session_Noticed(object sender, IrcMessageEventArgs e)
		{
			if (App.IsIgnoreMatch(e.From, IgnoreActions.Notice))
			{
				return;
			}

			if (this.IsServer)
			{
				if (e.From is IrcPeer)
				{
					this.Write("Notice", (IrcPeer)e.From, e.Text, false);
				}
				else if (this.IsServer)
				{
					this.Write("Notice", e.Text);
				}
				App.DoEvent("notice");
			}
		}

		private void Session_PrivateMessaged(object sender, IrcMessageEventArgs e)
		{
			if (App.IsIgnoreMatch(e.From, e.To.IsChannel ? IgnoreActions.Channel : IgnoreActions.Private))
			{
				return;
			}

			if (!this.IsServer)
			{
				if ((this.Target.IsChannel && this.Target.Equals(e.To)) ||
					(!this.Target.IsChannel && this.Target.Equals(new IrcTarget(e.From)) && !e.To.IsChannel))
				{
					bool attn = false;
					if (App.IsAttentionMatch(this.Session.Nickname, e.Text))
					{
						attn = true;
						if (_window != null)
						{
							App.Alert(_window, string.Format("You received an alert from {0}", this.Target.Name));
						}
						if (this.VisualParent == null)
						{
							this.NotifyState = NotifyState.Alert;
							App.DoEvent("inactiveAlert");
						}
						else if (_window != null && !_window.IsActive)
						{
							App.DoEvent("inactiveAlert");
						}
						else
						{
							App.DoEvent("activeAlert");
						}
					}

					this.Write("Default", e.From, e.Text, attn);
					if (!this.Target.IsChannel)
					{
						if (e.From.Prefix != _prefix)
						{
							_prefix = e.From.Prefix;
							this.SetTitle();
						}
						Interop.WindowHelper.FlashWindow(_window);
						if (this.VisualParent == null)
						{
							App.DoEvent("privateMessage");
						}
					}
				}
			}
		}

		private void Session_Kicked(object sender, IrcKickEventArgs e)
		{
			if (!this.IsServer && this.Target.Equals(e.Channel))
			{
				this.Write("Kick", string.Format("{0} has been kicked by {1} ({2})",
					e.KickeeNickname, e.Kicker.Nickname, e.Text));
				this.RemoveNick(e.KickeeNickname);
			}
		}

		private void Session_SelfKicked(object sender, IrcKickEventArgs e)
		{
			if (this.IsServer)
			{
				this.Write("Kick", string.Format("You have been kicked from {0} by {1} ({2})",
					e.Channel, e.Kicker.Nickname, e.Text));
			}
		}

		private void Session_InfoReceived(object sender, IrcInfoEventArgs e)
		{
			switch (e.Code)
			{
				case IrcCode.ERR_NICKNAMEINUSE:
					if (this.IsServer && this.Session.State == IrcSessionState.Connecting)
					{
						if (_usingAlternateNick || string.IsNullOrEmpty(App.Settings.Current.User.AlternateNickname))
						{
							this.SetInputText("/nick ");
						}
						else
						{
							this.Session.Nick(App.Settings.Current.User.AlternateNickname);
							_usingAlternateNick = true;
						}
					}
					break;
				case IrcCode.RPL_TOPIC:
					if (e.Message.Parameters.Count == 3 && !this.IsServer &&
						this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
					{
						_topic = e.Message.Parameters[2];
						this.SetTitle();
						this.Write("Topic", string.Format("Topic is: {0}", _topic));
					}
					return;
				case IrcCode.RPL_TOPICSETBY:
					if (e.Message.Parameters.Count == 4 && !this.IsServer &&
						this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
					{
						this.Write("Topic", string.Format("Topic set by {0} on {1}", e.Message.Parameters[2],
							this.FormatTime(e.Message.Parameters[3])));
					}
					return;
				case IrcCode.RPL_CHANNELCREATEDON:
					if (e.Message.Parameters.Count == 3 && !this.IsServer &&
						this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
					{
						//this.Write("ServerInfo", string.Format("* Channel created on {0}", this.FormatTime(e.Message.Parameters[2])));
					}
					return;
				case IrcCode.RPL_WHOISUSER:
				case IrcCode.RPL_WHOWASUSER:
					if (e.Message.Parameters.Count == 6 && this.IsDefault)
					{
						this.Write("ServerInfo",
							string.Format("{1} " + (e.Code == IrcCode.RPL_WHOWASUSER ? "was" : "is") + " {2}@{3} {4} {5}",
							(object[])e.Message.Parameters));
						return;
					}
					break;
				case IrcCode.RPL_WHOISCHANNELS:
					if (e.Message.Parameters.Count == 3 && this.IsDefault)
					{
						this.Write("ServerInfo", string.Format("{1} is on {2}",
							(object[])e.Message.Parameters));
						return;
					}
					break;
				case IrcCode.RPL_WHOISSERVER:
					if (e.Message.Parameters.Count == 4 && this.IsDefault)
					{
						this.Write("ServerInfo", string.Format("{1} using {2} {3}",
							(object[])e.Message.Parameters));
						return;
					}
					break;
				case IrcCode.RPL_WHOISIDLE:
					if (e.Message.Parameters.Count == 5 && this.IsDefault)
					{
						this.Write("ServerInfo", string.Format("{0} has been idle {1}, signed on {2}",
							e.Message.Parameters[1], this.FormatTimeSpan(e.Message.Parameters[2]),
							this.FormatTime(e.Message.Parameters[3])));
						return;
					}
					break;
				case IrcCode.RPL_INVITING:
					if (e.Message.Parameters.Count == 3 && this.IsDefault)
					{
						this.Write("ServerInfo", string.Format("Invited {0} to channel {1}",
							e.Message.Parameters[1], e.Message.Parameters[2]));
						return;
					}
					break;
				case IrcCode.RPL_LIST:
				case IrcCode.RPL_LISTSTART:
				case IrcCode.RPL_LISTEND:
					e.Handled = true;
					break;
			}

			if (!e.Handled && ((int)e.Code < 200 && this.IsServer || this.IsDefault))
			{
				this.Write("ServerInfo", e.Text);
			}
		}

		private bool IsDefault
		{
			get
			{
				if(_window is ChannelWindow && _window.IsActive)
				{
					return true;
				}
				else if (_window is ChatWindow)
				{
					if(this.IsVisible)
					{
						return true;
					}

					if(this.IsServer &&
						!((ChatWindow)_window).Items.Any((item) => item.IsVisible && item.Page.Session == this.Session) &&
						!App.Current.Windows.OfType<ChannelWindow>().Any((cw) => cw.Session == this.Session && cw.IsActive))
					{
						return true;
					}
				}

				return false;
			}
		}

		private void Session_CtcpCommandReceived(object sender, CtcpEventArgs e)
		{
			if (App.IsIgnoreMatch(e.From, IgnoreActions.Ctcp))
			{
				return;
			}

			if (((this.IsChannel && this.Target.Equals(e.To)) ||
				(this.IsNickname && this.Target.Equals(new IrcTarget(e.From)) && !e.To.IsChannel))
				&& e.Command.Command == "ACTION")
			{
				string text = string.Join(" ", e.Command.Arguments);
				bool attn = false;
				if (App.IsAttentionMatch(this.Session.Nickname, text))
				{
					attn = true;
					if (_window != null)
					{
						Interop.WindowHelper.FlashWindow(_window);
					}
				}

				this.Write("Action", string.Format("{0} {1}", e.From.Nickname, text, attn));
			}
			else if (this.IsServer && e.Command.Command != "ACTION")
			{
				this.Write("Ctcp", e.From, string.Format("[CTCP {1}] {2}",
					e.From.Nickname, e.Command.Command,
					e.Command.Arguments.Length > 0 ? string.Join(" ", e.Command.Arguments) : ""), false);
			}
		}

		private void Session_Joined(object sender, IrcJoinEventArgs e)
		{
			bool isIgnored = App.IsIgnoreMatch(e.Who, IgnoreActions.Join);

			if (!this.IsServer && this.Target.Equals(e.Channel))
			{
				if (!isIgnored)
				{
					this.Write("Join", string.Format("{0} ({1}@{2}) has joined channel {3}",
						e.Who.Nickname, e.Who.Username, e.Who.Hostname, this.Target.ToString()));
				}
				this.AddNick(ChannelLevel.Normal, e.Who.Nickname);
			}
		}

		private void Session_Parted(object sender, IrcPartEventArgs e)
		{
			bool isIgnored = App.IsIgnoreMatch(e.Who, IgnoreActions.Part);

			if (!this.IsServer && this.Target.Equals(e.Channel))
			{
				if (!isIgnored)
				{
					this.Write("Part", string.Format("{0} ({1}@{2}) has left channel {3}",
						e.Who.Nickname, e.Who.Username, e.Who.Hostname, this.Target.ToString()));
				}
				this.RemoveNick(e.Who.Nickname);
			}
		}

		private void Session_NickChanged(object sender, IrcNickEventArgs e)
		{
			bool isIgnored = App.IsIgnoreMatch(e.Message.From, IgnoreActions.NickChange);

			if (this.IsChannel && this.IsPresent(e.OldNickname))
			{
				if (!isIgnored)
				{
					this.Write("Nick", string.Format("{0} is now known as {1}", e.OldNickname, e.NewNickname));
				}
			}

			if (this.IsChannel && this.IsPresent(e.OldNickname))
			{
				this.ChangeNick(e.OldNickname, e.NewNickname);
			}
		}

		private void Session_SelfNickChanged(object sender, IrcNickEventArgs e)
		{
			if (this.IsServer || this.IsChannel)
			{
				this.Write("Nick", string.Format("You are now known as {0}", e.NewNickname));
			}
			this.SetTitle();

			if (this.IsChannel && this.IsPresent(e.OldNickname))
			{
				this.ChangeNick(e.OldNickname, e.NewNickname);
			}
		}

		private void Session_TopicChanged(object sender, IrcTopicEventArgs e)
		{
			if (!this.IsServer && this.Target.Equals(e.Channel))
			{
				this.Write("Topic", string.Format("{0} changed topic to: {1}", e.Who.Nickname, e.Text));
				_topic = e.Text;
				this.SetTitle();
			}
		}

		private void Session_UserModeChanged(object sender, IrcUserModeEventArgs e)
		{
			if (this.IsServer)
			{
				this.Write("Mode", string.Format("You set mode: {0}", IrcUserMode.RenderModes(e.Modes)));
			}
			this.SetTitle();
		}

		private void Session_UserQuit(object sender, IrcQuitEventArgs e)
		{
			bool isIgnored = App.IsIgnoreMatch(e.Who, IgnoreActions.Quit);

			if (this.IsChannel && this.IsPresent(e.Who.Nickname))
			{
				if (!isIgnored)
				{
					this.Write("Quit", string.Format("{0} has quit ({1})", e.Who.Nickname, e.Text));
				}
				this.RemoveNick(e.Who.Nickname);
			}
		}

		private void Session_ChannelModeChanged(object sender, IrcChannelModeEventArgs e)
		{
			if (!this.IsServer && this.Target.Equals(e.Channel))
			{
				if (e.Who != null)
				{
					this.Write("Mode", string.Format("{0} set mode: {1}", e.Who.Nickname,
						string.Join(" ", IrcChannelMode.RenderModes(e.Modes))));

					_channelModes = (from m in e.Modes.Where((newMode) => newMode.Parameter == null && newMode.Set).
										 Select((newMode) => newMode.Mode).Union(_channelModes).Distinct()
									 where !e.Modes.Any((newMode) => !newMode.Set && newMode.Mode == m)
									 select m).ToArray();
				}
				else
				{
					_channelModes = (from m in e.Modes
									 where m.Set && m.Parameter == null
									 select m.Mode).ToArray();
				}
				this.SetTitle();
				foreach (var mode in e.Modes)
				{
					this.ProcessMode(mode);
				}
			}
		}

		private void Session_Invited(object sender, IrcInviteEventArgs e)
		{
			if (App.IsIgnoreMatch(e.From, IgnoreActions.Invite))
			{
				return;
			}

			if (this.IsDefault || this.IsServer)
			{
				this.Write("Invite", string.Format("{0} invited you to channel {1}", e.From.Nickname, e.Channel));
			}
		}

		private void txtInput_KeyDown(object sender, KeyEventArgs e)
		{
			if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
			{
				int c = 0;
				switch (e.Key)
				{
					case Key.B:
						c = 2;
						break;
					case Key.K:
						c = 3;
						break;
					case Key.R:
						c = 22;
						break;
					case Key.O:
						c = 15;
						break;
					case Key.U:
						c = 31;
						break;
				}
				if ((int)c != 0)
				{
					var s = new string((char)(c + 0x2500), 1);
					this.Insert(s);
				}
			}

			switch (e.Key)
			{
				case Key.Enter:
					this.SubmitInput();
					break;
			}
		}

		private void txtInput_Pasting(object sender, DataObjectPastingEventArgs e)
		{
			if (e.DataObject.GetDataPresent(typeof(string)))
			{
				var text = e.DataObject.GetData(typeof(string)) as string;
				if (text.Contains(Environment.NewLine))
				{
					e.CancelCommand();

					var parts = text.Split(Environment.NewLine.ToCharArray()).Where((s) => s.Trim().Length > 0).ToArray();
					if (parts.Length > App.Settings.Current.Buffer.MaximumPasteLines)
					{
						Dispatcher.BeginInvoke((Action)(() =>
							{
								if (!App.Confirm(_window, string.Format("Are you sure you want to paste more than {0} lines?",
									App.Settings.Current.Buffer.MaximumPasteLines), "Paste Warning"))
								{
									return;
								}
								foreach (var part in parts)
								{
									txtInput.Text = txtInput.Text.Substring(0, txtInput.SelectionStart);
									txtInput.Text += part;
									this.SubmitInput();
								}
							}));
					}
					else
					{
						foreach (var part in parts)
						{
							txtInput.Text = txtInput.Text.Substring(0, txtInput.SelectionStart);
							txtInput.Text += part;
							this.SubmitInput();
						}
					}
				}
			}
		}

		private void lstNicknames_MouseDoubleClick(object sender, RoutedEventArgs e)
		{
			var nickItem = e.Source as NicknameItem;
			if (nickItem != null)
			{
				ChatWindow.ChatCommand.Execute(nickItem.Nickname, this);
			}
		}

		private void _window_Deactivated(object sender, EventArgs e)
		{
			_hasDeactivated = true;
			this.SelectedLink = null;
		}

		protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			_hasDeactivated = false;
		}

		private void boxOutput_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var link = boxOutput.SelectedLink;
			if (!string.IsNullOrEmpty(link))
			{
				if (Constants.UrlRegex.IsMatch(link))
				{
					App.BrowseTo(link);
				}
				else
				{
					ChatWindow.ChatCommand.Execute(this.GetNickWithoutLevel(link), this);
				}
			}
		}

		protected override void OnContextMenuOpening(ContextMenuEventArgs e)
		{
			this.SelectedLink = boxOutput.SelectedLink;
			if (!string.IsNullOrEmpty(this.SelectedLink))
			{
				if (Constants.UrlRegex.IsMatch(this.SelectedLink))
				{
					boxOutput.ContextMenu = this.Resources["cmHyperlink"] as ContextMenu;
				}
				else
				{
					if (this.Type == ChatPageType.DccChat)
					{
						return;
					}
					this.SelectedLink = this.GetNickWithoutLevel(this.SelectedLink);
					boxOutput.ContextMenu = this.Resources["cmNickname"] as ContextMenu;
				}
				boxOutput.ContextMenu.IsOpen = true;
				e.Handled = true;
			}
			else
			{
				boxOutput.ContextMenu = this.GetDefaultContextMenu();
				if (this.IsServer && boxOutput.ContextMenu != null)
				{
					boxOutput.ContextMenu.Items.Refresh();
				}
			}

			base.OnContextMenuOpening(e);
		}

		private void connect_Click(object sender, RoutedEventArgs e)
		{
			var item = ((MenuItem)boxOutput.ContextMenu.Items[0]).ItemContainerGenerator.ItemFromContainer((DependencyObject)e.OriginalSource)
				as Floe.Configuration.ServerElement;
			if (item != null)
			{
				if (this.IsConnected)
				{
					this.Session.Quit("Changing servers");
				}
				this.Connect(item);
			}
		}

		private void ChatControl_Loaded(object sender, RoutedEventArgs e)
		{
			Keyboard.Focus(txtInput);
			this.SetTitle();

			if (_window == null)
			{
				_window = Window.GetWindow(this);
				if (_window != null)
				{
					_window.Deactivated += new EventHandler(_window_Deactivated);
				}
			}
			else
			{
				_window = Window.GetWindow(this);
				this.NotifyState = NotifyState.None;
			}
		}

		private void ChatControl_Unloaded(object sender, RoutedEventArgs e)
		{
			_hasDeactivated = true;
			this.SelectedLink = null;
			if (_window != null)
			{
				_window.Deactivated -= new EventHandler(_window_Deactivated);
			}
		}

		private void txtInput_SelectionChanged(object sender, RoutedEventArgs e)
		{
			_nickCandidates = null;
		}

		protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
		{
			this.SelectedLink = null;
			base.OnPreviewMouseRightButtonDown(e);
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			var focused = FocusManager.GetFocusedElement(this);
			if (focused is TextBox && focused != txtInput)
			{
				return;
			}

			if((Keyboard.Modifiers & ModifierKeys.Alt) == 0 &&
				(Keyboard.Modifiers & ModifierKeys.Control) == 0 &&
				!(FocusManager.GetFocusedElement(this) is NicknameItem))
			{
				e.Handled = true;

				switch (e.Key)
				{
					case Key.PageUp:
						boxOutput.PageUp();
						break;
					case Key.PageDown:
						boxOutput.PageDown();
						break;
					case Key.Up:
						if (txtInput.GetLineIndexFromCharacterIndex(txtInput.CaretIndex) > 0)
						{
							e.Handled = false;
							return;
						}
						else
						{
							if (_historyNode != null)
							{
								if (_historyNode.Next != null)
								{
									_historyNode = _historyNode.Next;
									this.SetInputText(_historyNode.Value);
								}
							}
							else if (_history.First != null)
							{
								_historyNode = _history.First;
								this.SetInputText(_historyNode.Value);
							}
						}
						break;
					case Key.Down:
						if (txtInput.GetLineIndexFromCharacterIndex(txtInput.CaretIndex) < txtInput.LineCount - 1)
						{
							e.Handled = false;
							return;
						}
						if (_historyNode != null)
						{
							_historyNode = _historyNode.Previous;
							if (_historyNode != null)
							{
								this.SetInputText(_historyNode.Value);
							}
							else
							{
								txtInput.Clear();
							}
						}
						else
						{
							txtInput.Clear();
						}
						break;
					case Key.Tab:
						if (this.IsChannel || this.IsNickname)
						{
							DoNickCompletion();
						}
						break;
					default:
						Keyboard.Focus(txtInput);
						e.Handled = false;
						break;
				}
			}
			else if (e.Key >= Key.A && e.Key <= Key.Z)
			{
				Keyboard.Focus(txtInput);
			}

			base.OnPreviewKeyDown(e);
		}

		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			if (e.Delta > 0)
			{
				boxOutput.MouseWheelUp();
			}
			else
			{
				boxOutput.MouseWheelDown();
			}
			e.Handled = true;

			base.OnPreviewMouseWheel(e);
		}

		private void SubscribeEvents()
		{
			this.Session.StateChanged += new EventHandler<EventArgs>(Session_StateChanged);
			this.Session.ConnectionError += new EventHandler<ErrorEventArgs>(Session_ConnectionError);
			this.Session.Noticed += new EventHandler<IrcMessageEventArgs>(Session_Noticed);
			this.Session.PrivateMessaged += new EventHandler<IrcMessageEventArgs>(Session_PrivateMessaged);
			this.Session.Kicked += new EventHandler<IrcKickEventArgs>(Session_Kicked);
			this.Session.SelfKicked += new EventHandler<IrcKickEventArgs>(Session_SelfKicked);
			this.Session.InfoReceived += new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
			this.Session.CtcpCommandReceived += new EventHandler<CtcpEventArgs>(Session_CtcpCommandReceived);
			this.Session.Joined += new EventHandler<IrcJoinEventArgs>(Session_Joined);
			this.Session.Parted += new EventHandler<IrcPartEventArgs>(Session_Parted);
			this.Session.NickChanged += new EventHandler<IrcNickEventArgs>(Session_NickChanged);
			this.Session.SelfNickChanged += new EventHandler<IrcNickEventArgs>(Session_SelfNickChanged);
			this.Session.TopicChanged += new EventHandler<IrcTopicEventArgs>(Session_TopicChanged);
			this.Session.UserModeChanged += new EventHandler<IrcUserModeEventArgs>(Session_UserModeChanged);
			this.Session.ChannelModeChanged += new EventHandler<IrcChannelModeEventArgs>(Session_ChannelModeChanged);
			this.Session.UserQuit += new EventHandler<IrcQuitEventArgs>(Session_UserQuit);
            this.Session.Invited += new EventHandler<IrcInviteEventArgs>(Session_Invited);
			DataObject.AddPastingHandler(txtInput, new DataObjectPastingEventHandler(txtInput_Pasting));

			this.IsConnected = !(this.Session.State == IrcSessionState.Disconnected);
		}

		private void UnsubscribeEvents()
		{
			this.Session.StateChanged -= new EventHandler<EventArgs>(Session_StateChanged);
			this.Session.ConnectionError -= new EventHandler<ErrorEventArgs>(Session_ConnectionError);
			this.Session.Noticed -= new EventHandler<IrcMessageEventArgs>(Session_Noticed);
			this.Session.PrivateMessaged -= new EventHandler<IrcMessageEventArgs>(Session_PrivateMessaged);
			this.Session.Kicked -= new EventHandler<IrcKickEventArgs>(Session_Kicked);
			this.Session.SelfKicked -= new EventHandler<IrcKickEventArgs>(Session_SelfKicked);
			this.Session.InfoReceived -= new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
			this.Session.CtcpCommandReceived -= new EventHandler<CtcpEventArgs>(Session_CtcpCommandReceived);
			this.Session.Joined -= new EventHandler<IrcJoinEventArgs>(Session_Joined);
			this.Session.Parted -= new EventHandler<IrcPartEventArgs>(Session_Parted);
			this.Session.NickChanged -= new EventHandler<IrcNickEventArgs>(Session_NickChanged);
			this.Session.SelfNickChanged -= new EventHandler<IrcNickEventArgs>(Session_SelfNickChanged);
			this.Session.TopicChanged -= new EventHandler<IrcTopicEventArgs>(Session_TopicChanged);
			this.Session.UserModeChanged -= new EventHandler<IrcUserModeEventArgs>(Session_UserModeChanged);
			this.Session.ChannelModeChanged -= new EventHandler<IrcChannelModeEventArgs>(Session_ChannelModeChanged);
			this.Session.UserQuit -= new EventHandler<IrcQuitEventArgs>(Session_UserQuit);
			this.Session.Invited -= new EventHandler<IrcInviteEventArgs>(Session_Invited);
			DataObject.RemovePastingHandler(txtInput, new DataObjectPastingEventHandler(txtInput_Pasting));

			if (_window != null)
			{
				_window.Deactivated -= new EventHandler(_window_Deactivated);
			}
		}

		private void PrepareContextMenus()
		{
			var menu = this.Resources["cmServer"] as ContextMenu;
			if (menu != null)
			{
				NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
			}
			menu = this.Resources["cmNickList"] as ContextMenu;
			if (menu != null)
			{
				NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
			}
			menu = this.Resources["cmNickname"] as ContextMenu;
			if (menu != null)
			{
				NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
			}
			menu = this.Resources["cmHyperlink"] as ContextMenu;
			if (menu != null)
			{
				NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
			}
			menu = this.Resources["cmChannel"] as ContextMenu;
			if (menu != null)
			{
				NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
			}
		}

		private string FormatTime(string text)
		{
			int seconds = 0;
			if (!int.TryParse(text, out seconds))
			{
				return "";
			}
			var ts = new TimeSpan(0, 0, seconds);
			return new DateTime(1970, 1, 1).Add(ts).ToLocalTime().ToString();
		}

		private string FormatTimeSpan(string text)
		{
			int seconds = 0;
			if (!int.TryParse(text, out seconds))
			{
				return "";
			}
			return new TimeSpan(0, 0, seconds).ToString();
		}
	}
}
