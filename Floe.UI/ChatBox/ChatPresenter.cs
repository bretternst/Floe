using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.TextFormatting;

namespace Floe.UI
{
	public partial class ChatPresenter : ChatBoxBase, IScrollInfo
	{
		private Queue<ChatLine> _lines = new Queue<ChatLine>();
		private ScrollViewer _viewer;
		private bool _isAutoScrolling;

		public void AppendLine(ChatLine line)
		{
			_lines.Enqueue(line);

			while (_lines.Count > this.BufferLines)
			{
				_lines.Dequeue();
			}

			this.FormatText();

			if (_isAutoScrolling)
			{
				this.ScrollToEnd();
			}
		}
	}
}
