using System;
using System.IO;
using System.Net;
using System.Windows;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatWindow : Window
	{
		public void DccChat(IrcSession session, IrcTarget target)
		{
			var page = new ChatControl(ChatPageType.DccChat, session, target);
			App.Create(session, page, true);
			page.StartListen((port) =>
				{
					session.SendCtcp(target, new CtcpCommand("DCC", "CHAT", "chat",
						ConvertIPAddressToString(session.ExternalAddress),
						port.ToString()), false);
				});
		}

		public void DccXmit(IrcSession session, IrcTarget target, FileInfo file)
		{
			var page = new FileControl(session, target, DccMethod.Xmit);
			App.Create(session, page, true);
			page.StartSend(file, (port) =>
			{
				if (port > 0)
				{
					session.SendCtcp(target, new CtcpCommand("DCC", "XMIT", "CLEAR",
						ConvertIPAddressToString(session.ExternalAddress),
						port.ToString(), file.Name, file.Length.ToString()), false);
				}
			});
		}

		public void DccSend(IrcSession session, IrcTarget target, FileInfo file)
		{
			var page = new FileControl(session, target, DccMethod.Send);
			App.Create(session, page, true);
			page.StartSend(file, (port) =>
			{
				if (port > 0)
				{
					session.SendCtcp(target, new CtcpCommand("DCC", "SEND", file.Name,
						ConvertIPAddressToString(session.ExternalAddress),
						port.ToString(), file.Length.ToString(), "T"), false);
				}
			});
		}

		private bool HandleDcc(IrcSession session, IrcTarget target, string[] args)
		{
			if (args.Length < 1)
			{
				return false;
			}

			string type = args[0].ToUpperInvariant();
			IPAddress addr;
			int port;

			switch (type)
			{
				case "CHAT":
					{
						if (args.Length < 4 ||
							args[1].ToUpperInvariant() != "CHAT" ||
							!IPAddress.TryParse(args[2], out addr) ||
							!int.TryParse(args[3], out port))
						{
							session.SendCtcp(target, new CtcpCommand("ERRMSG", "DCC", "CHAT", "unavailable"), true);
							return true;
						}

						var page = new ChatControl(ChatPageType.DccChat, session, target);
						page.StartAccept(addr, port);
						page.NotifyState = NotifyState.Alert;
						App.Create(session, page, false);
						App.Alert(Window.GetWindow(page), string.Format("{0} wants to chat with you.", target.Name));
					}
					break;

				case "XMIT":
					{
						if (args.Length < 5 ||
							string.Compare(args[1], "CLEAR", StringComparison.OrdinalIgnoreCase) != 0 ||
							!IPAddress.TryParse(args[2], out addr) ||
							!int.TryParse(args[3], out port))
						{
							session.SendCtcp(target, new CtcpCommand("ERRMSG", "DCC", "XMIT", "unavailable"), true);
							return true;
						}

						string name = args[4];
						long size = 0;
						if (args.Length > 5)
						{
							long.TryParse(args[5], out size);
						}

						var page = new FileControl(session, target, DccMethod.Xmit);
						page.StartReceive(addr, port, name, size);
						page.NotifyState = NotifyState.Alert;
						App.Create(session, page, false);
						App.Alert(Window.GetWindow(page), string.Format("{0} wants to send you a file.", target.Name));
					}
					break;

				case "SEND":
					{
						if (args.Length < 4 ||
							!IPAddress.TryParse(args[2], out addr) ||
							!int.TryParse(args[3], out port))
						{
							session.SendCtcp(target, new CtcpCommand("ERRMSG", "DCC", "SEND", "unavailable"), true);
							return true;
						}

						string name = args[1];
						long size = 0;
						if (args.Length > 4)
						{
							long.TryParse(args[4], out size);
						}

						var page = new FileControl(session, target, DccMethod.Send);
						page.StartReceive(addr, port, name, size);
						page.NotifyState = NotifyState.Alert;
						App.Create(session, page, false);
						App.Alert(Window.GetWindow(page), string.Format("{0} wants to send you a file.", target.Name));
					}
					break;

				default:
					session.SendCtcp(target, new CtcpCommand("ERRMSG", "DCC", args[0], "unavailable"), true);
					break;
			}
			return true;
		}

		private static string ConvertIPAddressToString(IPAddress address)
		{
			return BitConverter.ToUInt32(address.GetAddressBytes(), 0).ToString();
		}
	}
}
