﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Floe.UI.Settings
{
	public partial class ColorsSettingsControl : UserControl
	{
		public ColorsSettingsControl()
		{
			InitializeComponent();

			this.AddHandler(Button.ClickEvent, new RoutedEventHandler(OnButtonClicked));
		}

		private void OnButtonClicked(object sender, RoutedEventArgs e)
		{
			var button = e.OriginalSource as Button;
			if (button != null)
			{
				var color = ((SolidColorBrush)button.Foreground).Color;
				Interop.ColorPicker.PickColor(Window.GetWindow(this), ref color);
				button.Foreground = new SolidColorBrush(color);
			}
		}
	}
}
