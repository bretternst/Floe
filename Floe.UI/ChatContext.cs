using System;
using System.Windows;
using Floe.Net;

namespace Floe.UI
{
	public sealed class ChatContext : DependencyObject
	{
		public IrcSession Session { get; private set; }

		public IrcTarget Target { get; private set; }

		public ChatContext(IrcSession ircSession, IrcTarget target)
		{
			this.Session = ircSession;
			this.Target = target;
		}

		public string Key
		{
			get
			{
				if (this.Target == null)
				{
					return "Server";
				}
				else
				{
					return string.Format("{0}.{1}", this.Session.NetworkName, this.Target.Name.ToLowerInvariant()).ToLowerInvariant();
				}
			}
		}
	}
}
