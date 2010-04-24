using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Floe.Net;

namespace Scratchpad
{
	class Program
	{
		static void Main(string[] args)
		{
			var session = new IrcSession();

			session.RawMessageSent += new EventHandler<IrcEventArgs>(session_MessageSent);
			session.RawMessageReceived += new EventHandler<IrcEventArgs>(session_MessageReceived);
			session.StateChanged += new EventHandler<EventArgs>(session_StateChanged);
			session.Open("irc.sorcery.net", 6667, "Test____");
		}

		static void session_StateChanged(object sender, EventArgs e)
		{
			Console.WriteLine("STATE: " + ((IrcSession)sender).State.ToString());
		}

		static void session_MessageReceived(object sender, IrcEventArgs e)
		{
			Console.WriteLine("RECV: " + e.Message.ToString());
		}

		static void session_MessageSent(object sender, IrcEventArgs e)
		{
			Console.WriteLine("SENT: " + e.Message.ToString());
		}
	}
}
