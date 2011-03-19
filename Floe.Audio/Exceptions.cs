using System;

namespace Floe.Audio
{
	[Serializable]
	public class FileFormatException : Exception
	{
		public FileFormatException()
		{
		}

		public FileFormatException(string message)
			: base(message)
		{
		}
	}
}
