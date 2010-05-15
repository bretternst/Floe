using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatControl : UserControl
	{
		private char[] _channelModes = new char[0];
		private string _topic = "", _prefix;
		private bool _hasNames = false, _hasModes = false, _hasDeactivated = false, _usingAlternateNick = false;
		private Window _window;

		private void Session_StateChanged(object sender, EventArgs e)
		{
			var state = this.Session.State;
			this.BeginInvoke(() =>
				{
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
								if (this.Perform != null)
								{
									foreach (var cmd in this.Perform.Split(Environment.NewLine.ToCharArray()).Where((s) => s.Trim().Length > 0))
									{
										this.Execute(cmd);
									}
								}
								break;
						}
						this.SetTitle();
					}
				});
		}

		private void Session_ConnectionError(object sender, ErrorEventArgs e)
		{
			if (this.IsServer)
			{
				this.BeginInvoke(() => this.Write("Error",
					string.IsNullOrEmpty(e.Exception.Message) ? e.Exception.GetType().Name : e.Exception.Message));
			}
		}

		private void Session_Noticed(object sender, IrcDialogEventArgs e)
		{
			if (this.IsServer)
			{
				this.BeginInvoke(() =>
					{
						if (e.From is IrcPeer)
						{
							this.Write("Notice", (IrcPeer)e.From, e.Text, false);
						}
						else if (this.IsServer)
						{
							this.Write("Notice", e.Text);
						}
					});
			}
		}

		private void Session_PrivateMessaged(object sender, IrcDialogEventArgs e)
		{
			if (!this.IsServer)
			{
				if ((this.Target.Type == IrcTargetType.Channel && this.Target.Equals(e.To)) ||
					(this.Target.Type == IrcTargetType.Nickname && this.Target.Equals(new IrcTarget(e.From))))
				{
					this.BeginInvoke(() =>
						{
							bool attn = false;
							if (App.IsAttentionMatch(this.Session.Nickname, e.Text))
							{
								attn = true;
								if (_window != null)
								{
									Interop.WindowHelper.FlashWindow(_window);
								}
							}

							this.Write("Default", e.From, e.Text, attn);
							if (this.Target.Type == IrcTargetType.Nickname)
							{
								if (e.From.Prefix != _prefix)
								{
									_prefix = e.From.Prefix;
									this.SetTitle();
								}
							}

							if (this.Target.Type == IrcTargetType.Nickname)
							{
								Interop.WindowHelper.FlashWindow(_window);
							}
						});
				}
			}
		}

		private void Session_Kicked(object sender, IrcKickEventArgs e)
		{
			this.BeginInvoke(() =>
				{
					if (e.IsSelfKicked && this.IsServer)
					{
						this.Write("Kick", string.Format("You have been kicked from {0} by {1} ({2})",
							e.Channel, e.Kicker.Nickname, e.Text));
					}
					else if (!this.IsServer && this.Target.Equals(e.Channel))
					{
						this.Write("Kick", string.Format("{0} has been kicked by {1} ({2})",
							e.KickeeNickname, e.Kicker.Nickname, e.Text));
						this.RemoveNick(e.KickeeNickname);
					}
				});
		}

		private void Session_InfoReceived(object sender, IrcInfoEventArgs e)
		{
			this.BeginInvoke(() =>
				{
					switch (e.Code)
					{
						case IrcCode.NicknameInUse:
							if (this.IsServer && this.Session.State == IrcSessionState.Connecting)
							{
								if (_usingAlternateNick)
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
						case IrcCode.Topic:
							if (e.Message.Parameters.Count == 3 && !this.IsServer &&
								this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
							{
								_topic = e.Message.Parameters[2];
								this.SetTitle();
								this.Write("Topic", string.Format("Topic is: {0}", _topic));
							}
							return;
						case IrcCode.TopicSetBy:
							if (e.Message.Parameters.Count == 4 && !this.IsServer &&
								this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
							{
								this.Write("Topic", string.Format("Topic set by {0} on {1}", e.Message.Parameters[2],
									this.FormatTime(e.Message.Parameters[3])));
							}
							return;
						case IrcCode.ChannelCreatedOn:
							if (e.Message.Parameters.Count == 3 && !this.IsServer &&
								this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
							{
								//this.Write("ServerInfo", string.Format("* Channel created on {0}", this.FormatTime(e.Message.Parameters[2])));
							}
							return;
						case IrcCode.WhoisUser:
						case IrcCode.WhoWas:
							if (e.Message.Parameters.Count == 6 && this.IsDefault)
							{
								this.Write("ServerInfo",
									string.Format("{1} " + (e.Code == IrcCode.WhoWas ? "was" : "is") + " {2}@{3} {4} {5}",
									(object[])e.Message.Parameters));
								return;
							}
							break;
						case IrcCode.WhoisChannels:
							if (e.Message.Parameters.Count == 3 && this.IsDefault)
							{
								this.Write("ServerInfo", string.Format("{1} is on {2}",
									(object[])e.Message.Parameters));
								return;
							}
							break;
						case IrcCode.WhoisServer:
							if (e.Message.Parameters.Count == 4 && this.IsDefault)
							{
								this.Write("ServerInfo", string.Format("{1} using {2} {3}",
									(object[])e.Message.Parameters));
								return;
							}
							break;
						case IrcCode.WhoisIdle:
							if (e.Message.Parameters.Count == 5 && this.IsDefault)
							{
								this.Write("ServerInfo", string.Format("{0} has been idle {1}, signed on {2}",
									e.Message.Parameters[1], this.FormatTimeSpan(e.Message.Parameters[2]),
									this.FormatTime(e.Message.Parameters[3])));
								return;
							}
							break;
					}

					if ((int)e.Code < 200 && this.IsServer || this.IsDefault)
					{
						this.Write("ServerInfo", e.Text);
					}
				});
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
						!((ChatWindow)_window).Items.Any((item) => item.IsVisible && item.Control.Session == this.Session) &&
						!App.Current.Windows.OfType<ChannelWindow>().Any((cw) => cw.Control.Session == this.Session && cw.IsActive))
					{
						return true;
					}
				}

				return false;
			}
		}

		private void Session_CtcpCommandReceived(object sender, CtcpEventArgs e)
		{
			this.BeginInvoke(() =>
				{
					if ((this.IsChannel && this.Target.Equals(e.To)) ||
						(this.IsNickname && this.Target.Equals(new IrcTarget(e.From)))
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
				});
		}

		private void Session_Joined(object sender, IrcChannelEventArgs e)
		{
			if (!e.IsSelf && !this.IsServer && this.Target.Equals(e.Channel))
			{
				this.BeginInvoke(() =>
					{
						this.Write("Join", string.Format("{0} ({1}@{2}) has joined channel {3}",
							e.Who.Nickname, e.Who.Username, e.Who.Hostname, this.Target.ToString()));
						this.AddNick(ChannelLevel.Normal, e.Who.Nickname);
					});
			}
		}

		private void Session_Parted(object sender, IrcChannelEventArgs e)
		{
			if (!e.IsSelf && !this.IsServer && this.Target.Equals(e.Channel))
			{
				this.BeginInvoke(() =>
					{
						this.Write("Part", string.Format("{0} ({1}@{2}) has left channel {3}",
							e.Who.Nickname, e.Who.Username, e.Who.Hostname, this.Target.ToString()));
						this.RemoveNick(e.Who.Nickname);
					});
			}
		}

		private void Session_NickChanged(object sender, IrcNickEventArgs e)
		{
			this.BeginInvoke(() =>
				{
					if (e.IsSelf)
					{
						if (this.IsServer || this.IsChannel)
						{
							this.Write("Nick", string.Format("You are now known as {0}", e.NewNickname));
						}
						this.SetTitle();
					}
					else if (this.IsChannel && this.IsPresent(e.OldNickname))
					{
						this.Write("Nick", string.Format("{0} is now known as {1}", e.OldNickname, e.NewNickname));
					}

					if (this.IsChannel && this.IsPresent(e.OldNickname))
					{
						this.ChangeNick(e.OldNickname, e.NewNickname);
					}
				});
		}

		private void Session_TopicChanged(object sender, IrcChannelEventArgs e)
		{
			if (!this.IsServer && this.Target.Equals(e.Channel))
			{
				this.BeginInvoke(() =>
					{
						this.Write("Topic", string.Format("{0} changed topic to: {1}", e.Who.Nickname, e.Text));
						_topic = e.Text;
						this.SetTitle();
					});
			}
		}

		private void Session_UserModeChanged(object sender, IrcUserModeEventArgs e)
		{
			this.BeginInvoke(() =>
				{
					if (this.IsServer)
					{
						this.Write("Mode", string.Format("You set mode: {0}", IrcUserMode.RenderModes(e.Modes)));
					}
					this.SetTitle();
				});
		}

		private void Session_UserQuit(object sender, IrcQuitEventArgs e)
		{
			this.BeginInvoke(() =>
				{
					if (this.IsChannel && this.IsPresent(e.Who.Nickname))
					{
						this.Write("Quit", string.Format("{0} has quit ({1})", e.Who.Nickname, e.Text));
						this.RemoveNick(e.Who.Nickname);
					}
				});
		}

		private void Session_ChannelModeChanged(object sender, IrcChannelModeEventArgs e)
		{
			if (!this.IsServer && this.Target.Equals(e.Channel))
			{
				this.BeginInvoke(() =>
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
					});
			}
		}

		private void Session_RawMessageReceived(object sender, IrcEventArgs e)
		{
			int code;
			if(int.TryParse(e.Message.Command, out code))
			{
				var ircCode = (IrcCode)code;
				switch(ircCode)
				{
					case IrcCode.NameReply:
						if (!_hasNames && e.Message.Parameters.Count >= 3 && this.IsChannel)
						{
							var target = new IrcTarget(e.Message.Parameters[e.Message.Parameters.Count - 2]);
							if (this.Target.Equals(target))
							{
								this.Invoke(() =>
									{
										foreach (var nick in e.Message.Parameters[e.Message.Parameters.Count - 1].Split(' '))
										{
											this.AddNick(nick);
										}
									});
								e.Handled = true;
							}
						}
						break;
					case IrcCode.EndOfNames:
						if (!_hasNames && this.IsChannel)
						{
							_hasNames = true;
							e.Handled = true;
						}
						break;
					case IrcCode.ChannelModes:
						if (!_hasModes && e.Message.Parameters.Count == 3 && this.IsChannel &&
							this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
						{
							this.Invoke(() =>
								{
									_channelModes = e.Message.Parameters[2].ToCharArray().Where((c) => c != '+').ToArray();
									this.SetTitle();
								});
							e.Handled = true;
						}
						break;
				}
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
					if (!string.IsNullOrEmpty(txtInput.SelectedText))
					{
						int pos = txtInput.CaretIndex;
						txtInput.SelectedText = new string((char)(c+0x2500), 1);
						txtInput.CaretIndex = pos;
					}
					else
					{
						int pos = txtInput.CaretIndex;
						txtInput.Text = txtInput.Text.Insert(txtInput.CaretIndex, new string((char)(c+0x2500), 1));
						txtInput.CaretIndex = pos + 1;
					}
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

					this.BeginInvoke(() =>
					{
						var parts = text.Split(Environment.NewLine.ToCharArray()).Where((s) => s.Trim().Length > 0).ToArray();
						if (parts.Length > App.Settings.Current.Buffer.MaximumPasteLines)
						{
							if(!App.Confirm(_window, string.Format("Are you sure you want to paste more than {0} lines?",
								App.Settings.Current.Buffer.MaximumPasteLines), "Paste Warning"))
							{
								return;
							}
						}
						foreach (var part in parts)
						{
							txtInput.Text = txtInput.Text.Substring(0, txtInput.SelectionStart);
							txtInput.Text += part;
							this.SubmitInput();
						}
					});
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
					ChatWindow.ChatCommand.Execute(link, this);
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
			if (_window == null)
			{
				_window = Window.GetWindow(this);
				if (_window != null)
				{
					_window.Deactivated += new EventHandler(_window_Deactivated);
				}
			}
			this.NotifyState = NotifyState.None;
		}

		private void ChatControl_Unloaded(object sender, RoutedEventArgs e)
		{
			_hasDeactivated = true;
			this.SelectedLink = null;
			if (_window != null)
			{
				_window.Deactivated -= new EventHandler(_window_Deactivated);
			}
			_window = null;
		}

		protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
		{
			this.SelectedLink = null;
			base.OnPreviewMouseRightButtonDown(e);
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
			{
				return;
			}

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
					break;
				case Key.Down:
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
					break;
				default:
					Keyboard.Focus(txtInput);
					e.Handled = false;
					break;
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
			this.Session.Noticed += new EventHandler<IrcDialogEventArgs>(Session_Noticed);
			this.Session.PrivateMessaged += new EventHandler<IrcDialogEventArgs>(Session_PrivateMessaged);
			this.Session.Kicked += new EventHandler<IrcKickEventArgs>(Session_Kicked);
			this.Session.InfoReceived += new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
			this.Session.CtcpCommandReceived += new EventHandler<CtcpEventArgs>(Session_CtcpCommandReceived);
			this.Session.Joined += new EventHandler<IrcChannelEventArgs>(Session_Joined);
			this.Session.Parted += new EventHandler<IrcChannelEventArgs>(Session_Parted);
			this.Session.NickChanged += new EventHandler<IrcNickEventArgs>(Session_NickChanged);
			this.Session.TopicChanged += new EventHandler<IrcChannelEventArgs>(Session_TopicChanged);
			this.Session.UserModeChanged += new EventHandler<IrcUserModeEventArgs>(Session_UserModeChanged);
			this.Session.ChannelModeChanged += new EventHandler<IrcChannelModeEventArgs>(Session_ChannelModeChanged);
			this.Session.UserQuit += new EventHandler<IrcQuitEventArgs>(Session_UserQuit);
			this.Session.RawMessageReceived += new EventHandler<IrcEventArgs>(Session_RawMessageReceived);
			DataObject.AddPastingHandler(txtInput, new DataObjectPastingEventHandler(txtInput_Pasting));

			this.Loaded += (sender, e) =>
			{
				Keyboard.Focus(txtInput);
				this.SetTitle();
			};

			this.IsConnected = !(this.Session.State == IrcSessionState.Disconnected);
		}

		private void UnsubscribeEvents()
		{
			this.Session.StateChanged -= new EventHandler<EventArgs>(Session_StateChanged);
			this.Session.Noticed -= new EventHandler<IrcDialogEventArgs>(Session_Noticed);
			this.Session.PrivateMessaged -= new EventHandler<IrcDialogEventArgs>(Session_PrivateMessaged);
			this.Session.Kicked -= new EventHandler<IrcKickEventArgs>(Session_Kicked);
			this.Session.InfoReceived -= new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
			this.Session.CtcpCommandReceived -= new EventHandler<CtcpEventArgs>(Session_CtcpCommandReceived);
			this.Session.Joined -= new EventHandler<IrcChannelEventArgs>(Session_Joined);
			this.Session.Parted -= new EventHandler<IrcChannelEventArgs>(Session_Parted);
			this.Session.NickChanged -= new EventHandler<IrcNickEventArgs>(Session_NickChanged);
			this.Session.TopicChanged -= new EventHandler<IrcChannelEventArgs>(Session_TopicChanged);
			this.Session.UserModeChanged -= new EventHandler<IrcUserModeEventArgs>(Session_UserModeChanged);
			this.Session.ChannelModeChanged -= new EventHandler<IrcChannelModeEventArgs>(Session_ChannelModeChanged);
			this.Session.UserQuit -= new EventHandler<IrcQuitEventArgs>(Session_UserQuit);
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
