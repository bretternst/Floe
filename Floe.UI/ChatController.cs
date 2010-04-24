using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Floe.Net;

namespace Floe.UI
{
	public sealed class ChatController
	{
		public IrcSession Session { get; private set; }

		public IrcTarget Target { get; private set; }

		public event EventHandler<OutputEventArgs> OutputReceived;

		public ChatController(IrcSession ircSession, IrcTarget target)
		{
			this.Session = ircSession;
			this.Target = target;
		}

		public void OnOutput(IrcMessage message)
		{
			var handler = this.OutputReceived;
			if (handler != null)
			{
				handler(this, new OutputEventArgs(message));
			}
		}

		public void OnOutput(string text)
		{
			var handler = this.OutputReceived;
			if (handler != null)
			{
				handler(this, new OutputEventArgs(text));
			}
		}

		public override string ToString()
		{
			return this.Target == null ? "Server" : this.Target.ToString();
		}
	}
}
