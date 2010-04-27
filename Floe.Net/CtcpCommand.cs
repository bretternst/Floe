using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.Net
{
	public sealed class CtcpCommand
	{
		public string Command { get; private set; }

		public string[] Arguments { get; private set; }

		public CtcpCommand(string command, params string[] arguments)
		{
			this.Command = command.ToUpperInvariant();
			this.Arguments = arguments.ToArray();
		}

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
