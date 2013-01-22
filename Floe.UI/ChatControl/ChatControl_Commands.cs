﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatControl : ChatPage
	{
		private const char CommandChar = '/';

		public readonly static RoutedUICommand WhoisCommand = new RoutedUICommand("Whois", "Whois", typeof(ChatControl));
		public readonly static RoutedUICommand OpenLinkCommand = new RoutedUICommand("Open", "OpenLink", typeof(ChatControl));
		public readonly static RoutedUICommand CopyLinkCommand = new RoutedUICommand("Copy", "CopyLink", typeof(ChatControl));
		public readonly static RoutedUICommand QuitCommand = new RoutedUICommand("Disconnect", "Quit", typeof(ChatControl));
		public readonly static RoutedUICommand ClearCommand = new RoutedUICommand("Clear", "Clear", typeof(ChatControl));
		public readonly static RoutedUICommand InsertCommand = new RoutedUICommand("Insert", "Insert", typeof(ChatControl));
		public readonly static RoutedUICommand OpCommand = new RoutedUICommand("Op", "Op", typeof(ChatControl));
		public readonly static RoutedUICommand DeopCommand = new RoutedUICommand("Deop", "Deop", typeof(ChatControl));
		public readonly static RoutedUICommand VoiceCommand = new RoutedUICommand("Voice", "Voice", typeof(ChatControl));
		public readonly static RoutedUICommand DevoiceCommand = new RoutedUICommand("Devoice", "Devoice", typeof(ChatControl));
		public readonly static RoutedUICommand KickCommand = new RoutedUICommand("Kick", "Kick", typeof(ChatControl));
		public readonly static RoutedUICommand BanCommand = new RoutedUICommand("Ban", "Ban", typeof(ChatControl));
		public readonly static RoutedUICommand UnbanCommand = new RoutedUICommand("Unban", "Unban", typeof(ChatControl));
		public readonly static RoutedUICommand SearchCommand = new RoutedUICommand("Search", "Search", typeof(ChatControl));
		public readonly static RoutedUICommand SearchPreviousCommand = new RoutedUICommand("Previous", "SearchPrevious", typeof(ChatControl));
		public readonly static RoutedUICommand SearchNextCommand = new RoutedUICommand("Next", "SearchNext", typeof(ChatControl));
		public readonly static RoutedUICommand SlapCommand = new RoutedUICommand("Slap!", "Slap", typeof(ChatControl));
		public readonly static RoutedUICommand DccChatCommand = new RoutedUICommand("Chat", "DccXmit", typeof(ChatControl));
		public readonly static RoutedUICommand DccXmitCommand = new RoutedUICommand("Xmit...", "DccXmit", typeof(ChatControl));
		public readonly static RoutedUICommand DccSendCommand = new RoutedUICommand("Send...", "DccSend", typeof(ChatControl));
		public readonly static RoutedUICommand JoinCommand = new RoutedUICommand("Join", "Join", typeof(ChatWindow));
		public readonly static RoutedUICommand ChannelPanelCommand = new RoutedUICommand("Channel Pane", "ChannelPane", typeof(ChatControl));
		public readonly static RoutedUICommand ListCommand = new RoutedUICommand("List", "List", typeof(ChatControl));

		private void CanExecuteConnectedCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.IsConnected;
		}

		private void CanExecuteChannelCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.IsConnected && this.IsChannel;
		}

		private void Insert(string s)
		{
			if (!string.IsNullOrEmpty(txtInput.SelectedText))
			{
				int pos = txtInput.CaretIndex;
				txtInput.SelectedText = s;
				txtInput.CaretIndex = pos;
			}
			else
			{
				int pos = txtInput.CaretIndex;
				txtInput.Text = txtInput.Text.Insert(txtInput.CaretIndex, s);
				txtInput.CaretIndex = pos + s.Length;
			}
		}

		private void ExecuteInsert(object sender, ExecutedRoutedEventArgs e)
		{
			var s = e.Parameter as string;
			if (!string.IsNullOrEmpty(s))
			{
				this.Insert(s);
			}
		}

		private void CanExecuteIsOp(object sender, CanExecuteRoutedEventArgs e)
		{
			if (!this.IsChannel || !_nickList.Contains(this.Session.Nickname))
			{
				e.CanExecute = false;
				return;
			}
			var nick = _nickList[this.Session.Nickname];
			e.CanExecute = nick != null && (nick.Level & ChannelLevel.Op) > 0;
		}

		private void CanExecuteIsHalfOp(object sender, CanExecuteRoutedEventArgs e)
		{
			if (!this.IsChannel || !_nickList.Contains(this.Session.Nickname))
			{
				e.CanExecute = false;
				return;
			}
			var nick = _nickList[this.Session.Nickname];
			e.CanExecute = nick != null && (nick.Level & (ChannelLevel.Op | ChannelLevel.HalfOp)) > 0;
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

		private void ExecuteClear(object sender, RoutedEventArgs e)
		{
			boxOutput.Clear();
		}

		private void ExecuteOp(object sender, ExecutedRoutedEventArgs e)
		{
			this.ExecuteOpVoice(e, 'o', true);
		}

		private void ExecuteDeop(object sender, ExecutedRoutedEventArgs e)
		{
			this.ExecuteOpVoice(e, 'o', false);
		}

		private void ExecuteVoice(object sender, ExecutedRoutedEventArgs e)
		{
			this.ExecuteOpVoice(e, 'v', true);
		}

		private void ExecuteDevoice(object sender, ExecutedRoutedEventArgs e)
		{
			this.ExecuteOpVoice(e, 'v', false);
		}

		private void ExecuteOpVoice(ExecutedRoutedEventArgs e, char mode, bool set)
		{
			IEnumerable<string> nicks;
			if (e.Parameter is System.Collections.IList)
			{
				nicks = ((System.Collections.IList)e.Parameter).OfType<NicknameItem>().Select((i) => i.Nickname);
			}
			else
			{
				nicks = new[] { e.Parameter.ToString() };
			}

			this.Session.Mode(this.Target.Name,
				from nick in nicks
				select new IrcChannelMode(set, mode, nick));
		}

		private void ExecuteKick(object sender, ExecutedRoutedEventArgs e)
		{
			IEnumerable<string> nicks;
			if (e.Parameter is System.Collections.IList)
			{
				nicks = ((System.Collections.IList)e.Parameter).OfType<NicknameItem>().Select((i) => i.Nickname);
			}
			else
			{
				nicks = new[] { e.Parameter.ToString() };
			}

			foreach (var nick in nicks)
			{
				this.Session.Kick(this.Target.Name, nick);
			}
		}

		private void ExecuteBan(object sender, ExecutedRoutedEventArgs e)
		{
			this.ExecuteBanOrUnban(e, true);
		}

		private void ExecuteUnban(object sender, ExecutedRoutedEventArgs e)
		{
			this.ExecuteBanOrUnban(e, false);
		}

		private void ExecuteBanOrUnban(ExecutedRoutedEventArgs e, bool banSet)
		{
			IEnumerable<string> nicks;
			if (e.Parameter is System.Collections.IList)
			{
				nicks = ((System.Collections.IList)e.Parameter).OfType<NicknameItem>().Select((i) => i.Nickname);
			}
			else
			{
				nicks = new[] { e.Parameter.ToString() };
			}

			for(int i = 0; i < nicks.Count(); i += 3)
			{
				this.Session.AddHandler(new IrcCodeHandler((ee) =>
					{
						if (ee.Message.Parameters.Count > 1)
						{
							var modes = from user in ee.Message.Parameters[1].Split(' ')
										let parts = user.Split('@')
										where parts.Length == 2
										select new IrcChannelMode(banSet, 'b', "*!*@" + parts[1]);
							this.Session.Mode(this.Target.Name, modes);
						}
						return true;
					}, IrcCode.RPL_USERHOST));
				var chunk = nicks.Skip(i).Take(3).ToArray();
				this.Session.UserHost(chunk);
			}
		}

		private void ExecuteDccChat(object sender, ExecutedRoutedEventArgs e)
		{
			App.ChatWindow.DccChat(this.Session, new IrcTarget((string)e.Parameter));
		}

		private void ExecuteDccXmit(object sender, ExecutedRoutedEventArgs e)
		{
			string fileName = App.OpenFileDialog(_window, App.Settings.Current.Dcc.DownloadFolder);
			if (!string.IsNullOrEmpty(fileName))
			{
				App.ChatWindow.DccXmit(this.Session, new IrcTarget((string)e.Parameter), new System.IO.FileInfo(fileName));
			}
		}

		private void ExecuteDccSend(object sender, ExecutedRoutedEventArgs e)
		{
			string fileName = App.OpenFileDialog(_window, App.Settings.Current.Dcc.DownloadFolder);
			if (!string.IsNullOrEmpty(fileName))
			{
				App.ChatWindow.DccSend(this.Session, new IrcTarget((string)e.Parameter), new System.IO.FileInfo(fileName));
			}
		}

		private void ExecuteSearch(object sender, ExecutedRoutedEventArgs e)
		{
			this.ToggleSearch();
		}

		private void ExecuteChannelPanel(object sender, ExecutedRoutedEventArgs e)
		{
			this.ToggleChannelPanel();
		}

		private void ExecuteSearchPrevious(object sender, ExecutedRoutedEventArgs e)
		{
			this.DoSearch(SearchDirection.Previous);
		}

		private void ExecuteSearchNext(object sender, ExecutedRoutedEventArgs e)
		{
			this.DoSearch(SearchDirection.Next);
		}

		private void ExecuteList(object sender, ExecutedRoutedEventArgs e)
		{
			this.Session.List();
		}

		private void ExecuteJoin(object sender, ExecutedRoutedEventArgs e)
		{
			string channel = e.Parameter as string;
			if (!string.IsNullOrEmpty(channel))
			{
				this.Session.Join(channel);
			}
		}

		private void Execute(string text, bool literal)
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

			if (command.Length > 0 && command[0] == CommandChar && !literal)
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
					try
					{
						this.Execute(command.ToUpperInvariant(), args);
					}
					catch (CommandException ex)
					{
						this.Write("Error", ex.Message);
					}
				}
			}
			else
			{
				if (text.Trim().Length > 0 && this.IsConnected)
				{
					if (this.Type == ChatPageType.Chat)
					{
						this.Session.PrivateMessage(this.Target, text);
						this.Write("Own", 0, this.GetNickWithLevel(this.Session.Nickname), text, false);
					}
					else if (this.Type == ChatPageType.DccChat)
					{
						_dcc.QueueMessage(text);
						this.Write("Own", 0, this.Session.Nickname, text, false);
					}
					else
					{
						this.Write("Error", "Can't talk in this window.");
					}
				}
				else
				{
					App.DoEvent("beep");
				}
			}
		}

		private void Execute(string command, string arguments)
		{
			string[] args;

			switch (command)
			{
                case "QUOTE":
                    args = Split(command, arguments, 1, 1);
                    this.Session.Quote(args[0]);
                    break;
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
					args = Split(command, arguments, 1, 2);
					if (args.Length == 2)
					{
						this.Session.Join(args[0], args[1]);
					}
					else
					{
						this.Session.Join(args[0]);
					}
					break;
				case "PART":
				case "LEAVE":
					args = Split(command, arguments, 1, 1, true);
					this.Session.Part(args[0]);
					break;
				case "TOPIC":
					args = Split(command, arguments, 1, 2, true);
					if (args.Length > 1)
					{
						this.Session.Topic(args[0], args[1]);
					}
					else
					{
						this.Session.Topic(args[0]);
					}
					break;
				case "INVITE":
					args = Split(command, arguments, 2, 2);
					this.Session.Invite(args[1], args[0]);
					break;
				case "KICK":
					args = Split(command, arguments, 2, 3, true);
					if (args.Length > 2)
					{
						this.Session.Kick(args[0], args[1], args[2]);
					}
					else
					{
						this.Session.Kick(args[0], args[1]);
					}
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
					if (!target.IsChannel)
					{
						if (!this.Session.IsSelf(target))
						{
							throw new CommandException("Can't change modes for another user.");
						}
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
					{
						args = Split(command, arguments, 1, 3);
						int port = 0;
						bool useSsl = false;
						if (args.Length > 1 && (args[1] = args[1].Trim()).Length > 0)
						{
							if (args[1][0] == '+')
							{
								useSsl = true;
							}
							int.TryParse(args[1], out port);
						}
						string password = null;
						if (args.Length > 2)
						{
							password = args[2];
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
						this.Connect(args[0], port, useSsl, false, password);
					}
					break;
				case "ME":
				case "ACTION":
					if (this.IsServer)
					{
						this.Write("Error", "Can't talk in this window.");
					}
					if (this.IsConnected)
					{
						args = Split(command, arguments, 1, int.MaxValue);
						this.Write("Own", string.Format("{0} {1}", this.Session.Nickname, string.Join(" ", args)));
						if (this.Type == ChatPageType.Chat)
						{
							this.Session.SendCtcp(this.Target, new CtcpCommand("ACTION", args), false);
						}
						else if (this.Type == ChatPageType.DccChat)
						{
							_dcc.QueueMessage(string.Format("\u0001ACTION {0}\u0001", string.Join(" ", args)));
						}
					}
					break;
				case "SETUP":
					App.ShowSettings();
					break;
				case "CLEAR":
					boxOutput.Clear();
					break;
				case "MSG":
					if (this.IsConnected)
					{
						args = Split(command, arguments, 2, 2);
						this.Session.PrivateMessage(new IrcTarget(args[0]), args[1]);
						this.Write("Own", string.Format("-> [{0}] {1}", args[0], args[1]));
					}
					break;
				case "LIST":
					args = Split(command, arguments, 0, 2);
					if (args.Length > 1)
					{
						this.Session.List(args[0], args[1]);
					}
					else if (args.Length > 0)
					{
						this.Session.List(args[0]);
					}
					else
					{
						this.Session.List();
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
						char mode = (command == "OP" || command == "DEOP") ? 'o' : 'v';
						args = Split(command, arguments, 1, int.MaxValue);
						var modes = from s in args
									select new IrcChannelMode(command == "OP" || command == "VOICE", mode, s);
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
				case "CTCP":
					args = Split(command, arguments, 2, int.MaxValue);
					this.Session.SendCtcp(new IrcTarget(args[0]),
						new CtcpCommand(args[1], args.Skip(2).ToArray()), false);
					break;
				case "QUERY":
					args = Split(command, arguments, 1, 1);
					ChatWindow.ChatCommand.Execute(args[0], this);
					break;
				case "BAN":
					args = Split(command, arguments, 1, 1);
					ChatControl.BanCommand.Execute(args[0], this);
					break;
				case "UNBAN":
					args = Split(command, arguments, 1, 1);
					ChatControl.UnbanCommand.Execute(args[0], this);
					break;
				case "IGNORE":
					{
						args = Split(command, arguments, 0, 2);
						if (args.Length == 0)
						{
							var ignores = App.GetIgnoreInfo();
							if (ignores.Any())
							{
								this.Write("Own", "Ignore list:");
								foreach (string i in ignores)
								{
									this.Write("Own", "  " + i);
								}
							}
							else
							{
								this.Write("Own", "Ignore list is empty.");
							}
							break;
						}

						string mask = args[0];
						string sactions = args.Length > 1 ? args[1] : "All";
						IgnoreActions actions;
						if (!Enum.TryParse(sactions, true, out actions))
						{
							this.Write("Error", "Invalid ignore action(s).");
							break;
						}

						if (!mask.Contains('!') && !mask.Contains('@'))
						{
							mask = mask + "!*@*";
						}
						App.AddIgnore(mask, actions);
						this.Write("Own", "Added to ignore list: " + mask);
					}
					break;
				case "UNIGNORE":
					{
						args = Split(command, arguments, 1, 2);
						string mask = args[0];

						string sactions = args.Length > 1 ? args[1] : "All";
						IgnoreActions actions;
						if (!Enum.TryParse(sactions, true, out actions))
						{
							this.Write("Error", "Invalid ignore action(s).");
							break;
						}
						if (!mask.Contains('!') && !mask.Contains('@'))
						{
							mask = mask + "!*@*";
						}
						if (App.RemoveIgnore(mask, actions))
						{
							this.Write("Own", "Removed from ignore list: " + mask);
						}
						else
						{
							this.Write("Error", "Specified pattern was not on ignore list.");
						}
					}
					break;
				case "DCC":
					{
						if (!this.IsConnected)
						{
							return;
						}
						args = Split(command, arguments, 2, 3);
						string dccCmd = args[0].ToUpperInvariant();

						switch (dccCmd)
						{
							case "CHAT":
								App.ChatWindow.DccChat(this.Session, new IrcTarget(args[1]));
								break;
							case "SEND":
							case "XMIT":
								string path = null;
								if (args.Length < 3)
								{
									this.Write("Error", "File name is required.");
									break;
								}
								try
								{
									if (System.IO.Path.IsPathRooted(args[2]) && System.IO.File.Exists(args[2]))
									{
										path = args[2];
									}
									else if (!System.IO.File.Exists(path = System.IO.Path.Combine(App.Settings.Current.Dcc.DownloadFolder, args[2])))
									{
										this.Write("Error", "Could not find file " + args[2]);
										break;
									}
								}
								catch (ArgumentException)
								{
									this.Write("Error", string.Format("Invalid pathname: {0}", args[2]));
									break;
								}
								if (dccCmd == "XMIT")
								{
									App.ChatWindow.DccXmit(this.Session, new IrcTarget(args[1]), new System.IO.FileInfo(path));
								}
								else
								{
									App.ChatWindow.DccSend(this.Session, new IrcTarget(args[1]), new System.IO.FileInfo(path));
								}
								break;
							default:
								this.Write("Error", "Unsupported DCC mode " + args[0]);
								break;
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
			return this.Split(command, args, minArgs, maxArgs, false);
		}

		private string[] Split(string command, string args, int minArgs, int maxArgs, bool isChannelRequired)
		{
			string[] parts = ChatControl.Split(args, maxArgs);
			if (isChannelRequired && (parts.Length < 1 || !IrcTarget.IsChannelName(parts[0])))
			{
				if (!this.IsChannel)
				{
					throw new CommandException("Not on a channel.");
				}
				parts = new[] { this.Target.Name }.Union(ChatControl.Split(args, maxArgs - 1)).ToArray();
			}
			if (parts.Length < minArgs)
			{
				throw new CommandException(string.Format("{0} requires {1} parameters.", command, minArgs));
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

		private void ToggleSearch()
		{
			if (pnlSearch.Visibility == Visibility.Visible)
			{
				pnlSearch.Visibility = Visibility.Collapsed;
				boxOutput.ClearSearch();
			}
			else
			{
				pnlSearch.Visibility = Visibility.Visible;
				txtSearchTerm.Focus();
				txtSearchTerm.SelectAll();
			}
		}
		
		private void ToggleChannelPanel()
		{
			if (pnlChannel.Visibility == Visibility.Visible)
			{
				pnlChannel.Visibility = Visibility.Collapsed;
			}
			else
			{
				pnlChannel.Visibility = Visibility.Visible;
				txtChannel.Focus();
				if (txtChannel.Text.Length < 1 || txtChannel.Text == "#")
				{
					txtChannel.Text = "#";
					txtChannel.CaretIndex = 1;
				}
				else
				{
					txtChannel.SelectAll();
				}
			}
		}

		private void DoSearch(SearchDirection dir)
		{
			Regex pattern = null;

			try
			{
				pattern = new Regex(
						chkUseRegEx.IsChecked.Value ? txtSearchTerm.Text : Regex.Escape(txtSearchTerm.Text),
						chkMatchCase.IsChecked.Value ? RegexOptions.None : RegexOptions.IgnoreCase);
			}
			catch (ArgumentException ex)
			{
				MessageBox.Show("The regular expression was not valid: " + Environment.NewLine + ex.Message,
					"Invalid pattern", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			boxOutput.Search(pattern, dir);
		}
	}
}
