using System;
using System.Configuration;

using Floe.UI.Interop;

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

		[ConfigurationProperty("text")]
		public OutputElement Text
		{
			get { return (OutputElement)this["text"]; }
			set { this["text"] = value; }
		}

		[ConfigurationProperty("colors")]
		public ColorsElement Colors
		{
			get { return (ColorsElement)this["colors"]; }
			set { this["colors"] = value; }
		}

		[ConfigurationProperty("windowPlacement")]
		public string WindowPlacement
		{
			get { return (string)this["windowPlacement"]; }
			set { this["windowPlacement"] = value; }
		}
	}
}
