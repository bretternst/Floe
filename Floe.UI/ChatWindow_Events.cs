using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
				this.BeginInvoke(() =>
				{
					this.AddPage(new ChatContext((IrcSession)sender, e.Channel), true);
				});
			}
		}

		private void Session_Parted(object sender, IrcChannelEventArgs e)
		{
			if (e.IsSelf)
			{
				this.BeginInvoke(() =>
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
				this.BeginInvoke(() =>
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
				this.BeginInvoke(() =>
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
			var session = sender as IrcSession;

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

		private void session_RawMessageReceived(object sender, IrcEventArgs e)
		{
			if (e.Message.Command == "PRIVMSG" && e.Message.Parameters.Count == 2)
			{
				var target = new IrcTarget(e.Message.Parameters[0]);
				if (target.Type == IrcTargetType.Nickname && e.Message.From is IrcPeer)
				{
					var session = sender as IrcSession;
					target = new IrcTarget((IrcPeer)e.Message.From);
					var control = this.FindPage(session, target);
					if (control == null)
					{
						this.Invoke(() => this.AddPage(new ChatContext(session, target), false));
					}
				}
			}
		}

		private void tabsChat_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (tabsChat.SelectedItem != null)
			{
				var context = ((ChatTabItem)tabsChat.SelectedItem).Control.Context;
				foreach (var item in this.Items)
				{
					bool isDefault = false;
					if (item == tabsChat.SelectedItem ||
						item.Control.Context.Session != context.Session && item.Control.IsServer)
					{
						isDefault = true;
					}

					if (item.Control.IsDefault != isDefault)
					{
						item.Control.IsDefault = isDefault;
					}
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
