using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Floe.UI
{
	public partial class ChatPresenter : ChatBoxBase, IScrollInfo
	{
		private bool _isSelecting;
		private int _selStart = -1, _selEnd = -1;

		protected int SelectionStart
		{
			get
			{
				return Math.Min(_selStart, _selEnd);
			}
		}

		protected int SelectionEnd
		{
			get
			{
				return Math.Max(_selStart, _selEnd);
			}
		}

		protected bool IsSelecting
		{
			get
			{
				return _isSelecting && this.SelectionEnd > this.SelectionStart;
			}
		}

		protected string SelectedText
		{
			get
			{
				int start = this.SelectionStart, end = this.SelectionEnd;
				if (start >= 0 && end > 0 && end > start)
				{
					var output = new StringBuilder();
					var baseIdx = 0;
					foreach (var s in _lines)
					{
						start = Math.Max(0, this.SelectionStart - baseIdx);
						end = Math.Min(s.Length, this.SelectionEnd - baseIdx + 1);
						if(end > start)
						{
							output.Append(s.Text.Substring(start, end - start));
							if (this.SelectionEnd > baseIdx + end)
							{
								output.AppendLine();
							}
						}
						baseIdx += s.Length + 1;
						if(this.SelectionEnd < baseIdx)
						{
							break;
						}
					}
					return output.ToString();
				}
				else
				{
					return "";
				}
			}
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (_lastLine == null)
			{
				return;
			}

			var p = e.GetPosition(this);
			int idx = this.GetCharIndexAt(p);
			if (idx >= 0 && idx < int.MaxValue)
			{
				_isSelecting = true;
				Mouse.OverrideCursor = Cursors.IBeam;
				this.CaptureMouse();
				_selStart = idx;
			}

			base.OnMouseDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (!_isSelecting)
			{
				return;
			}

			_selEnd = this.GetCharIndexAt(e.GetPosition(this));

			this.InvalidateVisual();
			base.OnMouseMove(e);
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			if (!_isSelecting)
			{
				return;
			}

			string selText = this.SelectedText;
			if (selText.Length >= this.MinimumCopyLength)
			{
				Clipboard.SetText(selText);
			}

			this.ReleaseMouseCapture();
			Mouse.OverrideCursor = null;
			_isSelecting = false;
			_selStart = -1;
			_selEnd = -1;

			this.InvalidateVisual();
			base.OnMouseMove(e);
		}

		private int GetCharIndexAt(Point p)
		{
			double x = p.X, y = this.ActualHeight - p.Y;
			if (y < 0.0)
			{
				return int.MaxValue;
			}

			double vPos = 0.0;
			int i = 0;
			for (i = _lastVisibleLineIdx; i >= 0 && vPos <= this.ViewportHeight; --i)
			{
				vPos += _output[i].Height;
				if (vPos > y)
				{
					break;
				}
			}
			if (i < 0)
			{
				i = 0;
			}
			var ch = _output[i].GetCharacterHitFromDistance(x);
			return ch.FirstCharacterIndex;
		}

		private void DrawSelectedLine(DrawingContext dc, double y, int idx, TextFormatter formatter,
			LineSource source, CustomParagraphProperties paraProperties)
		{
			TextLine line = _output[idx];
			int lineStart = line.GetCharacterHitFromDistance(0.0).FirstCharacterIndex;
			int lineEnd = lineStart + line.Length;
			int selStart = Math.Max(lineStart, this.SelectionStart);
			int selEnd = Math.Min(lineEnd, this.SelectionEnd);

			if (selStart <= lineEnd && selEnd >= lineStart)
			{
				foreach (var bounds in line.GetTextBounds(selStart, selEnd - selStart + 1))
				{
					var rect = new Rect(bounds.Rectangle.X, bounds.Rectangle.Y + y, bounds.Rectangle.Width, bounds.Rectangle.Height + 1);
					dc.DrawRectangle(SystemColors.HighlightBrush, null, rect);
					var clip = new RectangleGeometry(rect);
					dc.PushClip(clip);
					var selText = formatter.FormatLine(source, selStart, this.ViewportWidth, paraProperties, null);
					selText.Draw(dc, new Point(rect.X, y), InvertAxes.None);
					dc.Pop();
				}
			}
		}
	}
}
