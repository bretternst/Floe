using System;
using System.Linq;
using System.Text;

namespace Floe.Net
{
	/// <summary>
	/// Represents a CTCP command sent to, or received from, another client.
	/// </summary>
	public sealed class CtcpCommand
	{
		/// <summary>
		/// Gets the CTCP command name.
		/// </summary>
		public string Command { get; private set; }

		/// <summary>
		/// Gets the arguments used with the CTCP command.
		/// </summary>
		public string[] Arguments { get; private set; }

		/// <summary>
		/// Construct a new CTCP command.
		/// </summary>
		/// <param name="command">The command name.</param>
		/// <param name="arguments">The command arguments.</param>
		public CtcpCommand(string command, params string[] arguments)
		{
			this.Command = command.ToUpperInvariant();
			this.Arguments = arguments.ToArray();
		}

		/// <summary>
		/// Convert the command to a string that another IRC client will understand.
		/// </summary>
		/// <returns>Returns the raw string to be sent to another client.</returns>
		public override string ToString()
		{
			var output = new StringBuilder();
			output.Append('\u0001');
			output.Append(this.Command);
			foreach (string arg in this.Arguments)
			{
				output.Append(' ').Append(Quote(arg));
			}
			output.Append('\u0001');
			return output.ToString();
		}

		/// <summary>
		/// Create a new CtcpCommand object from raw text received from another client.
		/// </summary>
		/// <param name="text">The raw text that was received.</param>
		/// <returns>Returns the new CtcpCommand constructed from the specified text.</returns>
		public static CtcpCommand Parse(string text)
		{
			text = text.Replace("\u0001", "");
			string[] parts = text.Split(' ');
			if (parts.Length < 1)
			{
				return null;
			}

			string command = parts[0].ToUpperInvariant();
			string[] args = new string[parts.Length - 1];
			for (int i = 1; i < parts.Length; i++)
			{
				args[i - 1] = Unquote(parts[i]);
			}

			return new CtcpCommand(command, args);
		}

		/// <summary>
		/// Determine whether the specified raw text is a CTCP command.
		/// </summary>
		/// <param name="text">The text to test.</param>
		/// <returns>Returns true if the text contains a CTCP command, otherwise false.</returns>
		public static bool IsCtcpCommand(string text)
		{
			return text.Length > 0 && text[0] == '\u0001' && text[text.Length - 1] == '\u0001';
		}

		private static string Quote(string text)
		{
			var output = new StringBuilder();
			foreach (char c in text)
			{
				switch (c)
				{
					case '\u0000':
						output.Append("\\0");
						break;
					case '\u0001':
						output.Append("\\1");
						break;
					case '\u000a':
						output.Append("\\n");
						break;
					case '\u000d':
						output.Append("\\r");
						break;
					case ' ':
						output.Append("\\@");
						break;
					case '\\':
						output.Append("\\\\");
						break;
					default:
						output.Append(c);
						break;
				}
			}
			return output.ToString();
		}

		private static string Unquote(string text)
		{
			if (text.Length < 1)
			{
				return text;
			}

			var output = new StringBuilder();
			char last = (char)0;
			foreach (char c in text)
			{
				if (last == '\\')
				{
					switch (c)
					{
						case '0':
							output.Append('\u0000');
							break;
						case '1':
							output.Append('\u0001');
							break;
						case 'n':
							output.Append('\u000a');
							break;
						case 'r':
							output.Append('\u000d');
							break;
						case '@':
							output.Append(' ');
							break;
						case '\\':
							output.Append('\\');
							last = '\u0000';
							break;
					}
				}
				else if (c != '\\')
				{
					output.Append(c);
				}
				last = c;
			}
			return output.ToString();
		}
	}
}
