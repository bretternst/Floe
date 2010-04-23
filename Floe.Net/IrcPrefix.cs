using System;

namespace Floe.Net
{
	public abstract class IrcPrefix
	{
		public string Prefix { get; private set; }

		public IrcPrefix(string prefix)
		{
			this.Prefix = prefix;
		}

		public override string ToString()
		{
			return this.Prefix;
		}

		public static IrcPrefix Parse(string prefix)
		{
			if (string.IsNullOrEmpty(prefix))
			{
				return null;
			}

			int idx1 = prefix.IndexOf('!');
			int idx2 = prefix.IndexOf('@');
			if (idx1 > 0 && idx2 > 0 && idx2 > idx1 + 1)
			{
				return new IrcPeer(prefix);
			}
			else
			{
				return new IrcServer(prefix);
			}
		}
	}

	public sealed class IrcPeer : IrcPrefix
	{
		public string Nickname { get; private set; }
		public string UserName { get; private set; }
		public string HostName { get; private set; }

		public IrcPeer(string nickUserHost)
			: base(nickUserHost)
		{
			string[] parts = nickUserHost.Split('@');

			if(parts.Length > 1)
			{
				this.HostName = parts[1];
			}

			if(parts.Length > 0)
			{
				parts = parts[0].Split('!');
				if (parts.Length > 0)
				{
					this.UserName = parts[1];
				}
				if (parts.Length > 0)
				{
					this.Nickname = parts[0];
				}
			}
		}
	}

	public sealed class IrcServer : IrcPrefix
	{
		public string ServerName { get { return this.Prefix; } }

		public IrcServer(string serverName)
			: base(serverName)
		{
		}
	}
}
