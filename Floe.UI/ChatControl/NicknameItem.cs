using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Floe.UI
{
	[Flags]
	public enum ChannelLevel
	{
		Normal = 0,
		Voice = 1,
		HalfOp = 2,
		Op = 4
	}

	public class NicknameItem : DependencyObject, IComparable<NicknameItem>, IComparable
	{
		private static char[] NickSpecialChars = new[] { '[', ']', '\\', '`', '_', '^', '{', '|', '}' };
		public static bool IsNickChar(char c)
		{
			return char.IsLetterOrDigit(c) || NickSpecialChars.Contains(c);
		}

		public NicknameItem(ChannelLevel level, string nick)
		{
			this.Nickname = nick;
			this.Level = level;
		}

		public NicknameItem(string nick)
		{
			var level = ChannelLevel.Normal;
			int i = 0;
			for (i = 0; i < nick.Length && !IsNickChar(nick[i]); i++)
			{
				switch (nick[0])
				{
					case '@':
						level |= ChannelLevel.Op;
						break;
					case '%':
						level |= ChannelLevel.HalfOp;
						break;
					case '+':
						level |= ChannelLevel.Voice;
						break;
				}
			}
			if (i < nick.Length)
			{
				nick = nick.Substring(i);
			}
			else
			{
				nick = "";
			}
			this.Nickname = nick;
			this.Level = level;
		}

		public string Nickname { get; set; }
		public ChannelLevel Level { get; set; }
		public string NickWithLevel { get { return this.ToString(); } }

		private ChannelLevel HighestLevel
		{
			get
			{
				if ((this.Level & ChannelLevel.Op) > 0)
				{
					return ChannelLevel.Op;
				}
				else if ((this.Level & ChannelLevel.HalfOp) > 0)
				{
					return ChannelLevel.HalfOp;
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

		public override string ToString()
		{
			string prefix;
			switch (this.HighestLevel)
			{
				case ChannelLevel.Op:
					prefix = "@";
					break;
				case ChannelLevel.HalfOp:
					prefix = "%";
					break;
				case ChannelLevel.Voice:
					prefix = "+";
					break;
				default:
					prefix = "";
					break;
			}

			return prefix + this.Nickname;
		}

		public int CompareTo(NicknameItem other)
		{
			if (other == null)
			{
				return 1;
			}
			if (this.HighestLevel == other.HighestLevel)
			{
				return string.Compare(this.Nickname, other.Nickname, StringComparison.InvariantCultureIgnoreCase);
			}
			else
			{
				return (int)other.HighestLevel - (int)this.HighestLevel;
			}
		}

		public int CompareTo(object obj)
		{
			return this.CompareTo(obj as NicknameItem);
		}
	}
}
