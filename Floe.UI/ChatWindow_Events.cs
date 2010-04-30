using System;
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
				this.Dispatcher.BeginInvoke((Action)(() =>
				{
					this.AddPage(new ChatContext((IrcSession)sender, e.Channel));
				}));
			}
		}

		private void Session_Parted(object sender, IrcChannelEventArgs e)
		{
			if (e.IsSelf)
			{
				this.Dispatcher.BeginInvoke((Action)(() =>
				{
					var page = this.FindPage((IrcSession)sender, e.Channel);
					if (page != null)
					{
						this.RemovePage(page);
					}
				}));
			}
		}

		private void Session_Kicked(object sender, IrcKickEventArgs e)
		{
			if (e.IsSelfKicked)
			{
				this.Dispatcher.BeginInvoke((Action)(() =>
				{
					var page = this.FindPage((IrcSession)sender, e.Channel);
					if (page != null)
					{
						this.RemovePage(page);
					}
				}));
			}
		}

		private void Session_StateChanged(object sender, EventArgs e)
		{
			if (((IrcSession)sender).State == IrcSessionState.Connecting)
			{
				foreach (var p in (from i in this.Items
								   where i.Content.Context.Session == sender && i.Content.Context.Target != null
								   select i.Content).ToArray())
				{
					this.RemovePage(p);
				}
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

		private void Session_UserModeChanged(object sender, IrcUserModeEventArgs e)
		{
			_userModes = (from m in e.Modes.Where((newMode) => newMode.Set).Select((newMode) => newMode.Mode).Union(_userModes).Distinct()
						  where !e.Modes.Any((newMode) => !newMode.Set)
						  select m).ToArray();
		}
	}
}
