using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Floe.UI
{
	public partial class ChatPresenter : ChatBoxBase, IScrollInfo
	{
		private const double SeparatorPadding = 6.0;

		private class Block
		{
			public ChatLine Source { get; set; }
			public Brush Foreground { get; set; }

			public string TimeString { get; set; }
			public string NickString { get; set; }

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

		private LinkedList<Block> _blocks = new LinkedList<Block>();
		private double _lineHeight;
		private LinkedListNode<Block> _bottomBlock;

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

		private string FormatNick(string nick)
		{
			if (!this.UseTabularView)
			{
				if (nick == null)
				{
					nick = "* ";
				}
				else
				{
					nick = string.Format("<{0}> ", nick);
				}
			}
			return nick ?? "*";
		}

		private string FormatTime(DateTime time)
		{
			return this.ShowTimestamp ? time.ToString(this.TimestampFormat + " ") : "";
		}

		private Brush GetNickColor(int hashCode)
		{
			var rand = new Random(hashCode);
			int rgb = 0;
			Color c = Colors.Black;
			do
			{
				rgb = rand.Next();
				for (int i = 0; i < this.NicknameColorSeed; i++)
				{
					rgb = rand.Next();
				}

				var bg = this.BackgroundColor;
				c = Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
				rgb = (int)Math.Abs(bg.R - c.R) + (int)Math.Abs(bg.G - c.G) + (int)Math.Abs(bg.B - c.B);
			}
			while (rgb < 125);
			return new SolidColorBrush(c);
		}

		private void FormatSingle(ChatLine source)
		{
			if (string.IsNullOrEmpty(source.Text))
			{
				return;
			}
			var b = new Block();
			b.Source = source;
			b.Foreground = this.Palette[b.Source.ColorKey];
			b.TimeString = this.FormatTime(b.Source.Time);
			b.NickString = this.FormatNick(b.Source.Nick);

			var formatter = new ChatFormatter(this.Typeface, this.FontSize, this.Foreground, this.Palette);
			b.Time = formatter.Format(b.TimeString, null, this.ViewportWidth, b.Foreground, this.Background,
				TextWrapping.NoWrap).FirstOrDefault();
			b.NickX = b.Time != null ? b.Time.WidthIncludingTrailingWhitespace : 0.0;

			var nickBrush = b.Foreground;
			if (this.ColorizeNicknames && b.Source.NickHashCode != 0)
			{
				nickBrush = this.GetNickColor(b.Source.NickHashCode);
			}
			b.Nick = formatter.Format(b.NickString, null, this.ViewportWidth - b.NickX, nickBrush, this.Background,
				TextWrapping.NoWrap).First();
			b.TextX = b.NickX + b.Nick.WidthIncludingTrailingWhitespace;

			if (this.UseTabularView)
			{
				if (b.TextX > this.ColumnWidth)
				{
					if (this.AutoSizeColumn)
					{
						this.ColumnWidth = b.TextX;
						_blocks.AddLast(b);
						this.FormatAll();
						return;
					}
					else
					{
						b.Nick = formatter.Format(b.NickString, null, this.ColumnWidth - (b.Time != null ? b.Time.Width : 0.0),
							nickBrush, this.Background, TextWrapping.NoWrap).First();
					}
				}
				b.TextX = this.ColumnWidth + SeparatorPadding * 2.0 + 1.0;
				b.NickX = this.ColumnWidth - b.Nick.WidthIncludingTrailingWhitespace;
			}

			var offset = _blocks.Last != null ? _blocks.Last.Value.CharEnd : 0;
			b.Text = formatter.Format(b.Source.Text, b.Source, this.ViewportWidth - b.TextX, b.Foreground,
				this.Background, TextWrapping.Wrap).ToArray();
			b.Height = b.Text.Sum((t) => t.Height);
			_lineHeight = b.Text[0].Height;
			_extentHeight = Math.Max(_extentHeight, _lineHeight);
			_extentHeight += b.Height;
			b.CharStart = offset;
			offset += b.TimeString.Length + b.NickString.Length + b.Source.Text.Length;
			b.CharEnd = offset;

			_blocks.AddLast(b);

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

		private void FormatAll()
		{
			_extentHeight = 0.0;
			this.InvalidateVisual();

			if (_blocks.Count < 1 || this.ViewportWidth < 1.0)
			{
				return;
			}

			var formatter = new ChatFormatter(this.Typeface, this.FontSize, this.Foreground, this.Palette);

			_blocks.ForEach((b) =>
				{
					b.Foreground = this.Palette[b.Source.ColorKey];
					b.TimeString = this.FormatTime(b.Source.Time);
					b.NickString = this.FormatNick(b.Source.Nick);
					b.NickX = b.TextX = 0.0;

					b.Time = formatter.Format(b.TimeString, null, this.ViewportWidth, b.Foreground, this.Background,
						TextWrapping.NoWrap).FirstOrDefault();
					b.NickX = b.Time != null ? b.Time.WidthIncludingTrailingWhitespace : 0.0;
					this.ColumnWidth = Math.Max(this.ColumnWidth, b.NickX);
				});

			double nickX = _blocks.Max((b) => b.NickX);

			_blocks.ForEach((b) =>
				{
					var nickBrush = b.Foreground;
					if (this.ColorizeNicknames && b.Source.NickHashCode != 0)
					{
						nickBrush = this.GetNickColor(b.Source.NickHashCode);
					}
					b.Nick = formatter.Format(b.NickString, null,
						this.UseTabularView ? this.ColumnWidth - (b.Time != null ? b.Time.Width : 0.0) : this.ViewportWidth,
						nickBrush, this.Background, TextWrapping.NoWrap).First();
					if (this.UseTabularView)
					{
						b.NickX = nickX;
					}
					b.TextX = b.NickX + b.Nick.WidthIncludingTrailingWhitespace;
				});

			double textX = this.ColumnWidth + SeparatorPadding * 2.0 + 1.0;

			var offset = 0;
			_blocks.ForEach((b) =>
				{
					if (this.UseTabularView)
					{
						b.TextX = textX;
						b.NickX = Math.Max(b.Time != null ? b.Time.Width : 0.0, this.ColumnWidth - b.Nick.Width);
					}
					b.Text = formatter.Format(b.Source.Text, b.Source, this.ViewportWidth - b.TextX, b.Foreground,
						this.Background, TextWrapping.Wrap).ToArray();
					b.Height = b.Text.Sum((t) => t.Height);
					_extentHeight += b.Height;
					_lineHeight = b.Text[0].Height;
					b.CharStart = offset;
					offset += b.TimeString.Length + b.NickString.Length + b.Source.Text.Length;
					b.CharEnd = offset;
				});

			_extentHeight += _lineHeight;

			if (_viewer != null)
			{
				_viewer.InvalidateScrollInfo();

				if (_isAutoScrolling)
				{
					this.ScrollToEnd();
				}
			}
		}

		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);

			var visual = PresentationSource.FromVisual(this);
			if (visual == null)
			{
				return;
			}
			var m = visual.CompositionTarget.TransformToDevice;
			var scaledPen = new Pen(this.DividerBrush, 1 / m.M11);
			double guidelineHeight = scaledPen.Thickness;

			double vPos = this.ActualHeight, height = 0.0, minHeight = this.ExtentHeight - (this.VerticalOffset + this.ActualHeight);
			_bottomBlock = null;
			var guidelines = new GuidelineSet();

			dc.DrawRectangle(Brushes.Transparent, null, new Rect(new Size(this.ViewportWidth, this.ActualHeight)));

			var node = _blocks.Last;
			do
			{
				if (node == null)
				{
					break;
				}

				var block = node.Value;
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

					if ((block.Source.Marker & ChatMarker.NewMarker) > 0)
					{
						var markerBrush = new LinearGradientBrush(this.NewMarkerColor,
							this.NewMarkerTransparentColor, 90.0);
						dc.DrawRectangle(markerBrush, null,
							new Rect(new Point(0.0, block.Y), new Size(this.ViewportWidth, _lineHeight * 5)));
					}
					if ((block.Source.Marker & ChatMarker.OldMarker) > 0)
					{
						var markerBrush = new LinearGradientBrush(this.OldMarkerTransparentColor,
							this.OldMarkerColor, 90.0);
						dc.DrawRectangle(markerBrush, null,
							new Rect(new Point(0.0, (block.Y + block.Height) - _lineHeight * 5),
								new Size(this.ViewportWidth, _lineHeight * 5)));
					}

					if (_bottomBlock == null)
					{
						_bottomBlock = node;
					}
					guidelines.GuidelinesY.Add(vPos + guidelineHeight);
				}
			}
			while (node.Previous != null && vPos >= -_lineHeight * 5.0 && (node = node.Previous) != null);

			dc.PushGuidelineSet(guidelines);

			if (this.UseTabularView)
			{
				double lineX = this.ColumnWidth + SeparatorPadding;
				dc.DrawLine(scaledPen, new Point(lineX, 0.0), new Point(lineX, this.ActualHeight));
			}

			if (_blocks.Count < 1)
			{
				return;
			}

			do
			{
				var block = node.Value;
				if (double.IsNaN(block.Y))
				{
					continue;
				}

				if ((block.Source.Marker & ChatMarker.Attention) > 0)
				{
					var markerBrush = new SolidColorBrush(this.AttentionColor);
					dc.DrawRectangle(markerBrush, null,
						new Rect(new Point(block.TextX, block.Y), new Size(this.ViewportWidth - block.TextX, block.Height)));
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
			while ((node = node.Next) != null);
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			this.FormatAll();
		}
	}
}
