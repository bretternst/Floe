using System;
using System.Configuration;

namespace Floe.Configuration
{
	public class WindowsElement : ConfigurationElement
	{
		[ConfigurationProperty("placement")]
		public string Placement
		{
			get { return (string)this["placement"]; }
			set { this["placement"] = value; }
		}

		[ConfigurationProperty("customColors", DefaultValue = "")]
		public string CustomColors
		{
			get { return (string)this["customColors"]; }
			set { this["customColors"] = value; }
		}

		[ConfigurationProperty("nickListWidth", DefaultValue=115.0)]
		public double NickListWidth
		{
			get { return (double)this["nickListWidth"]; }
			set { this["nickListWidth"] = value; }
		}
	}
}
