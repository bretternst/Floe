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
		public int NickHashCode { get; private set; }
		public string Nick { get; private set; }
		public string Text { get; private set; }
		public ChatDecoration Decoration { get; set; }

		public ChatLine(string colorKey, DateTime time, int nickHashCode, string nick, string text, ChatDecoration decoration)
		{
			this.ColorKey = colorKey;
			this.Time = time;
			this.NickHashCode = nickHashCode;
			this.Nick = nick;
			this.Text = text;
			this.Decoration = decoration;
		}

		public ChatLine(string colorKey, int nickHashCode, string nick, string text, ChatDecoration decoration)
			: this(colorKey, DateTime.Now, nickHashCode, nick, text, decoration)
		{
		}

		public override string ToString()
		{
			return this.Text;
		}
	}
}
