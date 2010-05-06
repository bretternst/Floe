using System;
using System.Configuration;

using Floe.UI.Interop;

namespace Floe.Configuration
{
	public sealed class SettingsSection : ConfigurationSection
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

		[ConfigurationProperty("formatting")]
		public FormattingElement Formatting
		{
			get { return (FormattingElement)this["formatting"]; }
			set { this["formatting"] = value; }
		}

		[ConfigurationProperty("colors")]
		public ColorsElement Colors
		{
			get { return (ColorsElement)this["colors"]; }
			set { this["colors"] = value; }
		}

		[ConfigurationProperty("buffer")]
		public BufferElement Buffer
		{
			get { return (BufferElement)this["buffer"]; }
			set { this["buffer"] = value; }
		}

		[ConfigurationProperty("windows")]
		public WindowsElement Windows
		{
			get { return (WindowsElement)this["windows"]; }
			set { this["windows"] = value; }
		}
	}
}
