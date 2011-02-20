using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.Audio
{
	[Serializable]
	public class WaveFormatException : Exception
	{
		public WaveFormatException()
			: base("The wave file is not in the expected format.")
		{
		}

		public WaveFormatException(string message)
			: base(message)
		{
		}
	}
}
