using System;
using System.Collections.Generic;
using System.Text;

namespace Floe.Net
{
	public class IrcEventArgs : EventArgs
	{
		public IrcMessage Message { get; private set; }

		internal IrcEventArgs(IrcMessage message)
		{
			this.Message = message;
		}
	}

	public class ErrorEventArgs : EventArgs
	{
		public Exception Exception { get; private set; }

		internal ErrorEventArgs(Exception ex)
		{
			this.Exception = ex;
		}
	}
}
