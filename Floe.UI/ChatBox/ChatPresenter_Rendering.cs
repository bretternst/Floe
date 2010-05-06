using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Floe.UI
{
	public partial class ChatPresenter : ChatBoxBase, IScrollInfo
	{
		private const double SeparatorPadding = 6.0;
		private const double DefaultColumnWidth = 100.0;

		private class FormattedLine
		{
			public ChatDecoration Decoration { get; set; }
			public Brush Foreground { get; set; }

			public string TimeString { get; set; }
			public int NickHashCode { get; set; }
			public string NickString { get; set; }
			public string TextString { get; set; }

			public TextLine Time { get; set; }
			public TextLine Nick { get; set; }
			public TextLine[] Text { get; set; }

			public int CharStart { get; set; }
			public int CharEnd { get; set; }
			public double Y { get; set; }
			public double NickX { get; set; }
			public double TextX { get; set; }
			public double Height { get; set; }
		}

		private List<FormattedLine> _blocks = new List<FormattedLine>();
		private double _lineHeight, _columnWidth = DefaultColumnWidth;
		private int _bottomBlock;

		private Typeface Typeface
		{
			get
			{
				return new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
			}
		}

		private Color BackgroundColor
		{
			get
			{
				if (this.Background is SolidColorBrush)
				{
					return ((SolidColorBrush)this.Background).Color;
				}
				return Colors.Black;
			}
		}

		private void FormatText()
		{
			_blocks.Clear();
			_extentHeight = 0.0;
			if (_lines.Count < 1 || this.ActualWidth < 1.0)
			{
				return;
			}

			foreach (var srcLine in _lines)
			{
				var nick = srcLine.Nick;
				if (!this.UseTabularView)
				{
					if (nick == "*")
					{
						nick = "* ";
					}
					else
					{
						nick = string.Format("<{0}> ", nick);
					}
				}

				_blocks.Add(new FormattedLine()
				{
					Foreground = this.Palette[srcLine.ColorKey],
					TimeString = (srcLine.Nick != "*" && this.ShowTimestamp) ?
						srcLine.Time.ToString(this.TimestampFormat+" ") : "",
					NickHashCode = srcLine.NickHashCode,
					NickString = nick,
					TextString = srcLine.Text,
					Decoration = srcLine.Decoration
				});
			}

			var formatter = new ChatFormatter(this.Typeface, this.FontSize, this.Foreground);
			_blocks.ForEach((l) =>
				{
					if (l.TimeString.Length > 0)
					{
						l.Time = formatter.Format(l.TimeString, this.ActualWidth, l.Foreground, TextWrapping.NoWrap).First();
						l.NickX = l.Time.WidthIncludingTrailingWhitespace;
					}
				});

			if (this.UseTabularView)
			{
				double nickX = _blocks.Max((b) => b.NickX);
				_blocks.ForEach((b) => b.NickX = nickX);
			}

			_blocks.ForEach((l) =>
				{
					var nickBrush = l.Foreground;
					if (this.ColorizeNicknames && l.NickHashCode != 0)
					{
						nickBrush = this.GetNickColor(l.NickHashCode);
					}
					l.Nick = formatter.Format(l.NickString, this.ActualWidth - l.NickX, nickBrush, TextWrapping.NoWrap).First();
					l.TextX = l.NickX + l.Nick.WidthIncludingTrailingWhitespace;
					_columnWidth = Math.Max(_columnWidth, l.TextX);
				});

			if (this.UseTabularView)
			{
				double textX = _columnWidth + SeparatorPadding * 2.0 + 1.0;
				_blocks.ForEach((b) =>
					{
						b.TextX = textX;
						b.NickX = _columnWidth - b.Nick.WidthIncludingTrailingWhitespace;
					});
			}

			var offset = 0;
			_blocks.ForEach((l) =>
				{
					l.Text = formatter.Format(l.TextString, this.ActualWidth - l.TextX, l.Foreground, TextWrapping.Wrap).ToArray();
					l.Height = l.Text.Sum((t) => t.Height);
					_extentHeight += l.Height;
					_lineHeight = l.Text[0].Height;
					l.CharStart = offset;
					offset += l.TimeString.Length + l.NickString.Length + l.TextString.Length;
					l.CharEnd = offset;
				});

			_extentHeight += _lineHeight;

			this.InvalidateVisual();
			if (_viewer != null)
			{
				_viewer.InvalidateScrollInfo();

				if (_isAutoScrolling)
				{
					this.ScrollToEnd();
				}
			}
		}

		private Brush GetNickColor(int hashCode)
		{
			var rand = new Random(hashCode);
			int rgb = rand.Next();
			Color c;
			do
			{
				var bg = this.BackgroundColor;
				c = Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
				rgb = (int)Math.Abs(bg.R - c.R) + (int)Math.Abs(bg.G - c.G) + (int)Math.Abs(bg.B - c.B);
			}
			while (rgb < 50);
			return new SolidColorBrush(c);
		}

		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);

			var m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
			var scaledPen = new Pen(this.Palette["Default"], 1 / m.M11);
			double guidelineHeight = scaledPen.Thickness;

			double vPos = this.ActualHeight, height = 0.0, minHeight = this.ExtentHeight - (this.VerticalOffset + this.ActualHeight);
			_bottomBlock = -1;
			var guidelines = new GuidelineSet();

			dc.DrawRectangle(this.Background, null, new Rect(new Size(this.ActualWidth, this.ActualHeight)));

			int i = 0;
			for (i = _blocks.Count - 1; i >= 0 && vPos >= -_lineHeight * 5.0; --i)
			{
				var block = _blocks[i];
				block.Y = double.NaN;

				bool drawAny = false;
				if (block.Text == null || block.Text.Length < 1)
				{
					continue;
				}
				for (int j = block.Text.Length - 1; j >= 0; --j)
				{
					var line = block.Text[j];
					if ((height += line.Height) <= minHeight)
					{
						continue;
					}
					vPos -= line.Height;
					drawAny = true;
				}
				if (drawAny)
				{
					block.Y = vPos;

					if ((block.Decoration & ChatDecoration.NewMarker) > 0)
					{
						var markerBrush = new LinearGradientBrush(this.NewMarkerColor,
							this.BackgroundColor, 90.0);
						dc.DrawRectangle(markerBrush, null,
							new Rect(new Point(0.0, block.Y), new Size(this.ActualWidth, _lineHeight * 5)));
					}
					if ((block.Decoration & ChatDecoration.OldMarker) > 0)
					{
						var markerBrush = new LinearGradientBrush(this.BackgroundColor,
							this.OldMarkerColor, 90.0);
						dc.DrawRectangle(markerBrush, null,
							new Rect(new Point(0.0, (block.Y + block.Height) - _lineHeight * 5),
								new Size(this.ActualWidth, _lineHeight * 5)));
					}

					if (_bottomBlock < 0)
					{
						_bottomBlock = i;
					}
					guidelines.GuidelinesY.Add(vPos + guidelineHeight);
				}
			}

			dc.PushGuidelineSet(guidelines);

			for(int j = i + 1; j < _blocks.Count; j++)
			{
				var block = _blocks[j];

				if(double.IsNaN(block.Y))
				{
					continue;
				}

				block.Nick.Draw(dc, new Point(block.NickX, block.Y), InvertAxes.None);
				if (block.Time != null)
				{
					block.Time.Draw(dc, new Point(0.0, block.Y), InvertAxes.None);
				}
				for (int k = 0; k < block.Text.Length; k++)
				{
					block.Text[k].Draw(dc, new Point(block.TextX, block.Y + k * _lineHeight), InvertAxes.None);
				}

				if (this.IsSelecting)
				{
					this.DrawSelectedLine(dc, block);
				}
			}

			if (this.UseTabularView)
			{
				double lineX = _columnWidth + SeparatorPadding;
				dc.DrawLine(scaledPen, new Point(lineX, 0.0), new Point(lineX, this.ActualHeight));
			}
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			this.FormatText();
		}
	}
}
