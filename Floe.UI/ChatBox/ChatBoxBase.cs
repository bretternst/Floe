using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
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
			typeof(ChatPalette), typeof(ChatBoxBase));
		public ChatPalette Palette
		{
			get { return (ChatPalette)this.GetValue(PaletteProperty); }
			set { this.SetValue(PaletteProperty, value); }
		}
	}
}
