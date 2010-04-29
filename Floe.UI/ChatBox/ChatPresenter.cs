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
		private Queue<string> _lines = new Queue<string>();
		private ScrollViewer _viewer;
		private bool _isAutoScrolling;

		public void AppendLine(string text)
		{
			_lines.Enqueue(text);

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
