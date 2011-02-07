using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Floe.Net;

namespace Floe.UI
{
	public class ChatTabItem : TabItem
	{
		private ChatPage _content;

		public ChatPage Page { get { return _content; } }

		public ChatTabItem(ChatPage page)
		{
			_content = page;
			this.Content = page;
		}

		public void Dispose()
		{
			_content.Dispose();
		}
	}
}
