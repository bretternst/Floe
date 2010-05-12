using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace Floe.UI
{
	public class ChatBoxBase : Control
	{
		public static readonly DependencyProperty BufferLinesProperty = DependencyProperty.Register("BufferLines",
			typeof(int), typeof(ChatBoxBase));
		public int BufferLines
		{
			get { return (int)this.GetValue(BufferLinesProperty); }
			set { this.SetValue(BufferLinesProperty, value); }
		}

		public static readonly DependencyProperty MinimumCopyLengthProperty = DependencyProperty.Register("MinimumCopyLength",
			typeof(int), typeof(ChatBoxBase));
		public int MinimumCopyLength
		{
			get { return (int)this.GetValue(MinimumCopyLengthProperty); }
			set { this.SetValue(MinimumCopyLengthProperty, value); }
		}

		public static readonly DependencyProperty PaletteProperty = DependencyProperty.Register("Palette",
			typeof(IDictionary<string,Brush>), typeof(ChatBoxBase));
		public IDictionary<string,Brush> Palette
		{
			get { return (ChatPalette)this.GetValue(PaletteProperty); }
			set { this.SetValue(PaletteProperty, value); }
		}

		public static readonly DependencyProperty ShowTimestampProperty = DependencyProperty.Register("ShowTimestamp",
			typeof(bool), typeof(ChatBoxBase));
		public bool ShowTimestamp
		{
			get { return (bool)this.GetValue(ShowTimestampProperty); }
			set { this.SetValue(ShowTimestampProperty, value); }
		}

		public static readonly DependencyProperty TimestampFormatProperty = DependencyProperty.Register("TimestampFormat",
			typeof(string), typeof(ChatBoxBase));
		public string TimestampFormat
		{
			get { return (string)this.GetValue(TimestampFormatProperty); }
			set { this.SetValue(TimestampFormatProperty, value); }
		}

		public static readonly DependencyProperty UseTabularViewProperty = DependencyProperty.Register("UseTabularView",
			typeof(bool), typeof(ChatBoxBase));
		public bool UseTabularView
		{
			get { return (bool)this.GetValue(UseTabularViewProperty); }
			set { this.SetValue(UseTabularViewProperty, value); }
		}

		public static readonly DependencyProperty ColorizeNicknamesProperty = DependencyProperty.Register("ColorizeNicknames",
			typeof(bool), typeof(ChatBoxBase));
		public bool ColorizeNicknames
		{
			get { return (bool)this.GetValue(ColorizeNicknamesProperty); }
			set { this.SetValue(ColorizeNicknamesProperty, value); }
		}

		public static readonly DependencyProperty NicknameColorSeedProperty = DependencyProperty.Register("NicknameColorSeed",
			typeof(int), typeof(ChatBoxBase));
		public int NicknameColorSeed
		{
			get { return (int)this.GetValue(NicknameColorSeedProperty); }
			set { this.SetValue(NicknameColorSeedProperty, value); }
		}

		public static readonly DependencyProperty NewMarkerColorProperty = DependencyProperty.Register("NewMarkerColor",
			typeof(Color), typeof(ChatBoxBase));
		public Color NewMarkerColor
		{
			get { return (Color)this.GetValue(NewMarkerColorProperty); }
			set { this.SetValue(NewMarkerColorProperty, value); }
		}

		public static readonly DependencyProperty OldMarkerColorProperty = DependencyProperty.Register("OldMarkerColor",
			typeof(Color), typeof(ChatBoxBase));
		public Color OldMarkerColor
		{
			get { return (Color)this.GetValue(OldMarkerColorProperty); }
			set { this.SetValue(OldMarkerColorProperty, value); }
		}

		public static readonly DependencyProperty DividerBrushProperty = DependencyProperty.Register("DividerBrush",
			typeof(Brush), typeof(ChatBoxBase));
		public Brush DividerBrush
		{
			get { return (Brush)this.GetValue(DividerBrushProperty); }
			set { this.SetValue(DividerBrushProperty, value); }
		}

		public static readonly DependencyProperty SelectedLinkProperty = DependencyProperty.Register("SelectedLink",
			typeof(string), typeof(ChatBoxBase));
		public string SelectedLink
		{
			get { return (string)this.GetValue(SelectedLinkProperty); }
			set { this.SetValue(SelectedLinkProperty, value); }
		}
	}
}
