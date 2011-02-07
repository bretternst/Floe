using System;

namespace Floe.Net
{
	/// <summary>
	/// Represents an abstract IRC prefix which could refer to either a user or a server.
	/// </summary>
	public abstract class IrcPrefix
	{
		/// <summary>
		/// Gets the raw prefix.
		/// </summary>
		public string Prefix { get; private set; }

		internal IrcPrefix(string prefix)
		{
			this.Prefix = prefix;
		}

		/// <summary>
		/// Gets the raw prefix.
		/// </summary>
		/// <returns>Returns the raw prefix.</returns>
		public override string ToString()
		{
			return this.Prefix;
		}

		internal static IrcPrefix Parse(string prefix)
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

	/// <summary>
	/// Represents another user on the IRC network, identified by a prefix (nick!user@host), and exposes the separated
	/// properties of the prefix.
	/// </summary>
	public sealed class IrcPeer : IrcPrefix
	{
		/// <summary>
		/// Gets the user's nickname.
		/// </summary>
		public string Nickname { get; private set; }

		/// <summary>
		/// Gets the user's username.
		/// </summary>
		public string Username { get; private set; }

		/// <summary>
		/// Gets the user's hostname.
		/// </summary>
		public string Hostname { get; private set; }

		internal IrcPeer(string nickUserHost)
			: base(nickUserHost)
		{
			string[] parts = nickUserHost.Split('@');

			if(parts.Length > 1)
			{
				this.Hostname = parts[1];
			}

			if(parts.Length > 0)
			{
				parts = parts[0].Split('!');
				if (parts.Length > 0)
				{
					this.Username = parts[1];
				}
				if (parts.Length > 0)
				{
					this.Nickname = parts[0];
				}
			}
		}
	}

	/// <summary>
	/// Represents an IRC server from which messages may be received.
	/// </summary>
	public sealed class IrcServer : IrcPrefix
	{
		/// <summary>
		/// Gets the name of the server.
		/// </summary>
		public string ServerName { get { return this.Prefix; } }

		internal IrcServer(string serverName)
			: base(serverName)
		{
		}
	}
}
