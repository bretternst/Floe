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

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
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
			if (_isSelecting)
			{
				_selEnd = this.GetCharIndexAt(e.GetPosition(this));
				this.InvalidateVisual();
				e.Handled = true;
			}

			base.OnMouseMove(e);
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			if (!_isSelecting)
			{
				return;
			}

			string selText = this.GetSelectedText();
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
			if (p.Y > this.ActualHeight)
			{
				return int.MaxValue;
			}
			FormattedLine block = null;
			for (int j = _bottomBlock; j >= 0; --j)
			{
				if (p.Y >= _blocks[j].Y && p.Y < _blocks[j].Y + _blocks[j].Height)
				{
					block = _blocks[j];
					break;
				}
			}
			if (block == null)
			{
				return -1;
			}

			int line = (int)(p.Y - block.Y) / (int)_lineHeight;
			int idx = 0;
			if (line > 0 || p.X >= block.TextX)
			{
				idx += block.TimeString.Length + block.NickString.Length;
				var ch = block.Text[line].GetCharacterHitFromDistance(p.X - block.TextX);
				idx += Math.Min(ch.FirstCharacterIndex, block.TextString.Length - 1);
			}
			else if (p.X >= block.NickX || block.Time == null)
			{
				idx += block.TimeString.Length;
				var ch = block.Nick.GetCharacterHitFromDistance(p.X - block.NickX);
				idx += Math.Min(ch.FirstCharacterIndex, block.NickString.Length - 1);
			}
			else
			{
				var ch = block.Time.GetCharacterHitFromDistance(p.X);
				idx += Math.Min(ch.FirstCharacterIndex, block.TimeString.Length - 1);
			}
			return idx + block.CharStart;
		}

		private void FindSelectedArea(int idx, int txtLen, int txtOffset, double x, TextLine line, ref double start, ref double end)
		{
			int first = Math.Max(txtOffset, this.SelectionStart - idx);
			int last = Math.Min(txtLen - 1 + txtOffset, this.SelectionEnd - idx);
			if (last > first)
			{
				start = Math.Min(start, line.GetDistanceFromCharacterHit(new CharacterHit(first, 0)) + x);
				end = Math.Max(end, line.GetDistanceFromCharacterHit(new CharacterHit(last, 1)) + x);
			}
		}

		private static Lazy<Brush> _highlightBrush = new Lazy<Brush>(() =>
			{
				var c = SystemColors.HighlightColor;
				c.A = 102;
				return new SolidColorBrush(c);
			}, true);

		private void DrawSelectedLine(DrawingContext dc, FormattedLine block)
		{
			if (this.SelectionEnd < block.CharStart || this.SelectionStart >= block.CharEnd ||
				this.SelectionStart >= this.SelectionEnd)
			{
				return;
			}

			int idx = block.CharStart, txtOffset = 0;
			double y = block.Y;
			for (int i = 0; i < block.Text.Length; i++)
			{
				double start = double.MaxValue, end = double.MinValue;
				if (i == 0)
				{
					if (block.Time != null)
					{
						this.FindSelectedArea(idx, block.TimeString.Length, 0, 0.0, block.Time, ref start, ref end);
						idx += block.TimeString.Length;
					}
					this.FindSelectedArea(idx, block.NickString.Length, 0, block.NickX, block.Nick, ref start, ref end);
					idx += block.NickString.Length;
				}
				this.FindSelectedArea(idx, block.Text[i].Length, txtOffset, block.TextX, block.Text[i], ref start, ref end);
				txtOffset += block.Text[i].Length;

				if (end > start)
				{
					dc.DrawRectangle(_highlightBrush.Value, null,
						new Rect(new Point(start, y), new Point(end, y + _lineHeight)));
				}
				y += _lineHeight;
			}
		}

		private string GetSelectedText()
		{
			var output = new StringBuilder();
			foreach (var block in _blocks)
			{
				if (this.SelectionEnd < block.CharStart || this.SelectionStart >= block.CharEnd)
				{
					continue;
				}

				int idx = block.CharStart;
				output.Append(this.GetSelectedText(idx, block.TimeString, output));
				idx += block.TimeString.Length;
				output.Append(this.GetSelectedText(idx, block.NickString, output));
				idx += block.NickString.Length;
				output.Append(this.GetSelectedText(idx, block.TextString, output));
				if (this.SelectionEnd >= block.CharEnd)
				{
					output.AppendLine();
				}
			}
			return output.ToString();
		}

		private string GetSelectedText(int idx, string s, StringBuilder output)
		{
			int first = Math.Max(0, this.SelectionStart - idx);
			int last = Math.Min(s.Length - 1, this.SelectionEnd - idx);
			return last >= first ? s.Substring(first, last - first + 1) : "";
		}
	}
}
