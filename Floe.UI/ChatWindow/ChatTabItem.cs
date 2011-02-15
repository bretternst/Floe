using System;
using System.Windows.Controls;

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
