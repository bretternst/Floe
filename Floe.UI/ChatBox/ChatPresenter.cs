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
		private bool _isAutoScrolling = true;

		public ChatPresenter()
		{
			this.Loaded += (sender, e) =>
				{
					if (_isAutoScrolling)
					{
						this.ScrollToEnd();
					}
				};
		}

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

		public void Clear()
		{
			_lines.Clear();
			this.FormatText();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.Property == Control.FontFamilyProperty ||
				e.Property == Control.FontSizeProperty ||
				e.Property == Control.FontStyleProperty ||
				e.Property == Control.FontWeightProperty ||
				e.Property == ChatBoxBase.PaletteProperty)
			{
				this.FormatText();
			}
			
			base.OnPropertyChanged(e);
		}
	}
}
