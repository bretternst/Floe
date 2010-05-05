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
		private class FormattedLine
		{
			public Brush Foreground { get; set; }

			public string TimeString { get; set; }
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
		private double _lineHeight;
		private int _bottomBlock;

		private Typeface Typeface
		{
			get
			{
				return new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
			}
		}

		protected void FormatText()
		{
			_blocks.Clear();
			_extentHeight = 0.0;

			foreach (var srcLine in _lines)
			{
				_blocks.Add(new FormattedLine()
				{
					Foreground = this.Palette[srcLine.ColorKey],
					TimeString = srcLine.Nick != "*" ? srcLine.Time.ToString("[HH:mm] ") : "",
					NickString = (srcLine.Nick != "*" ? "<" + srcLine.Nick + ">" : srcLine.Nick) + " ",
					TextString = srcLine.Text
				});
			}

			var formatter = new ChatFormatter(this.Typeface, this.FontSize, this.Foreground, this.Background);
			_blocks.ForEach((l) =>
				{
					if (l.TimeString.Length > 0)
					{
						l.Time = formatter.Format(l.TimeString, this.ActualWidth, l.Foreground, TextAlignment.Left,
							TextWrapping.NoWrap).First();
						l.NickX = l.Time.WidthIncludingTrailingWhitespace;
					}
				});
			_blocks.ForEach((l) =>
				{
					l.Nick = formatter.Format(l.NickString, this.ActualWidth - l.NickX, l.Foreground, TextAlignment.Left,
						TextWrapping.NoWrap).First();
					l.TextX = l.NickX + l.Nick.WidthIncludingTrailingWhitespace;
				});

			var offset = 0;
			_blocks.ForEach((l) =>
				{
					l.Text = formatter.Format(l.TextString, this.ActualWidth - l.TextX, l.Foreground, TextAlignment.Left,
						TextWrapping.Wrap).ToArray();
					l.Height = l.Text.Sum((t) => t.Height);
					_extentHeight += l.Height;
					_lineHeight = l.Text[0].Height;
					l.CharStart = offset;
					offset += l.TimeString.Length + l.NickString.Length + l.TextString.Length;
					l.CharEnd = offset;
				});

			_extentHeight += this.ActualHeight - _lineHeight;

			this.InvalidateVisual();
			if (_viewer != null)
			{
				_viewer.InvalidateScrollInfo();
			}
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			var m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
			var scaledPen = new Pen(Brushes.Black, 1 / m.M11);
			double guidelineHeight = scaledPen.Thickness;

			drawingContext.DrawRectangle(this.Background, null, new Rect(new Size(this.ActualWidth, this.ActualHeight)));

			double vPos = this.ActualHeight, height = 0.0, minHeight = this.ExtentHeight - (this.VerticalOffset + this.ActualHeight);
			_isAutoScrolling = true;
			_bottomBlock = -1;
			var guidelines = new GuidelineSet();

			for (int i = _blocks.Count - 1; i >= 0; --i)
			{
				var block = _blocks[i];
				bool drewAny = false;
				for (int j = block.Text.Length - 1; j >= 0; --j)
				{
					var line = block.Text[j];
					if ((height += line.Height) <= minHeight)
					{
						_isAutoScrolling = false;
						continue;
					}
					vPos -= line.Height;
					line.Draw(drawingContext, new Point(block.TextX, vPos), InvertAxes.None);
					drewAny = true;
				}
				if (drewAny)
				{
					block.Nick.Draw(drawingContext, new Point(block.NickX, vPos), InvertAxes.None);
					if (block.Time != null)
					{
						block.Time.Draw(drawingContext, new Point(0.0, vPos), InvertAxes.None);
					}
					block.Y = vPos;
					if (_bottomBlock < 0)
					{
						_bottomBlock = i;
					}
					guidelines.GuidelinesY.Add(vPos + guidelineHeight);
				}
				else
				{
					block.Y = double.NaN;
				}
				if (vPos <= 0.0)
				{
					break;
				}
			}
			drawingContext.PushGuidelineSet(guidelines);

			if (this.IsSelecting)
			{
				for (int i = 0; i < _blocks.Count; i++)
				{
					if (_blocks[i].Y >= 0.0)
					{
						this.DrawSelectedLine(drawingContext, _blocks[i]);
					}
				}
			}
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			this.FormatText();
		}
	}
}
