using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Floe.UI
{
	public class ChatFormatter : TextSource
	{
		#region Nested classes

		private class CustomTextRunProperties : TextRunProperties
		{
			private Typeface _typeface;
			private double _fontSize;
			private Brush _foreground;

			public override double FontHintingEmSize { get { return _fontSize; } }
			public override TextDecorationCollection TextDecorations { get { return null; } }
			public override TextEffectCollection TextEffects { get { return null; } }
			public override CultureInfo CultureInfo { get { return CultureInfo.InvariantCulture; } }
			public override Typeface Typeface { get { return _typeface; } }
			public override double FontRenderingEmSize { get { return _fontSize; } }
			public override Brush BackgroundBrush { get { return Brushes.Transparent; } }
			public override Brush ForegroundBrush { get { return _foreground; } }

			public CustomTextRunProperties(Typeface typeface, double fontSize, Brush foreground)
			{
				_typeface = typeface;
				_fontSize = fontSize;
				_foreground = foreground;
			}
		}

		private class CustomParagraphProperties : TextParagraphProperties
		{
			private TextRunProperties _defaultProperties;
			private TextWrapping _textWrapping;

			public override FlowDirection FlowDirection { get { return FlowDirection.LeftToRight; } }
			public override TextAlignment TextAlignment { get { return TextAlignment.Left; } }
			public override double LineHeight { get { return 0.0; } }
			public override bool FirstLineInParagraph { get { return false; } }
			public override TextWrapping TextWrapping { get { return _textWrapping; } }
			public override TextMarkerProperties TextMarkerProperties { get { return null; } }
			public override TextRunProperties DefaultTextRunProperties { get { return _defaultProperties; } }
			public override double Indent { get { return 0.0; } }

			public CustomParagraphProperties(TextRunProperties defaultTextRunProperties)
			{
				_defaultProperties = defaultTextRunProperties;
				_textWrapping = TextWrapping.Wrap;
			}
		}

		#endregion

		private string _text;
		private CustomTextRunProperties _runProperties;
		private CustomParagraphProperties _paraProperties;
		private TextFormatter _formatter;

		public ChatFormatter(Typeface typeface, double fontSize, Brush foreground)
		{
			_runProperties = new CustomTextRunProperties(typeface, fontSize, foreground);
			_paraProperties = new CustomParagraphProperties(_runProperties);
			_formatter = TextFormatter.Create(TextFormattingMode.Display);
		}

		public IEnumerable<TextLine> Format(string text, double width, Brush foreground, TextWrapping textWrapping)
		{
			_text = text;
			_runProperties = new CustomTextRunProperties(_runProperties.Typeface, _runProperties.FontRenderingEmSize, foreground);
			_paraProperties = new CustomParagraphProperties(_runProperties);

			int idx = 0;
			while(idx < _text.Length)
			{
				var line = _formatter.FormatLine(this, idx, width, _paraProperties, null);
				idx += line.Length;
				yield return line;
			}
		}

		public override TextRun GetTextRun(int idx)
		{
			if (idx >= _text.Length)
			{
				return new TextEndOfLine(1);
			}
			else
			{
				return new TextCharacters(_text, idx, _text.Length - idx, _runProperties);
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
}
