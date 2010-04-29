using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.TextFormatting;

namespace Floe.UI
{
	public partial class ChatPresenter : Control, IScrollInfo
	{
		private Queue<string> _lines;
		private ScrollViewer _viewer;
		private bool _isAutoScrolling;

		public static DependencyProperty BufferLinesProperty = DependencyProperty.Register("BufferLines",
			typeof(int), typeof(ChatPresenter));
		public int BufferLines
		{
			get { return (int)this.GetValue(BufferLinesProperty); }
			set { this.SetValue(BufferLinesProperty, value); }
		}

		public ChatPresenter()
		{
			_lines = new Queue<string>();
			_output = new List<TextLine>();
		}

		public void AppendLine(string text)
		{
			_lines.Enqueue(text);

			while (_lines.Count > this.BufferLines)
			{
				_lines.Dequeue();
			}

			this.FormatText();

			if (_isAutoScrolling && !_isSelecting)
			{
				this.ScrollToEnd();
			}
		}
	}
}
