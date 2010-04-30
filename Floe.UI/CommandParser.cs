using System;
using Floe.Net;

namespace Floe.UI
{
	public enum CommandEchoType
	{
		Error,
		Message,
		Action
	}

	public class CommandEcho
	{
		public CommandEchoType Type { get; private set; }
		public string Text { get; private set; }

		public CommandEcho(CommandEchoType type, string text)
		{
			this.Type = type;
			this.Text = text;
		}
	}

	public static class CommandParser
	{
		private const char CommandChar = '/';

		public static CommandEcho Execute(ChatContext context, string text)
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
					return Execute(context, command.ToUpperInvariant(), args);
				}
				else
				{
					return null;
				}
			}
			else
			{
				if (text.Trim().Length < 1)
				{
					return null;
				}

				if (context.Target == null)
				{
					return new CommandEcho(CommandEchoType.Error, "Can't talk in this window.");
				}

				context.Session.PrivateMessage(context.Target, text);
				return new CommandEcho(CommandEchoType.Message, text);
			}
		}

		private static CommandEcho Execute(ChatContext context, string command, string arguments)
		{
			string[] args;

			switch (command)
			{
				case "QUIT":
					args = Split(command, arguments, 0, 1);
					context.Session.Quit(args.Length == 0 ? "Leaving" : args[0]);
					break;
				case "NICK":
					args = Split(command, arguments, 1, 1);
					context.Session.Nick(args[0]);
					break;
				case "NOTICE":
					args = Split(command, arguments, 2, 2);
					context.Session.Notice(new IrcTarget(args[0]), args[1]);
					break;
				case "JOIN":
				case "J":
					args = Split(command, arguments, 1, 1);
					context.Session.Join(args[0]);
					break;
				case "PART":
				case "LEAVE":
					args = Split(command, arguments, 1, 1);
					context.Session.Part(args[0]);
					break;
				case "TOPIC":
					args = Split(command, arguments, 2, 2);
					context.Session.Topic(args[0], args[1]);
					break;
				case "INVITE":
					args = Split(command, arguments, 2, 2);
					context.Session.Invite(args[1], args[0]);
					break;
				case "KICK":
					args = Split(command, arguments, 2, 2);
					context.Session.Kick(args[0], args[1]);
					break;
				case "MOTD":
					args = Split(command, arguments, 0, 1);
					if (args.Length > 0)
					{
						context.Session.Motd(args[0]);
					}
					else
					{
						context.Session.Motd();
					}
					break;
				case "WHO":
					args = Split(command, arguments, 1, 1);
					context.Session.Who(args[0]);
					break;
				case "WHOIS":
					args = Split(command, arguments, 1, 2);
					if (args != null)
					{
						if (args.Length == 2)
						{
							context.Session.WhoIs(args[0], args[1]);
						}
						else if (args.Length == 1)
						{
							context.Session.WhoIs(args[0]);
						}
					}
					break;
				case "WHOWAS":
					args = Split(command, arguments, 1, 1);
					context.Session.WhoWas(args[0]);
					break;
				case "AWAY":
					args = Split(command, arguments, 0, 1);
					if (args.Length > 0)
					{
						context.Session.Away(args[0]);
					}
					else
					{
						context.Session.UnAway();
					}
					break;
				case "USERHOST":
					args = Split(command, arguments, 1, int.MaxValue);
					context.Session.UserHost(args);
					break;
				case "MODE":
					args = Split(command, arguments, 1, 2);
					var target = new IrcTarget(args[0]);
					if (target.Type == IrcTargetType.Nickname)
					{
						if (!context.Session.IsSelf(target))
						{
							return new CommandEcho(CommandEchoType.Error, "Can't set modes for another user.");
						}
						if (args.Length > 1)
						{
							context.Session.Mode(args[1]);
						}
						else
						{
							context.Session.Mode("");
						}
					}
					else
					{
						if (args.Length > 1)
						{
							context.Session.Mode(target.Name, args[1]);
						}
						else
						{
							context.Session.Mode(target.Name, "");
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
					if (context.IsConnected)
					{
						context.Session.Quit("Changing servers");
					}
					context.Session.Open(args[0], port,
						!string.IsNullOrEmpty(context.Session.Nickname) ? context.Session.Nickname : App.Preferences.User.Nickname);
					break;
				case "ME":
				case "ACTION":
					if (context.Target == null)
					{
						return new CommandEcho(CommandEchoType.Error, "Can't talk in this window.");
					}
					args = Split(command, arguments, 1, int.MaxValue);
					context.Session.SendCtcp(context.Target,
						new CtcpCommand("ACTION", args), false);
					return new CommandEcho(CommandEchoType.Action, string.Join(" ", args));
				default:
					return new CommandEcho(CommandEchoType.Error,
						string.Format("Unrecognized command: {0}", command));
			}
			return null;
		}

		public static string[] Split(string command, string args, int minArgs, int maxArgs)
		{
			string[] parts = StringUtility.Split(args, maxArgs);
			if (parts.Length < minArgs)
			{
				throw new Exception(string.Format("{0} requires {1} parameters.", command, minArgs));
			}
			return parts;
		}
	}
}
