using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.Net
{
	/// <summary>
	/// Represents a single IRC user mode change and provides utility functions for converting collections of modes to a textual
	/// mode specification and vice-versa.
	/// </summary>
	public struct IrcUserMode
	{
		/// <summary>
		/// Gets a value indicating whether the mode was set (true) or unset (false).
		/// </summary>
		public readonly bool Set;

		/// <summary>
		/// Gets the character that specifies the mode.
		/// </summary>
		public readonly char Mode;

		/// <summary>
		/// Initialize a user mode.
		/// </summary>
		/// <param name="set">Indicates whether the mode is set (true) or unset (false).</param>
		/// <param name="mode">The character that specifies the mode.</param>
		public IrcUserMode(bool set, char mode)
		{
			this.Set = set;
			this.Mode = mode;
		}

		/// <summary>
		/// Parse a given mode spec and returns a collection describing the change.
		/// </summary>
		/// <param name="modeSpec">A user mode specification (for example: "+im" or "+i-m").</param>
		/// <returns>Returns the collection of modes describing the mode change.</returns>
		public static ICollection<IrcUserMode> ParseModes(string modeSpec)
		{
			return ParseModes(modeSpec.Split(' '));
		}

		/// <summary>
		/// Converts a collection of modes into a mode specification string.
		/// </summary>
		/// <param name="modes">The collection of modes to convert.</param>
		/// <returns>Returns a mode specification string describing the modes.</returns>
		public static string RenderModes(IEnumerable<IrcUserMode> modes)
		{
			var output = new StringBuilder();
			foreach (var mode in modes.Where((m) => m.Set))
			{
				if (output.Length == 0)
				{
					output.Append('+');
				}
				output.Append(mode.Mode);
			}
			bool hasMinus = false;
			foreach (var mode in modes.Where((m) => !m.Set))
			{
				if (!hasMinus)
				{
					output.Append('-');
					hasMinus = true;
				}
				output.Append(mode.Mode);
			}
			return output.ToString();
		}

		internal static ICollection<IrcUserMode> ParseModes(IEnumerable<string> parts)
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
	}

	/// <summary>
	/// Represents a single IRC channel mode change and provides utility functions for converting collections of modes to a textual
	/// mode specification and vice-versa.
	/// </summary>
	public struct IrcChannelMode
	{
		/// <summary>
		/// Gets a value indicating whether the mode was set (true) or unset (false).
		/// </summary>
		public readonly bool Set;

		/// <summary>
		/// Gets the character that specifies the mode.
		/// </summary>
		public readonly char Mode;

		/// <summary>
		/// Gets the optional parameter attached to the mode.
		/// </summary>
		public readonly string Parameter;

		/// <summary>
		/// Initialize a channel mode.
		/// </summary>
		/// <param name="set">Indicates whether the mode is set (true) or unset (false).</param>
		/// <param name="mode">The character that specifies the mode.</param>
		/// <param name="parameter">The optional mode parameter.</param>
		public IrcChannelMode(bool set, char mode, string parameter = null)
		{
			this.Set = set;
			this.Mode = mode;
			this.Parameter = parameter;
		}

		/// <summary>
		/// Parse a mode spec into a collection of channel modes.
		/// </summary>
		/// <param name="modes">The mode spec to parse (for example, "-i+l 50" or "+snt").</param>
		/// <returns>Returns the collection of modes representing the change.</returns>
		public static ICollection<IrcChannelMode> ParseModes(string modeSpec)
		{
			return ParseModes(modeSpec.Split(' '));
		}

		/// <summary>
		/// Convert a collection of modes into a set of strings that can be sent to a server.
		/// </summary>
		/// <param name="modes">The collection of modes to convert.</param>
		/// <returns>Returns an array of strings that make up a mode change. The first string is the mode spec with mode 
		/// characters only, and all subsequent strings are mode arguments. For example, the first string may contain "-i+l"
		/// and the second string may contain "100".</returns>
		public static string[] RenderModes(IEnumerable<IrcChannelMode> modes)
		{
			var output = new StringBuilder();
			var paramsOutput = new StringBuilder();
			var args = new List<string>();
			foreach (var mode in modes.Where((m) => m.Set))
			{
				if (output.Length == 0)
				{
					output.Append('+');
				}
				output.Append(mode.Mode);
				if (!string.IsNullOrEmpty(mode.Parameter))
				{
					args.Add(mode.Parameter);
				}
			}
			bool hasMinus = false;
			foreach (var mode in modes.Where((m) => !m.Set))
			{
				if (!hasMinus)
				{
					output.Append('-');
					hasMinus = true;
				}
				output.Append(mode.Mode);
				if (!string.IsNullOrEmpty(mode.Parameter))
				{
					args.Add(mode.Parameter);
				}
			}
			var result = new string[args.Count + 1];
			result[0] = output.ToString();
			args.CopyTo(result, 1);
			return result;
		}

		internal static ICollection<IrcChannelMode> ParseModes(IEnumerable<string> parts)
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
							set = false;
						}
						else if (c == '+')
						{
							set = true;
						}
						else
						{
							switch (c)
							{
								case 'O':
								case 'o':
								case 'v':
								case 'h':
								case 'k':
								case 'l':
								case 'b':
								case 'e':
								case 'I':
								case 'f':
								case 'j':
								case 'q':
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
				modeList.Add(new IrcChannelMode(paramSetList[i], paramModeList[i], i < paramList.Count ? paramList[i] : null));
			}

			return modeList;
		}
	}
}
