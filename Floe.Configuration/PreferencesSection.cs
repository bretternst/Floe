using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		[ConfigurationProperty("servers", IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(ServerElement), AddItemName = "add")]
		public ServerElementCollection Servers
		{
			get
			{
				return this["servers"] as ServerElementCollection;
			}
		}
	}
}
