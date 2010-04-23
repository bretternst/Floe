using System;
using System.Collections.Generic;

namespace Floe.Net
{
	public struct IrcUserMode
	{
		public readonly bool Set;
		public readonly char Mode;

		public IrcUserMode(bool set, char mode)
		{
			this.Set = set;
			this.Mode = mode;
		}

		public static ICollection<IrcUserMode> ParseModes(IEnumerable<string> parts)
		{
			var modeList = new List<IrcUserMode>();

			bool set = false;
			foreach (var str in parts)
			{
				var s = str.Trim();
				if (!s.StartsWith("+") && !s.StartsWith("-"))
				{
					continue;
				}
				foreach (var c in s)
				{
					if (c == '+')
					{
						set = true;
					}
					else if (c == '-')
					{
						set = false;
					}
					else
					{
						modeList.Add(new IrcUserMode(set, c));
					}
				}
			}
			return modeList;
		}

		public static ICollection<IrcUserMode> ParseModes(string modes)
		{
			return ParseModes(modes.Split(' '));
		}
	}

	public struct IrcChannelMode
	{
		public readonly bool Set;
		public readonly char Mode;
		public readonly string Parameter;

		public IrcChannelMode(bool set, char mode, string parameter)
		{
			this.Set = set;
			this.Mode = mode;
			this.Parameter = parameter;
		}

		public static ICollection<IrcChannelMode> ParseModes(IEnumerable<string> parts)
		{
			var modeList = new List<IrcChannelMode>();
			var paramSetList = new List<bool>();
			var paramModeList = new List<char>();
			var paramList = new List<string>();

			bool set = false;
			foreach (var str in parts)
			{
				var s = str.Trim();

				if (!s.StartsWith("+") && !s.StartsWith("-"))
				{
					paramList.Add(s);
				}
				else
				{
					foreach (var c in s)
					{
						if (c == '-')
						{
							set = true;
						}
						else if (c == '+')
						{
							set = false;
						}
						else
						{
							switch (c)
							{
								case 'O':
								case 'o':
								case 'v':
								case 'k':
								case 'l':
								case 'b':
								case 'e':
								case 'I':
									paramSetList.Add(set);
									paramModeList.Add(c);
									break;
								default:
									modeList.Add(new IrcChannelMode(set, c, null));
									break;
							}
						}
					}
				}
			}

			for (int i = 0; i < paramModeList.Count; i++)
			{
				if (i >= paramList.Count)
				{
					break;
				}

				modeList.Add(new IrcChannelMode(paramSetList[i], paramModeList[i], paramList[i]));
			}

			return modeList;
		}

		public static ICollection<IrcChannelMode> ParseModes(string modes)
		{
			return ParseModes(modes.Split(' '));
		}
	}
}
