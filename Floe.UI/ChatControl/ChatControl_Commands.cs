using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Text;
using System.Windows.Input;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatControl : UserControl
	{
		private const char CommandChar = '/';

		public readonly static RoutedUICommand WhoisCommand = new RoutedUICommand("Whois", "Whois", typeof(ChatControl));
		public readonly static RoutedUICommand OpenLinkCommand = new RoutedUICommand("Open", "OpenLink", typeof(ChatControl));
		public readonly static RoutedUICommand CopyLinkCommand = new RoutedUICommand("Copy", "CopyLink", typeof(ChatControl));
		public readonly static RoutedUICommand QuitCommand = new RoutedUICommand("Quit", "Quit", typeof(ChatControl));

		private void CanExecuteConnectedCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.IsConnected;
		}

		private void ExecuteWhois(object sender, ExecutedRoutedEventArgs e)
		{
			var s = e.Parameter as string;
			if (!string.IsNullOrEmpty(s))
			{
				this.Session.WhoIs(s);
			}
		}

		private void ExecuteOpenLink(object sender, ExecutedRoutedEventArgs e)
		{
			var s = e.Parameter as string;
			if (!string.IsNullOrEmpty(s))
			{
				App.BrowseTo(s);
			}
		}

		private void ExecuteCopyLink(object sender, ExecutedRoutedEventArgs e)
		{
			var s = e.Parameter as string;
			if (!string.IsNullOrEmpty(s))
			{
				Clipboard.SetText(s);
			}
		}

		private void ExecuteQuit(object sender, RoutedEventArgs e)
		{
			try
			{
				this.Session.AutoReconnect = false;
				this.Session.Quit("Leaving");
			}
			catch { }
		}

		private void Execute(string text)
		{
			var chars = text.ToCharArray();
			for (int i = 0; i < chars.Length; i++)
			{
				if (chars[i] >= 0x2500 && chars[i] <= 0x2520)
				{
					chars[i] = (char)((int)chars[i] - 0x2500);
				}
			}
			text = new string(chars);

			string command = text.Trim();

			if (command.Length > 0 && command[0] == CommandChar && !Keyboard.IsKeyToggled(Key.Scroll))
			{
				string args = string.Empty;
				command = command.Substring(1).TrimStart();
				int spaceIdx = command.IndexOf(' ');
				if (spaceIdx > 0)
				{
					args = command.Substring(spaceIdx + 1);
					command = command.Substring(0, spaceIdx);
				}
				if (command.Length > 0)
				{
					this.Execute(command.ToUpperInvariant(), args);
				}
			}
			else
			{
				if (text.Trim().Length > 0)
				{
					if (!this.IsServer)
					{
						this.Session.PrivateMessage(this.Target, text);
						this.Write("Own", 0, this.Session.Nickname, text);
					}
					else
					{
						this.Write("Error", "Can't talk in this window.");
					}
				}
			}
		}

		private void Execute(string command, string arguments)
		{
			string[] args;

			switch (command)
			{
				case "QUIT":
					args = Split(command, arguments, 0, 1);
					this.Session.AutoReconnect = false;
					this.Session.Quit(args.Length == 0 ? "Leaving" : args[0]);
					break;
				case "NICK":
					args = Split(command, arguments, 1, 1);
					this.Session.Nick(args[0]);
					break;
				case "NOTICE":
					args = Split(command, arguments, 2, 2);
					this.Session.Notice(new IrcTarget(args[0]), args[1]);
					break;
				case "JOIN":
				case "J":
					args = Split(command, arguments, 1, 1);
					this.Session.Join(args[0]);
					break;
				case "PART":
				case "LEAVE":
					args = Split(command, arguments, 1, 1);
					this.Session.Part(args[0]);
					break;
				case "TOPIC":
					args = Split(command, arguments, 2, 2);
					this.Session.Topic(args[0], args[1]);
					break;
				case "INVITE":
					args = Split(command, arguments, 2, 2);
					this.Session.Invite(args[1], args[0]);
					break;
				case "KICK":
					args = Split(command, arguments, 2, 2);
					this.Session.Kick(args[0], args[1]);
					break;
				case "MOTD":
					args = Split(command, arguments, 0, 1);
					if (args.Length > 0)
					{
						this.Session.Motd(args[0]);
					}
					else
					{
						this.Session.Motd();
					}
					break;
				case "WHO":
					args = Split(command, arguments, 1, 1);
					this.Session.Who(args[0]);
					break;
				case "WHOIS":
					args = Split(command, arguments, 1, 2);
					if (args != null)
					{
						if (args.Length == 2)
						{
							this.Session.WhoIs(args[0], args[1]);
						}
						else if (args.Length == 1)
						{
							this.Session.WhoIs(args[0]);
						}
					}
					break;
				case "WHOWAS":
					args = Split(command, arguments, 1, 1);
					this.Session.WhoWas(args[0]);
					break;
				case "AWAY":
					args = Split(command, arguments, 0, 1);
					if (args.Length > 0)
					{
						this.Session.Away(args[0]);
					}
					else
					{
						this.Session.UnAway();
					}
					break;
				case "USERHOST":
					args = Split(command, arguments, 1, int.MaxValue);
					this.Session.UserHost(args);
					break;
				case "MODE":
					args = Split(command, arguments, 1, 2);
					var target = new IrcTarget(args[0]);
					if (target.Type == IrcTargetType.Nickname)
					{
						if (args.Length > 1)
						{
							this.Session.Mode(args[1]);
						}
						else
						{
							this.Session.Mode("");
						}
					}
					else
					{
						if (args.Length > 1)
						{
							this.Session.Mode(target.Name, args[1]);
						}
						else
						{
							this.Session.Mode(target.Name, "");
						}
					}
					break;
				case "SERVER":
					args = Split(command, arguments, 1, 2);
					int port = 0;
					if (args.Length > 1)
					{
						int.TryParse(args[1], out port);
					}
					if (port == 0)
					{
						port = 6667;
					}
					if (this.IsConnected)
					{
						this.Session.AutoReconnect = false;
						this.Session.Quit("Changing servers");
					}
					this.Perform = "";
					this.Connect(args[0], port, false);
					break;
				case "ME":
				case "ACTION":
					if (this.IsServer)
					{
						this.Write("Error", "Can't talk in this window.");
					}
					args = Split(command, arguments, 1, int.MaxValue);
					this.Session.SendCtcp(this.Target,
						new CtcpCommand("ACTION", args), false);
					this.Write("Own", string.Format("{0} {1}", this.Session.Nickname, string.Join(" ", args)));
					break;
				case "SETUP":
					App.ShowSettings();
					break;
				case "CLEAR":
					boxOutput.Clear();
					break;
				case "MSG":
					args = Split(command, arguments, 2, 2);
					this.Session.PrivateMessage(new IrcTarget(args[0]), args[1]);
					this.Write("Own", string.Format("-> [{0}] {1}", args[0], args[1]));
					break;
				case "LIST":
					args = Split(command, arguments, 1, 2);
					if (args.Length > 1)
					{
						this.Session.List(args[0], args[1]);
					}
					else
					{
						this.Session.List(args[0]);
					}
					break;
				case "OP":
				case "DEOP":
				case "VOICE":
				case "DEVOICE":
					if (!this.IsChannel)
					{
						this.Write("Error", "Cannot perform that action in this window.");
					}
					else
					{
						char mode = command == "OP" || command == "DEOP" ? 'o' : 'v';
						args = Split(command, arguments, 1, int.MaxValue);
						var modes = from s in args
									select new IrcChannelMode(command == "OP", mode, s);
						this.Session.Mode(this.Target.Name, modes);
					}
					break;
				case "HELP":
					foreach (var s in App.HelpText.Split(Environment.NewLine.ToCharArray()))
					{
						if (s.Length > 0)
						{
							this.Write("Client", s);
						}
					}
					break;
				default:
					this.Write("Error", string.Format("Unrecognized command: {0}", command));
					break;
			}
		}

		private string[] Split(string command, string args, int minArgs, int maxArgs)
		{
			string[] parts = ChatControl.Split(args, maxArgs);
			if (parts.Length < minArgs)
			{
				throw new IrcException(string.Format("{0} requires {1} parameters.", command, minArgs));
			}
			return parts;
		}

		private static string[] Split(string str, int maxParts)
		{
			if (maxParts == 1)
			{
				str = str.Trim();
				return str.Length > 0 ? new[] { str } : new string[0];
			}

			var parts = new List<string>();
			var part = new StringBuilder();

			for (int i = 0; i < str.Length; i++)
			{
				if (maxParts == 1)
				{
					string remainder = str.Substring(i).Trim();
					if (remainder.Length > 0)
					{
						parts.Add(remainder);
					}
					break;
				}

				if (str[i] == ' ')
				{
					if (part.Length > 0)
					{
						parts.Add(part.ToString());
						part.Length = 0;
						--maxParts;
					}
				}
				else
				{
					part.Append(str[i]);
				}
			}
			if (part.Length > 0)
			{
				parts.Add(part.ToString());
			}

			return parts.ToArray();
		}
	}

	public class QueryEventArgs : RoutedEventArgs
	{
		public string Nickname { get; private set; }

		public QueryEventArgs(RoutedEvent evt, string nickname)
			: base(evt)
		{
			this.Nickname = nickname;
		}
	}

	public delegate void QueryEventHandler(object sender, QueryEventArgs e);
}
