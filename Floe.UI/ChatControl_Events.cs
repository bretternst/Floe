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
		private bool _hasNames = false;

		private void Session_StateChanged(object sender, EventArgs e)
		{
			this.BeginInvoke(() =>
				{
					if (this.Session.State == IrcSessionState.Disconnected)
					{
						this.Write("Error", "* Disconnected");
					}

					if (this.IsServer)
					{
						switch (this.Session.State)
						{
							case IrcSessionState.Connecting:
								this.Write("Client", string.Format(
								"* Connecting to {0}:{1}", this.Session.Server, this.Session.Port));
								break;
							case IrcSessionState.Connected:
								this.Header = this.Session.NetworkName;
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
				this.BeginInvoke(() => this.Write("Error", string.Format("* {0}",
					string.IsNullOrEmpty(e.Exception.Message) ? e.Exception.GetType().Name : e.Exception.Message)));
			}
		}

		private void Session_Noticed(object sender, IrcDialogEventArgs e)
		{
			this.BeginInvoke(() =>
				{
					if (e.From is IrcPeer)
					{
						this.Write("Notice", string.Format("-{0}- {1}", ((IrcPeer)e.From).Nickname, e.Text));
					}
					else if (this.IsServer)
					{
						this.Write("Notice", e.Text);
					}
				});
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
							this.Write("Default", string.Format("<{0}> {1}", e.From.Nickname, e.Text));
							if (this.Target.Type == IrcTargetType.Nickname)
							{
								if (e.From.Prefix != _prefix)
								{
									_prefix = e.From.Prefix;
									this.SetTitle();
								}
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
						this.Write("Kick", string.Format("* You have been kicked from {0} by {1} ({2})",
							e.Channel, e.Kicker.Nickname, e.Text));
					}
					else if (!this.IsServer && this.Target.Equals(e.Channel))
					{
						this.Write("Kick", string.Format("* {0} has been kicked by {1} ({2})",
							e.KickeeNickname, e.Kicker.Nickname, e.Text));
						this.RemoveNick(e.KickeeNickname);
					}
				});
		}

		private void Session_InfoReceived(object sender, IrcInfoEventArgs e)
		{
			var line = new ChatLine("ServerInfo", string.Format("* {0}", e.Text));

			this.BeginInvoke(() =>
				{
					switch (e.Code)
					{
						case IrcCode.NicknameInUse:
							if (this.IsServer && this.Session.State == IrcSessionState.Connecting)
							{
								this.SetInputText("/nick ");
							}
							break;
						case IrcCode.ChannelModes:
							if (e.Message.Parameters.Count == 3 && !this.IsServer &&
								this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
							{
								_channelModes = e.Message.Parameters[2].ToCharArray().Where((c) => c != '+').ToArray();
								this.SetTitle();
								return;
							}
							break;
						case IrcCode.Topic:
							if (e.Message.Parameters.Count == 3 && !this.IsServer &&
								this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
							{
								_topic = e.Message.Parameters[2];
								this.SetTitle();
								this.Write("Topic", string.Format("* Topic is: {0}", _topic));
							}
							return;
						case IrcCode.TopicSetBy:
							if (e.Message.Parameters.Count == 4 && !this.IsServer &&
								this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
							{
								this.Write("Topic", string.Format("* Topic set by {0} on {1}", e.Message.Parameters[2],
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
								this.Write("ServerInfo", string.Format("{1} " + (e.Code == IrcCode.WhoWas ? "was" : "is") + " {2}@{3} {4} {5}",
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
						case IrcCode.NameReply:
							if (!_hasNames && e.Message.Parameters.Count >= 3 && this.IsChannel)
							{
								var target = new IrcTarget(e.Message.Parameters[e.Message.Parameters.Count - 2]);
								if (this.Target.Equals(target))
								{
									foreach (var nick in e.Message.Parameters[e.Message.Parameters.Count - 1].Split(' '))
									{
										this.AddNick(nick);
									}
									return;
								}
							}
							break;
						case IrcCode.EndOfNames:
							if (this.IsChannel && !_hasNames)
							{
								_hasNames = true;
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

		private void Session_CtcpCommandReceived(object sender, CtcpEventArgs e)
		{
			this.BeginInvoke(() =>
				{
					if ((this.IsChannel && this.Target.Equals(e.To)) ||
						(this.IsNickname && this.Target.Equals(new IrcTarget(e.From))))
					{
						this.Write("Action", string.Format("* {0} {1}", e.From.Nickname, string.Join(" ", e.Command.Arguments)));
					}
					else if (this.IsServer && e.Command.Command != "ACTION")
					{
						this.Write("Ctcp", string.Format("-{0}- [CTCP {1}] {2}",
							e.From.Nickname, e.Command.Command,
							e.Command.Arguments.Length > 0 ? string.Join(" ", e.Command.Arguments) : ""));
					}
				});
		}

		private void Session_Joined(object sender, IrcChannelEventArgs e)
		{
			if (!e.IsSelf && !this.IsServer && this.Target.Equals(e.Channel))
			{
				this.BeginInvoke(() =>
					{
						this.Write("Join", string.Format("* {0} ({1}@{2}) has joined channel {3}",
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
						this.Write("Part", string.Format("* {0} ({1}@{2}) has left channel {3}",
						e.Who.Nickname, e.Who.Username, e.Who.Hostname, this.Target.ToString()));
						this.RemoveNick(e.Who.Nickname);
					});
			}
		}

		private void Session_NickChanged(object sender, IrcNickEventArgs e)
		{
			this.BeginInvoke(() =>
				{
					if (!this.IsServer)
					{
						this.Write("Nick", string.Format("* {0} is now known as {1}", e.OldNickname, e.NewNickname));
					}
					else if (e.IsSelf)
					{
						this.Write("Nick", string.Format("* You are now known as {0}", e.NewNickname));
						this.SetTitle();
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
						this.Write("Topic", string.Format("* {0} changed topic to: {1}", e.Who.Nickname, e.Text));
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
						this.Write("Mode", string.Format("* You set mode: {0}", IrcUserMode.RenderModes(e.Modes)));
					}
					this.SetTitle();
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
							this.Write("Mode", string.Format("* {0} set mode: {1}", e.Who.Nickname,
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

		private void txtInput_KeyDown(object sender, KeyEventArgs e)
		{
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
							if (MessageBox.Show(string.Format("Are you sure you want to paste more than {0} lines?",
								App.Settings.Current.Buffer.MaximumPasteLines), "Paste Warnings", MessageBoxButton.YesNo,
								MessageBoxImage.Question) == MessageBoxResult.No)
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
				this.BeginInvoke(() => this.Query(nickItem.Nickname));
			}
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
			DataObject.AddPastingHandler(txtInput, new DataObjectPastingEventHandler(txtInput_Pasting));

			this.Loaded += (sender, e) =>
			{
				Keyboard.Focus(txtInput);
				this.SetTitle();
			};
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
			DataObject.RemovePastingHandler(txtInput, new DataObjectPastingEventHandler(txtInput_Pasting));
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
