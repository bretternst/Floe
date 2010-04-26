using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using Floe.Net;

namespace Floe.UI
{
	public partial class App : Application
	{
		private const char CommandChar = '/';

		public void ShowSettings()
		{
			var settings = new Settings.SettingsWindow();
			settings.ShowDialog();
		}

		private void OnInput(object sender, InputEventArgs e)
		{
			try
			{
				this.Execute(e.Context, e.Text);
			}
			catch (InputException ex)
			{
				e.Context.OnOutput(ex.Message);
			}
			catch (IrcException ex)
			{
				e.Context.OnOutput(ex.Message);
			}
		}

		private static IrcSession CreateSession()
		{
			return new IrcSession(App.Preferences.User.UserName, App.Preferences.User.HostName, App.Preferences.User.FullName);
		}

		private void OpenWindow()
		{
			var window = new ChatWindow(CreateSession());
			window.Closed += new EventHandler(mainWindow_Closed);
			window.AddHandler(ChatControl.InputReceivedEvent, new InputEventHandler(this.OnInput));
			window.Show();
			this.MainWindow = window;
		}

		private void Execute(ChatController context, string text)
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
				if (command.Length == 0)
				{
					return;
				}
				this.Execute(context, command.ToUpperInvariant(), args);
			}
			else
			{
				if (text.Trim().Length < 1)
				{
					return;
				}

				if (context.Target == null)
				{
					throw new InputException("Can't talk in this window.");
				}

				context.Session.PrivateMessage(context.Target, text);
			}
		}

		private void Execute(ChatController context, string command, string arguments)
		{
			string[] args;

			switch (command)
			{
				case "QUIT":
					args = this.Split(command, arguments, 0, 1);
					context.Session.Quit(args.Length == 0 ? "" : args[0]);
					break;
				case "NICK":
					args = this.Split(command, arguments, 1, 1);
					context.Session.Nick(args[0]);
					break;
				case "NOTICE":
					args = this.Split(command, arguments, 2, 2);
					context.Session.Notice(new IrcTarget(args[0]), args[1]);
					break;
				case "JOIN":
					args = this.Split(command, arguments, 1, 1);
					context.Session.Join(args[0]);
					break;
				case "PART":
					args = this.Split(command, arguments, 1, 1);
					context.Session.Part(args[0]);
					break;
				case "TOPIC":
					args = this.Split(command, arguments, 2, 2);
					context.Session.Topic(args[0], args[1]);
					break;
				case "INVITE":
					args = this.Split(command, arguments, 2, 2);
					context.Session.Invite(args[1], args[0]);
					break;
				case "KICK":
					args = this.Split(command, arguments, 2, 2);
					context.Session.Kick(args[0], args[1]);
					break;
				case "MOTD":
					args = this.Split(command, arguments, 0, 1);
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
					args = this.Split(command, arguments, 1, 1);
					context.Session.Who(args[0]);
					break;
				case "WHOIS":
					args = this.Split(command, arguments, 1, 2);
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
					args = this.Split(command, arguments, 1, 1);
					context.Session.WhoWas(args[0]);
					break;
				case "AWAY":
					args = this.Split(command, arguments, 0, 1);
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
					args = this.Split(command, arguments, 1, int.MaxValue);
					context.Session.UserHost(args);
					break;
				case "MODE":
					args = this.Split(command, arguments, 2, 2);
					var target = new IrcTarget(args[0]);
					if (target.Type == IrcTargetType.Nickname)
					{
						if (!context.Session.IsSelf(target))
						{
							throw new InputException("Can't set modes for another user.");
						}
						context.Session.Mode(args[1]);
					}
					else
					{
						context.Session.Mode(target.Name, args[1]);
					}
					break;
				case "SERVER":
					args = this.Split(command, arguments, 1, 2);
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
				default:
					throw new InputException("Unrecognized command.");
			}
		}

		public string[] Split(string command, string args, int minArgs, int maxArgs)
		{
			string[] parts = StringUtility.Split(args, maxArgs);
			if (parts.Length < minArgs)
			{
				throw new InputException(string.Format("{0} requires {1} parameters.", command, minArgs));
			}
			return parts;
		}
	}
}
