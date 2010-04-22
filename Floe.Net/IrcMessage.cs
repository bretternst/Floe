using System;
using System.Collections.Generic;
using System.Text;

namespace Floe.Net
{
	public sealed class IrcMessage
	{
		public IrcPrefix From { get; private set; }

		public string Command { get; private set; }

		public IList<string> Parameters { get; private set; }

		public IrcMessage(string command, params string[] parameters)
			: this(null, command, parameters)
		{
		}

		public IrcMessage(IrcPrefix prefix, string command, params string[] parameters)
		{
			this.From = prefix;
			this.Command = command.ToUpperInvariant();
			this.Parameters = parameters;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			if (this.From != null)
				sb.Append(':').Append(this.From.ToString()).Append(' ');
			sb.Append(this.Command);
			for (int i = 0; i < this.Parameters.Count; i++)
			{
				if (this.Parameters[i] == null)
					continue;

				sb.Append(' ');
				if (i == this.Parameters.Count - 1)
					sb.Append(':');
				sb.Append(this.Parameters[i]);
			}

			return sb.ToString();
		}

		public static IrcMessage Parse(string data)
		{
			StringBuilder sb = new StringBuilder();
			List<string> para = new List<string>();
			int size = data.Length > 512 ? 512 : data.Length;
			Char[] c = data.ToCharArray(0, size);
			int pos = 0;
			string prefix = null;
			string command = null;

			if (c[0] == ':')
			{
				for (pos = 1; pos < c.Length; pos++)
				{
					if (c[pos] == ' ')
						break;

					sb.Append(c[pos]);
				}
				prefix = sb.ToString();
				sb.Length = 0;
				pos++;
			}

			for (; pos < c.Length; pos++)
			{
				if (c[pos] == ' ')
					break;
				sb.Append(c[pos]);
			}
			command = sb.ToString();
			sb.Length = 0;
			pos++;

			bool trailing = false;
			while (pos < c.Length)
			{
				if (c[pos] == ':')
				{
					trailing = true;
					pos++;
				}

				for (; pos < c.Length; pos++)
				{
					if (c[pos] == ' ' && !trailing)
						break;
					sb.Append(c[pos]);
				}
				para.Add(sb.ToString());
				sb.Length = 0;
				pos++;
			}

			return new IrcMessage(IrcPrefix.Parse(prefix), command, para.ToArray());
		}
	}
}
