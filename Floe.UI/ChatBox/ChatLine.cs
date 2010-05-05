using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.UI
{
	public class ChatLine
	{
		public DateTime Time { get; private set; }
		public string ColorKey { get; private set; }
		public string Nick { get; private set; }
		public string Text { get; private set; }

		public ChatLine(string colorKey, string nick, string text)
		{
			this.Time = DateTime.Now;
			this.ColorKey = colorKey;
			this.Nick = nick;
			this.Text = text;
		}

		public override string ToString()
		{
			return this.Text;
		}
	}
}
