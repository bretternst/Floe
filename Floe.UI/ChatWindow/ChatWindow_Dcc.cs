using System;
using System.Linq;
using System.Windows;
using System.Net;
using System.IO;

using Floe.Configuration;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatWindow : Window
	{
		public void DccSend(IrcSession session, IrcTarget target, FileInfo file)
		{
			var page = new FileControl(session, target);
			int port = page.StartSend(file);
			App.Create(session, page, true);
			if (port > 0)
			{
				session.SendCtcp(target, new CtcpCommand("DCC", "XMIT", "CLEAR", session.ExternalAddress.ToString(), port.ToString(), file.Name, file.Length.ToString()), false);
			}
		}

		private bool HandleDcc(IrcSession session, IrcTarget target, string[] args)
		{
			if (args.Length < 1)
			{
				return false;
			}

			string type = args[0].ToUpperInvariant();

			switch (type)
			{
				case "XMIT":
					if (args.Length < 4)
					{
						return false;
					}

					IPAddress addr;
					int port;
					if (string.Compare(args[1], "CLEAR", StringComparison.OrdinalIgnoreCase) != 0 ||
						!IPAddress.TryParse(args[2], out addr) ||
						!int.TryParse(args[3], out port))
					{
						session.SendCtcp(target, new CtcpCommand("ERRMSG", "DCC", args[0], args[1], "unavailable"), true);
						return true;
					}

					string name = args.Length > 4 ? args[4] : null;
					long size = 0;
					if (args.Length > 5)
					{
						long.TryParse(args[5], out size);
					}

					var page = new FileControl(session, target);
					page.StartReceive(addr, port, name, size);
					page.NotifyState = NotifyState.Alert;
					App.Create(session, page, false);
					App.Alert(Window.GetWindow(page), string.Format("{0} wants to send you a file.", target.Name));
					break;

				default:
					session.SendCtcp(target, new CtcpCommand("ERRMSG", "DCC", args[0], "unavailable"), true);
					break;
			}
			return true;
		}
	}
}
