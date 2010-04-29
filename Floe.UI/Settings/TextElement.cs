using System;
using System.Configuration;

namespace Floe.Configuration
{
	public class OutputElement : ConfigurationElement
	{
		[ConfigurationProperty("bufferLines", DefaultValue=500)]
		public int BufferLines
		{
			get { return (int)this["bufferLines"]; }
			set { this["bufferLines"] = value; }
		}

		[ConfigurationProperty("minimumCopyLength", DefaultValue=3)]
		public int MinimumCopyLength
		{
			get { return (int)this["minimumCopyLength"]; }
			set { this["minimumCopyLength"] = value; }
		}
	}
}
