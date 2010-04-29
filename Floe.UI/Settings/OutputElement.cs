using System;
using System.Configuration;

namespace Floe.Configuration
{
	public class OutputElement : ConfigurationElement
	{
		[ConfigurationProperty("bufferLines", DefaultValue=500, IsRequired = true)]
		public int BufferLines
		{
			get { return (int)this["bufferLines"]; }
			set { this["bufferLines"] = value; }
		}
	}
}
