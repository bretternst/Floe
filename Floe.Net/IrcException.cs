using System;

namespace Floe.Net
{
	public class IrcException : Exception
	{
		public IrcException(string message)
			: base(message)
		{
		}

		public IrcException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
