using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Text;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatControl : UserControl
	{
		private const char CommandChar = '/';

		private void Execute(string text)
		{
			string command = text.Trim();

			if (command.Length > 0 && command[0] == CommandChar)
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
					if (this.Context.Target != null)
					{
						this.Context.Session.PrivateMessage(this.Context.Target, text);
						this.Write("Own", text);
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
					this.Context.Session.Quit(args.Length == 0 ? "Leaving" : args[0]);
					break;
				case "NICK":
					args = Split(command, arguments, 1, 1);
					this.Context.Session.Nick(args[0]);
					break;
				case "NOTICE":
					args = Split(command, arguments, 2, 2);
					this.Context.Session.Notice(new IrcTarget(args[0]), args[1]);
					break;
				case "JOIN":
				case "J":
					args = Split(command, arguments, 1, 1);
					this.Context.Session.Join(args[0]);
					break;
				case "PART":
				case "LEAVE":
					args = Split(command, arguments, 1, 1);
					this.Context.Session.Part(args[0]);
					break;
				case "TOPIC":
					args = Split(command, arguments, 2, 2);
					this.Context.Session.Topic(args[0], args[1]);
					break;
				case "INVITE":
					args = Split(command, arguments, 2, 2);
					this.Context.Session.Invite(args[1], args[0]);
					break;
				case "KICK":
					args = Split(command, arguments, 2, 2);
					this.Context.Session.Kick(args[0], args[1]);
					break;
				case "MOTD":
					args = Split(command, arguments, 0, 1);
					if (args.Length > 0)
					{
						this.Context.Session.Motd(args[0]);
					}
					else
					{
						this.Context.Session.Motd();
					}
					break;
				case "WHO":
					args = Split(command, arguments, 1, 1);
					this.Context.Session.Who(args[0]);
					break;
				case "WHOIS":
					args = Split(command, arguments, 1, 2);
					if (args != null)
					{
						if (args.Length == 2)
						{
							this.Context.Session.WhoIs(args[0], args[1]);
						}
						else if (args.Length == 1)
						{
							this.Context.Session.WhoIs(args[0]);
						}
					}
					break;
				case "WHOWAS":
					args = Split(command, arguments, 1, 1);
					this.Context.Session.WhoWas(args[0]);
					break;
				case "AWAY":
					args = Split(command, arguments, 0, 1);
					if (args.Length > 0)
					{
						this.Context.Session.Away(args[0]);
					}
					else
					{
						this.Context.Session.UnAway();
					}
					break;
				case "USERHOST":
					args = Split(command, arguments, 1, int.MaxValue);
					this.Context.Session.UserHost(args);
					break;
				case "MODE":
					args = Split(command, arguments, 1, 2);
					var target = new IrcTarget(args[0]);
					if (target.Type == IrcTargetType.Nickname)
					{
						if (args.Length > 1)
						{
							this.Context.Session.Mode(args[1]);
						}
						else
						{
							this.Context.Session.Mode("");
						}
					}
					else
					{
						if (args.Length > 1)
						{
							this.Context.Session.Mode(target.Name, args[1]);
						}
						else
						{
							this.Context.Session.Mode(target.Name, "");
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
					if (this.Context.IsConnected)
					{
						this.Context.Session.Quit("Changing servers");
					}
					this.Context.Session.Open(args[0], port,
						!string.IsNullOrEmpty(this.Context.Session.Nickname) ? this.Context.Session.Nickname : App.Settings.Current.User.Nickname);
					break;
				case "ME":
				case "ACTION":
					if (this.Context.Target == null)
					{
						this.Write("Error", "Can't talk in this window.");
					}
					args = Split(command, arguments, 1, int.MaxValue);
					this.Context.Session.SendCtcp(this.Context.Target,
						new CtcpCommand("ACTION", args), false);
					this.Write("Own", string.Format("* {0} {1}", this.Context.Session.Nickname, string.Join(" ", args)));
					break;
				case "SETUP":
					((App)Application.Current).ShowSettings();
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
				this.Write("Error", string.Format("{0} requires {1} parameters.", command, minArgs));
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
						part.Clear();
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
}
