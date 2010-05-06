﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

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
										   where i.Content.Context.Session == sender && i.Content.Context.Target != null
										   select i.Content).ToArray())
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

		private void chatControl_Query(object sender, QueryEventArgs e)
		{
			var control = e.OriginalSource as ChatControl;
			if (control != null)
			{
				var target = new IrcTarget(e.Nickname);
				var context = this.FindPage(control.Session, target);
				this.BeginInvoke(() =>
					{
						if (context != null)
						{
							this.SwitchToPage(context);
						}
						else
						{
							this.AddPage(new ChatContext(control.Session, target), true);
						}
					});
			}
		}

		private void tabsChat_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (tabsChat.SelectedItem != null)
			{
				var context = ((ChatTabItem)tabsChat.SelectedItem).Content.Context;
				foreach (var item in this.Items)
				{
					bool isDefault = false;
					if (item == tabsChat.SelectedItem ||
						item.Content.Context.Session != context.Session && item.Content.IsServer)
					{
						isDefault = true;
					}

					if (item.Content.IsDefault != isDefault)
					{
						item.Content.IsDefault = isDefault;
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
			this.AddHandler(ChatControl.QueryEvent, new QueryEventHandler(chatControl_Query));
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