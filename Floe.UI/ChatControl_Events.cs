using System;
using System.Windows.Controls;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatControl : UserControl
	{
		private bool _welcomeReceived = false;

		private void Session_StateChanged(object sender, EventArgs e)
		{
			if (this.Context.Session.State == IrcSessionState.Disconnected)
			{
				this.Write("Error", "*** Disconnected");
				if (this.Context.Target == null)
				{
					this.Dispatcher.BeginInvoke((Action)(() => this.Header = "Server"));
				}
			}
			else if (this.Context.Session.State == IrcSessionState.Connecting &&
				this.Context.Target == null)
			{
				_welcomeReceived = false;
				this.Dispatcher.BeginInvoke((Action)(() => this.Header = this.Context.Session.Server));
			}
		}

		private void Session_ConnectionError(object sender, ErrorEventArgs e)
		{
			if (this.Context.Target == null)
			{
				this.Write("Error", string.Format("*** {0}",
					string.IsNullOrEmpty(e.Exception.Message) ? e.Exception.GetType().Name : e.Exception.Message));
			}
		}

		private void Session_Noticed(object sender, IrcDialogEventArgs e)
		{
			if (e.From is IrcPeer)
			{
				this.Write("Notice", string.Format("-{0}- {1}", ((IrcPeer)e.From).Nickname, e.Text));
			}
			else if (this.Context.Target == null)
			{
				this.Write("Notice", e.Text);
			}
		}

		private void Session_PrivateMessaged(object sender, IrcDialogEventArgs e)
		{
			if (this.Context.Target != null && this.Context.Target.Equals(e.To))
			{
				this.Write("Default", string.Format("<{0}> {1}", e.From.Nickname, e.Text));
			}
		}

		private void Session_Kicked(object sender, IrcKickEventArgs e)
		{
			if (e.IsSelfKicked && this.Context.Target == null)
			{
				this.Write("Kick", string.Format("* You have been kicked from {0} by {1} ({2})",
					e.Channel, e.Kicker.Nickname, e.Text));
			}
			else if (this.Context.Target != null && this.Context.Target.Equals(e.Channel))
			{
				this.Write("Kick", string.Format("* {0} has been kicked by {1} ({2})",
					e.KickeeNickname, e.Kicker.Nickname, e.Text));
			}
		}

		private void Session_InfoReceived(object sender, IrcInfoEventArgs e)
		{
			var line = new ChatLine("ServerInfo", string.Format("*** {0}", e.Text));

			switch (e.Code)
			{
				case IrcCode.Welcome:
					if (this.Context.Target == null && e.Text.StartsWith("Welcome to the "))
					{
						var parts = e.Text.Split(' ');
						if (parts.Length > 3)
						{
							this.Dispatcher.BeginInvoke((Action)(() => this.Header = parts[3]));
						}
					}
					_welcomeReceived = true;
					break;
				case IrcCode.NicknameInUse:
					if (this.Context.Target == null && !_welcomeReceived)
					{
						this.Dispatcher.BeginInvoke((Action)(() =>
							{
								txtInput.Text = "/NICK "; txtInput.SelectionStart = txtInput.Text.Length;
							}));
					}
					break;
			}

			if ((int)e.Code < 200 && this.Context.Target == null || this.IsVisible)
			{
				this.Write("ServerInfo", e.Text);
			}
		}

		private void Session_CtcpCommandReceived(object sender, CtcpEventArgs e)
		{
			if (this.Context.Target != null && this.Context.Target.Equals(e.To))
			{
				this.Write("Action", string.Format("* {0} {1}", e.From.Nickname, string.Join(" ", e.Command.Arguments)));
			}
			else if (this.Context.Target == null && e.Command.Command != "ACTION")
			{
				this.Write("Ctcp", string.Format("-{0}- [CTCP {1}] {2}",
					e.From.Nickname, e.Command.Command,
					e.Command.Arguments.Length > 0 ? string.Join(" ", e.Command.Arguments) : ""));
			}
		}

		private void Session_Joined(object sender, IrcChannelEventArgs e)
		{
			if (!e.IsSelf && this.Context.Target != null && this.Context.Target.Equals(e.Channel))
			{
				this.Write("Join", string.Format("* {0} ({1}@{2}) has joined channel {3}",
					e.Who.Nickname, e.Who.Username, e.Who.Hostname, this.Context.Target.ToString()));
			}
		}

		private void Session_Parted(object sender, IrcChannelEventArgs e)
		{
			if (!e.IsSelf && this.Context.Target != null && this.Context.Target.Equals(e.Channel))
			{
				this.Write("Part", string.Format("* {0} ({1}@{2}) has left channel {3}",
				e.Who.Nickname, e.Who.Username, e.Who.Hostname, this.Context.Target.ToString()));
			}
		}

		private void Session_NickChanged(object sender, IrcNickEventArgs e)
		{
			if (this.Context.Target != null)
			{
				this.Write("Nick", string.Format("* {0} is now known as {1}", e.OldNickname, e.NewNickname));
			}
		}

		private void Session_TopicChanged(object sender, IrcChannelEventArgs e)
		{
			if (this.Context.Target != null && this.Context.Target.Equals(e.Channel))
			{
				this.Write("Topic", string.Format("* {0} changed topic to: {1}", e.Who.Nickname, e.Text));
			}
		}

		private void Session_UserModeChanged(object sender, IrcUserModeEventArgs e)
		{
			if (this.Context.Target == null)
			{
				this.Write("Mode", string.Format("* You set mode: {0}", IrcUserMode.RenderModes(e.Modes)));
			}
		}

		private void Session_ChannelModeChanged(object sender, IrcChannelModeEventArgs e)
		{
			if (this.Context.Target != null && this.Context.Target.Equals(e.Channel))
			{
				this.Write("Mode", string.Format("* {0} set mode: {1}", e.Who.Nickname,
					string.Join(" ", IrcChannelMode.RenderModes(e.Modes))));
			}
		}

		private void SubscribeEvents()
		{
			this.Context.Session.StateChanged += new EventHandler<EventArgs>(Session_StateChanged);
			this.Context.Session.ConnectionError += new EventHandler<ErrorEventArgs>(Session_ConnectionError);
			this.Context.Session.Noticed += new EventHandler<IrcDialogEventArgs>(Session_Noticed);
			this.Context.Session.PrivateMessaged += new EventHandler<IrcDialogEventArgs>(Session_PrivateMessaged);
			this.Context.Session.Kicked += new EventHandler<IrcKickEventArgs>(Session_Kicked);
			this.Context.Session.InfoReceived += new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
			this.Context.Session.CtcpCommandReceived += new EventHandler<CtcpEventArgs>(Session_CtcpCommandReceived);
			this.Context.Session.Joined += new EventHandler<IrcChannelEventArgs>(Session_Joined);
			this.Context.Session.Parted += new EventHandler<IrcChannelEventArgs>(Session_Parted);
			this.Context.Session.NickChanged += new EventHandler<IrcNickEventArgs>(Session_NickChanged);
			this.Context.Session.TopicChanged += new EventHandler<IrcChannelEventArgs>(Session_TopicChanged);
			this.Context.Session.UserModeChanged += new EventHandler<IrcUserModeEventArgs>(Session_UserModeChanged);
			this.Context.Session.ChannelModeChanged += new EventHandler<IrcChannelModeEventArgs>(Session_ChannelModeChanged);
		}

		private void UnsubscribeEvents()
		{
			this.Context.Session.StateChanged -= new EventHandler<EventArgs>(Session_StateChanged);
			this.Context.Session.Noticed -= new EventHandler<IrcDialogEventArgs>(Session_Noticed);
			this.Context.Session.PrivateMessaged -= new EventHandler<IrcDialogEventArgs>(Session_PrivateMessaged);
			this.Context.Session.Kicked -= new EventHandler<IrcKickEventArgs>(Session_Kicked);
			this.Context.Session.InfoReceived -= new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
			this.Context.Session.CtcpCommandReceived -= new EventHandler<CtcpEventArgs>(Session_CtcpCommandReceived);
			this.Context.Session.Joined -= new EventHandler<IrcChannelEventArgs>(Session_Joined);
			this.Context.Session.Parted -= new EventHandler<IrcChannelEventArgs>(Session_Parted);
			this.Context.Session.NickChanged -= new EventHandler<IrcNickEventArgs>(Session_NickChanged);
			this.Context.Session.TopicChanged -= new EventHandler<IrcChannelEventArgs>(Session_TopicChanged);
			this.Context.Session.UserModeChanged -= new EventHandler<IrcUserModeEventArgs>(Session_UserModeChanged);
			this.Context.Session.ChannelModeChanged -= new EventHandler<IrcChannelModeEventArgs>(Session_ChannelModeChanged);
		}
	}
}
