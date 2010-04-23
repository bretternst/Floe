using System;
using System.Configuration;

namespace Floe.Configuration
{
	public sealed class PreferencesSection : ConfigurationSection
	{
		[ConfigurationProperty("user")]
		public UserElement User
		{
			get { return (UserElement)this["user"]; }
			set { this["user"] = value; }
		}

		[ConfigurationProperty("servers")]
		[ConfigurationCollection(typeof(ServerElement))]
		public ServerElementCollection Servers
		{
			get
			{
				return this["servers"] as ServerElementCollection;
			}
		}
	}
}
