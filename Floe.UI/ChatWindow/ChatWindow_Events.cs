using System;
using System.Linq;
using System.Windows;

using Floe.Configuration;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatWindow : Window
	{
		private char[] _userModes = new char[0];

		private void Session_SelfJoined(object sender, IrcJoinEventArgs e)
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

		private void Session_SelfParted(object sender, IrcPartEventArgs e)
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

		private void Session_SelfKicked(object sender, IrcKickEventArgs e)
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
							if (App.Create(sender as IrcSession, new IrcTarget((IrcPeer)e.Message.From), false)
								&& _notifyIcon != null && _notifyIcon.IsVisible)
							{
								_notifyIcon.Show("IRC Message", string.Format("You received a message from {0}.", ((IrcPeer)e.Message.From).Nickname));
							}
						});
				}
			}
		}

		private void ChatWindow_Loaded(object sender, RoutedEventArgs e)
		{
			this.AddPage(new ChatContext(new IrcSession(), null), true);

			if (Application.Current.MainWindow == this)
			{
				if (App.Settings.IsFirstLaunch)
				{
					App.ShowSettings();
				}

				int i = 0;
				foreach (var server in from ServerElement s in App.Settings.Current.Servers
									   where s.ConnectOnStartup == true
									   select s)
				{
					if (i++ > 0)
					{
						this.AddPage(new ChatContext(new IrcSession(), null), false);
					}
					this.Items[this.Items.Count - 1].Control.Connect(server);
				}
			}
		}

		private void NotifyQuit_Click(object sender, RoutedEventArgs e)
		{
			this.ExecuteClose(sender, null);
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);

			if (!_isShuttingDown && !App.Settings.Current.Windows.SuppressWarningOnQuit && this.Items.Any((i) => i.Control.IsConnected))
			{
				if (!this.ConfirmQuit("Are you sure you want to exit?", "Confirm Exit"))
				{
					e.Cancel = true;
					return;
				}
			}

			this.QuitAllSessions();

			foreach (var page in this.Items)
			{
				page.Control.Dispose();
			}

			foreach (var win in App.Current.Windows)
			{
				var channelWindow = win as ChannelWindow;
				if (channelWindow != null)
				{
					channelWindow.Close();
				}
			}

			App.Settings.Current.Windows.Placement = Interop.WindowHelper.Save(this);

			if (_notifyIcon != null)
			{
				_notifyIcon.Dispose();
			}
		}

		private void SubscribeEvents(IrcSession session)
		{
			session.SelfJoined += new EventHandler<IrcJoinEventArgs>(Session_SelfJoined);
			session.SelfParted += new EventHandler<IrcPartEventArgs>(Session_SelfParted);
			session.SelfKicked += new EventHandler<IrcKickEventArgs>(Session_SelfKicked);
			session.StateChanged += new EventHandler<EventArgs>(Session_StateChanged);
			session.CtcpCommandReceived += new EventHandler<CtcpEventArgs>(Session_CtcpCommandReceived);
			session.RawMessageReceived += new EventHandler<IrcEventArgs>(session_RawMessageReceived);
		}

		public void UnsubscribeEvents(IrcSession session)
		{
			session.SelfJoined -= new EventHandler<IrcJoinEventArgs>(Session_SelfJoined);
			session.SelfParted -= new EventHandler<IrcPartEventArgs>(Session_SelfParted);
			session.SelfKicked -= new EventHandler<IrcKickEventArgs>(Session_SelfKicked);
			session.StateChanged -= new EventHandler<EventArgs>(Session_StateChanged);
			session.CtcpCommandReceived -= new EventHandler<CtcpEventArgs>(Session_CtcpCommandReceived);
			session.RawMessageReceived -= new EventHandler<IrcEventArgs>(session_RawMessageReceived);
		}
	}
}
