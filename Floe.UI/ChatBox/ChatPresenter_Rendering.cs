using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Floe.UI
{
	public partial class ChatPresenter : Control, IScrollInfo
	{
		private class CustomTextRunProperties : TextRunProperties
		{
			private Typeface _typeface;
			private double _fontSize;
			private Brush _foreground;
			private Brush _background;

			public override double FontHintingEmSize { get { return 0.0; } }
			public override TextDecorationCollection TextDecorations { get { return null; } }
			public override TextEffectCollection TextEffects { get { return null; } }
			public override CultureInfo CultureInfo { get { return CultureInfo.InvariantCulture; } }
			public override Typeface Typeface { get { return _typeface; } }
			public override double FontRenderingEmSize { get { return _fontSize; } }
			public override Brush BackgroundBrush { get { return _background; } }
			public override Brush ForegroundBrush { get { return _foreground; } }

			public void SetForeground(Brush foreground)
			{
				_foreground = foreground;
			}

			public CustomTextRunProperties(Typeface typeface, double fontSize, Brush foreground, Brush background)
			{
				_typeface = typeface;
				_fontSize = fontSize;
				_foreground = foreground;
				_background = background;
			}
		}

		private class CustomParagraphProperties : TextParagraphProperties
		{
			private TextRunProperties _defaultTextRunProperties;
			private double _indent;

			public override FlowDirection FlowDirection { get { return FlowDirection.LeftToRight; } }
			public override TextAlignment TextAlignment { get { return TextAlignment.Left; } }
			public override double LineHeight { get { return 0.0; } }
			public override bool FirstLineInParagraph { get { return false; } }
			public override TextWrapping TextWrapping { get { return TextWrapping.Wrap; } }
			public override TextMarkerProperties TextMarkerProperties { get { return null; } }
			public override TextRunProperties DefaultTextRunProperties { get { return _defaultTextRunProperties; } }
			public override double Indent { get { return _indent; } }

			public void SetIndent(double indent)
			{
				_indent = indent;
			}

			public CustomParagraphProperties(TextRunProperties defaultTextRunProperties)
			{
				_defaultTextRunProperties = defaultTextRunProperties;
			}
		}

		private class LineSource : TextSource
		{
			TextRunProperties _defaultProperties;
			private IEnumerator<string> _enumerator;

			public LineSource(TextRunProperties defaultProperties, IEnumerable<string> lines)
			{
				_defaultProperties = defaultProperties;
				_enumerator = lines.GetEnumerator();
			}

			public int Length { get { return _enumerator.Current.Length; } }

			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			public override TextRun GetTextRun(int idx)
			{
				string text = _enumerator.Current;

				if (idx < text.Length)
				{
					return new TextCharacters(text, idx, text.Length - idx, _defaultProperties);
				}
				else
				{
					return new TextEndOfLine(1);
				}
			}

			public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
			{
				throw new NotImplementedException();
			}

			public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
			{
				throw new NotImplementedException();
			}
		}

		private double _extentHeight, _offset, _visibleLineCount;
		private List<TextLine> _output;
		private TextLine _lastLine;

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			drawingContext.DrawRectangle(this.Background, null, new Rect(new Size(this.ActualWidth, this.ActualHeight)));

			_isAutoScrolling = true;
			_visibleLineCount = 0;

			if (_lastLine == null)
			{
				return;
			}

			double height = 0.0, minHeight = _extentHeight - (_offset + this.ActualHeight - _lastLine.Height);
			double vPos = this.ActualHeight;

			for (int i = _output.Count - 1; i >= 0 && vPos >= 0.0; --i)
			{
				if ((height += _output[i].Height) < minHeight)
				{
					_isAutoScrolling = false;
					continue;
				}
				_visibleLineCount++;
				vPos -= _output[i].Height;
				_output[i].Draw(drawingContext, new Point(0.0, vPos), InvertAxes.None);
			}
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			this.FormatText();
		}

		private void FormatText()
		{
			var formatter = TextFormatter.Create(TextFormattingMode.Display);
			var textRunProperties = new CustomTextRunProperties(
				new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch),
				this.FontSize, this.Foreground, this.Background);
			var paraProperties = new CustomParagraphProperties(textRunProperties);
			var source = new LineSource(textRunProperties, _lines);

			_extentHeight = 0.0;
			_output.Clear();
			while (source.MoveNext())
			{
				int idx = 0;
				while (idx < source.Length)
				{
					paraProperties.SetIndent(idx > 0 ? 20.0 : 0.0);
					var textLine = formatter.FormatLine(source, idx, this.ActualWidth, paraProperties, null);
					_output.Add(textLine);
					idx += textLine.Length;
					_extentHeight += textLine.Height;
				}
			}
			this.InvalidateVisual();
			if (_viewer != null)
			{
				_viewer.InvalidateScrollInfo();
			}
			_lastLine = _output.Count > 0 ? _output[_output.Count - 1] : null;
			_extentHeight += this.ActualHeight - (_output.Count > 0 ? _output[0].Height : 0.0);
		}
	}
}
