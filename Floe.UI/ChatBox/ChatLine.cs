using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.UI
{
	public class ChatLine
	{
		public string ColorKey { get; private set; }
		public string Text { get; private set; }

		public ChatLine(string colorKey, string text)
		{
			this.Text = text;
			this.ColorKey = colorKey;
		}

		public ChatLine(string text)
			: this(text, null)
		{
		}

		public int Length { get { return this.Text.Length; } }

		public override string ToString()
		{
			return this.Text;
		}
	}
}
