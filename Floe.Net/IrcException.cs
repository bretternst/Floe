using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
