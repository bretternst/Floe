using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Floe.Net;

namespace Floe.UI
{
	public enum OutputType
	{
		Client,
		Info,
		PrivateMessage,
		Notice,
		Topic,
		Nick,
		Action,
		Join,
		Part,
		Disconnected
	}

	public sealed class OutputEventArgs : EventArgs
	{
		public OutputType Type { get; private set; }
		public IrcPrefix From { get; private set; }
		public string Text { get; private set; }

		public OutputEventArgs(OutputType type, IrcPrefix from, string text)
		{
			this.Type = type;
			this.From = from;
			this.Text = text;
		}

		public OutputEventArgs(OutputType type, string text)
			: this(type, null, text)
		{
		}
	}
}
