using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Floe.Net;

namespace Floe.UI
{
	public sealed class OutputEventArgs : EventArgs
	{
		public IrcMessage Message { get; private set; }

		public string Text { get; private set; }

		public OutputEventArgs(IrcMessage message)
		{
			this.Message = message;
		}

		public OutputEventArgs(string text)
		{
			this.Text = text;
		}
	}
}
