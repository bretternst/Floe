using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatControl : UserControl
	{
		public static readonly DependencyProperty NicknamesProperty = DependencyProperty.Register(
			"Nicknames", typeof(ObservableCollection<NicknameItem>), typeof(ChatControl));

		public ObservableCollection<NicknameItem> Nicknames
		{
			get { return (ObservableCollection<NicknameItem>)this.GetValue(NicknamesProperty); }
			set { this.SetValue(NicknamesProperty, value); }
		}

		private void AddNick(ChannelLevel level, string nick)
		{
			this.RemoveNick(nick);
			var cn = new NicknameItem(level, nick);
			int count = this.Nicknames.Count;
			if (count == 0 || cn.CompareTo(this.Nicknames[count-1]) >= 0)
			{
				this.Nicknames.Add(cn);
			}
			else
			{
				int i;
				for (i = 0; i < this.Nicknames.Count; i++)
				{
					if (cn.CompareTo(this.Nicknames[i]) < 0)
					{
						break;
					}
				}
				this.Nicknames.Insert(i, cn);
			}
		}

		private void AddNick(string nick)
		{
			if (string.IsNullOrEmpty(nick) || nick == "+" || nick == "@")
			{
				return;
			}

			var level = ChannelLevel.Normal;
			while (nick.StartsWith("@") || nick.StartsWith("+"))
			{
				level |= nick[0] == '@' ? ChannelLevel.Op : ChannelLevel.Voice;
				nick = nick.Substring(1);
			}
			this.AddNick(level, nick);
		}

		private void RemoveNick(string nick)
		{
			var cn = this.GetNick(nick);
			if (cn != null)
			{
				this.Nicknames.Remove(cn);
			}
		}

		private void ChangeNick(string oldNick, string newNick)
		{
			var cn = this.GetNick(oldNick);
			if (cn != null)
			{
				this.RemoveNick(cn.Nickname);
				this.AddNick(cn.Level, newNick);
			}
		}

		private string GetNickWithLevel(string nick)
		{
			if (!this.IsChannel)
			{
				return nick;
			}

			var cn = this.GetNick(nick);
			return cn != null ? cn.ToString() : nick;
		}

		private string GetNickWithoutLevel(string nick)
		{
			return (nick.Length > 1 && (nick[0] == '@' || nick[0] == '+')) ? nick.Substring(1) : nick;
		}

		private bool IsPresent(string nick)
		{
			return this.Nicknames.Any((cn) => cn.CompareTo(nick) == 0);
		}

		private NicknameItem GetNick(string nick)
		{
			return this.Nicknames.Where((n) => n.CompareTo(nick) == 0).FirstOrDefault();
		}

		private void ProcessMode(IrcChannelMode mode)
		{
			if (mode.Mode == 'o' || mode.Mode == 'v')
			{
				var cn = this.GetNick(mode.Parameter);
				if (cn != null)
				{
					var level = cn.Level;
					var mask = mode.Mode == 'o' ? ChannelLevel.Op : ChannelLevel.Voice;
					level = mode.Set ? level | mask : level & ~mask;
					this.AddNick(level, cn.Nickname);
				}
			}
		}
	}

	[Flags]
	public enum ChannelLevel
	{
		Normal = 0,
		Voice = 1,
		Op = 2
	}

	public class NicknameItem : ListBoxItem, IComparable<NicknameItem>, IComparable<string>
	{
		public ChannelLevel Level { get; private set; }
		public string Nickname { get; private set; }

		private ChannelLevel HighestLevel
		{
			get
			{
				if ((this.Level & ChannelLevel.Op) > 0)
				{
					return ChannelLevel.Op;
				}
				else if ((this.Level & ChannelLevel.Voice) > 0)
				{
					return ChannelLevel.Voice;
				}
				else
				{
					return ChannelLevel.Normal;
				}
			}
		}

		public NicknameItem(ChannelLevel level, string nickname)
		{
			this.Level = level;
			this.Nickname = nickname;
			this.Content = this.ToString();
		}

		public int CompareTo(NicknameItem other)
		{
			if (this.HighestLevel == other.HighestLevel)
			{
				return string.Compare(this.Nickname, other.Nickname, StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				return (int)other.HighestLevel - (int)this.HighestLevel;
			}
		}

		public int CompareTo(string other)
		{
			return string.Compare(this.Nickname, other, StringComparison.OrdinalIgnoreCase);
		}

		public override string ToString()
		{
			return (this.Level == ChannelLevel.Op ? "@" : (this.Level == ChannelLevel.Voice ? "+" : "")) + this.Nickname;
		}
	}
}
