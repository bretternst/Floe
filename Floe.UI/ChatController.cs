using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Floe.Net;
using System.Windows;

namespace Floe.UI
{
	public sealed class ChatController : DependencyObject
	{
		public IrcSession Session { get; private set; }

		public IrcTarget Target { get; private set; }

		public bool IsConnected { get { return this.Session.State == IrcSessionState.Connecting ||
			this.Session.State == IrcSessionState.Connected; } }

		public event EventHandler<OutputEventArgs> OutputReceived;
		public event EventHandler ChangingServer;

		public ChatController(IrcSession ircSession, IrcTarget target)
		{
			this.Session = ircSession;
			this.Target = target;

			this.Session.StateChanged += new EventHandler<EventArgs>(Session_StateChanged);
			this.Session.Noticed += new EventHandler<IrcDialogEventArgs>(Session_Noticed);
			this.Session.InfoReceived += new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
			this.Session.PrivateMessaged += new EventHandler<IrcDialogEventArgs>(Session_PrivateMessaged);
		}

		public void OnOutput(OutputType type, IrcPeer from, string text)
		{
			Dispatcher.BeginInvoke((Action)(() => {
				var handler = this.OutputReceived;
				if (handler != null)
				{
					handler(this, new OutputEventArgs(type, from, text));
				}
			}));
		}

		public void OnChangeServer()
		{
			Dispatcher.BeginInvoke((Action)(() =>
			{
				var handler = this.ChangingServer;
				if (handler != null)
				{
					handler(this, EventArgs.Empty);
				}
			}));
		}

		public void OnOutput(string text)
		{
			this.OnOutput(OutputType.Client, null, text);
		}

		public override string ToString()
		{
			return this.Target == null ? "Server" : this.Target.ToString();
		}

		private void Session_StateChanged(object sender, EventArgs e)
		{
			if (this.Session.State == IrcSessionState.Disconnected)
			{
				this.OnOutput(OutputType.Disconnected, null, null);
			}
		}

		private void Session_InfoReceived(object sender, IrcInfoEventArgs e)
		{
			if (this.Target == null)
			{
				this.OnOutput(OutputType.Info, null, e.Text);
			}
		}

		private void Session_Noticed(object sender, IrcDialogEventArgs e)
		{
			this.OnOutput(OutputType.Notice, e.From as IrcPeer, e.Text);
		}

		private void Session_PrivateMessaged(object sender, IrcDialogEventArgs e)
		{
			if (this.Target != null && this.Target.Equals(e.To))
			{
				this.OnOutput(OutputType.PrivateMessage, e.From, e.Text);
			}
		}
	}
}
