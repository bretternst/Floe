using System;
using System.Linq;
using System.Windows;

using Floe.Net;

namespace Floe.UI
{
	public partial class ChatWindow : Window
	{
		private char[] _userModes = new char[0];

		private void Session_Joined(object sender, IrcChannelEventArgs e)
		{
			if (e.IsSelf)
			{
				this.Invoke(() =>
				{
					var context = new ChatContext((IrcSession)sender, e.Channel);
					var state = App.Settings.Current.Windows.States[context.Key];
					if (state.IsDetached)
					{
						var window = new ChannelWindow(new ChatControl(context));
						window.Show();
					}
					else
					{
						this.AddPage(context, true);
					}
				});
			}
		}

		private void Session_Parted(object sender, IrcChannelEventArgs e)
		{
			if (e.IsSelf)
			{
				this.Invoke(() =>
				{
					var context = this.FindPage((IrcSession)sender, e.Channel);
					if (context != null)
					{
						this.RemovePage(context);
					}
				});
			}
		}

		private void Session_Kicked(object sender, IrcKickEventArgs e)
		{
			if (e.IsSelfKicked)
			{
				this.Invoke(() =>
				{
					var context = this.FindPage((IrcSession)sender, e.Channel);
					if (context != null)
					{
						this.RemovePage(context);
					}
				});
			}
		}

		private void Session_StateChanged(object sender, EventArgs e)
		{
			if (((IrcSession)sender).State == IrcSessionState.Connecting)
			{
				this.Invoke(() =>
					{
						foreach (var p in (from i in this.Items
										   where i.Control.Context.Session == sender && i.Control.Context.Target != null
										   select i.Control).ToArray())
						{
							this.RemovePage(p.Context);
						}
					});
			}
		}

		private void Session_CtcpCommandReceived(object sender, CtcpEventArgs e)
		{
			if (App.IsIgnoreMatch(e.From))
			{
				return;
			}

			var session = sender as IrcSession;

			if (!e.IsResponse)
			{
				switch (e.Command.Command)
				{
					case "VERSION":
						session.SendCtcp(new IrcTarget(e.From), new CtcpCommand(
							"VERSION",
							App.Product,
							App.Version), true);
						break;
					case "PING":
						session.SendCtcp(new IrcTarget(e.From), new CtcpCommand(
							"PONG",
							e.Command.Arguments.Length > 0 ? e.Command.Arguments[0] : null), true);
						break;
					case "CLIENTINFO":
						session.SendCtcp(new IrcTarget(e.From), new CtcpCommand(
							"CLIENTINFO",
							"VERSION", "PING", "CLIENTINFO", "ACTION"), true);
						break;
				}
			}
		}

		private void session_RawMessageReceived(object sender, IrcEventArgs e)
		{
			if (e.Message.Command == "PRIVMSG" && e.Message.Parameters.Count == 2
				&& (!CtcpCommand.IsCtcpCommand(e.Message.Parameters[1]) ||
				CtcpCommand.Parse(e.Message.Parameters[1]).Command == "ACTION"))
			{
				if (App.IsIgnoreMatch(e.Message.From))
				{
					return;
				}
				var target = new IrcTarget(e.Message.Parameters[0]);
				if (target.Type == IrcTargetType.Nickname && e.Message.From is IrcPeer)
				{
					this.Invoke(() =>
						{
							App.Create(sender as IrcSession, new IrcTarget((IrcPeer)e.Message.From), false);
							Interop.WindowHelper.FlashWindow(this);
						});
				}
			}
		}

		private void SubscribeEvents(IrcSession session)
		{
			session.Joined += new EventHandler<IrcChannelEventArgs>(Session_Joined);
			session.Parted += new EventHandler<IrcChannelEventArgs>(Session_Parted);
			session.Kicked += new EventHandler<IrcKickEventArgs>(Session_Kicked);
			session.StateChanged += new EventHandler<EventArgs>(Session_StateChanged);
			session.CtcpCommandReceived += new EventHandler<CtcpEventArgs>(Session_CtcpCommandReceived);
			session.RawMessageReceived += new EventHandler<IrcEventArgs>(session_RawMessageReceived);
		}

		public void UnsubscribeEvents(IrcSession session)
		{
			session.Joined -= new EventHandler<IrcChannelEventArgs>(Session_Joined);
			session.Parted -= new EventHandler<IrcChannelEventArgs>(Session_Parted);
			session.Kicked -= new EventHandler<IrcKickEventArgs>(Session_Kicked);
			session.StateChanged -= new EventHandler<EventArgs>(Session_StateChanged);
			session.CtcpCommandReceived -= new EventHandler<CtcpEventArgs>(Session_CtcpCommandReceived);
			session.RawMessageReceived -= new EventHandler<IrcEventArgs>(session_RawMessageReceived);
		}
	}
}
