using System;
using System.Configuration;

namespace Floe.Configuration
{
	public class NetworkElement : ConfigurationElement
	{
		[ConfigurationProperty("useSocks5Proxy", DefaultValue = false)]
		public bool UseSocks5Proxy
		{
			get { return (bool)this["useSocks5Proxy"]; }
			set { this["useSocks5Proxy"] = value; }
		}

		[ConfigurationProperty("proxyHostname", DefaultValue = "127.0.0.1")]
		public string ProxyHostname
		{
			get
			{
				if (((string)this["proxyHostname"]).Trim().Length < 1)
				{
					this["proxyHostname"] = "127.0.0.1";
				}
				return (string)this["proxyHostname"];
			}
			set { this["proxyHostname"] = value; }
		}

		[ConfigurationProperty("proxyPort", DefaultValue = 1080)]
		public int ProxyPort
		{
			get { return (int)this["proxyPort"]; }
			set
			{
				if (value <= 0 || value > ushort.MaxValue)
				{
					throw new ArgumentException("The port must be within the range of 1 - 65535.");
				}
				this["proxyPort"] = value;
			}
		}

		[ConfigurationProperty("proxyUsername", DefaultValue = "")]
		public string ProxyUsername
		{
			get { return (string)this["proxyUsername"]; }
			set { this["proxyUsername"] = value; }
		}

		[ConfigurationProperty("proxyPassword", DefaultValue = "")]
		public string ProxyPassword
		{
			get { return (string)this["proxyPassword"]; }
			set { this["proxyPassword"] = value; }
		}
	}
}
