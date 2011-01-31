using System;
using System.Configuration;

namespace Floe.Configuration
{
	public class DccElement : ConfigurationElement
	{
		private const int MinPort = 1024;

		[ConfigurationProperty("lowPort", DefaultValue = 57000)]
		public int LowPort
		{
			get { return (int)this["lowPort"]; }
			set
			{
				if (value < MinPort || value > this.HighPort)
				{
					throw new ArgumentException(string.Format(
						"Low port must be greater than {0} and less than the high port value.", MinPort));
				}
				this["lowPort"] = value;
			}
		}

		[ConfigurationProperty("highPort", DefaultValue = 58000)]
		public int HighPort
		{
			get { return (int)this["highPort"]; }
			set
			{
				if (value < MinPort || value < this.LowPort)
				{
					throw new ArgumentException(string.Format(
						"High port must be greater than the low port value.", MinPort));
				}
				this["highPort"] = value;
			}
		}

		[ConfigurationProperty("downloadFolder", DefaultValue = "")]
		public string DownloadFolder
		{
			get
			{
				if (((string)this["downloadFolder"]).Trim().Length < 1)
				{
					this["downloadFolder"] = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
				}
				return (string)this["downloadFolder"];
			}
			set
			{
				// Just throw an exception if the path is in an invalid format.
				if (value.Length > 0)
				{
					System.IO.Path.GetDirectoryName(value);
				}
				this["downloadFolder"] = value;
			}
		}

		[ConfigurationProperty("autoAccept", DefaultValue = false)]
		public bool AutoAccept
		{
			get { return (bool)this["autoAccept"]; }
			set { this["autoAccept"] = value; }
		}
	}
}
