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

		private void Execute(string text)
		{
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
						this.Write("Own", string.Format("<{0}> {1}", this.Session.Nickname, text));
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
					if (this.Context.IsConnected)
					{
						this.Session.Quit("Changing servers");
					}
					this.Session.Open(args[0], port,
						!string.IsNullOrEmpty(this.Session.Nickname) ? 
							this.Session.Nickname : App.Settings.Current.User.Nickname,
						App.Settings.Current.User.Username,
						App.Settings.Current.User.Hostname,
						App.Settings.Current.User.FullName);
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
					this.Write("Own", string.Format("* {0} {1}", this.Session.Nickname, string.Join(" ", args)));
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
