using System;
using System.Configuration;

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

		[ConfigurationProperty("ignore", DefaultValue="")]
		public string Ignore
		{
			get { return (string)this["ignore"]; }
			set { this["ignore"] = value; }
		}

		[ConfigurationProperty("dcc")]
		public DccElement Dcc
		{
			get { return (DccElement)this["dcc"]; }
			set { this["dcc"] = value; }
		}

		[ConfigurationProperty("sounds")]
		public SoundsElement Sounds
		{
			get { return (SoundsElement)this["sounds"]; }
			set { this["sounds"] = value; }
		}

		[ConfigurationProperty("network")]
		public NetworkElement Network
		{
			get { return (NetworkElement)this["network"]; }
			set { this["network"] = value; }
		}
	}
}
