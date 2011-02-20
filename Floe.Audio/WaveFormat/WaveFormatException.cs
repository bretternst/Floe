using System;

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
