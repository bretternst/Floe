using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;

namespace Floe.UI.Settings
{
	public partial class SettingsWindow : Window
	{
		public SettingsWindow()
		{
			InitializeComponent();

			App.Settings.Save();

			grdSettings.Children.Add(new UserSettingsControl());
			grdSettings.Children.Add(new ServerSettingsControl());
			grdSettings.Children.Add(new FormattingSettingsControl());
			grdSettings.Children.Add(new ColorsSettingsControl());
			grdSettings.Children.Add(new BufferSettingsControl());
			grdSettings.Children.Add(new WindowSettingsControl());
			grdSettings.Children.Add(new DccSettingsControl());
			grdSettings.Children.Add(new SoundSettingsControl());
			grdSettings.Children.Add(new NetworkSettingsControl());
			grdSettings.Children.Add(new VoiceSettingsControl());

			if (lstCategories.SelectedIndex < 0)
			{
				lstCategories.SelectedIndex = 0;
			}

			this.AddHandler(TextBox.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(SelectivelyIgnoreMouseButton));
			this.AddHandler(TextBox.GotKeyboardFocusEvent, new RoutedEventHandler(SelectAllText));
			this.AddHandler(TextBox.MouseDoubleClickEvent, new RoutedEventHandler(SelectAllText));
		}

		private void btnApply_Click(object sender, RoutedEventArgs e)
		{
			App.Settings.Save();
			this.Close();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			App.Settings.Load();
			this.Close();
		}

		private void lstCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			for (int i = 0; i < grdSettings.Children.Count; i++)
			{
				grdSettings.Children[i].Visibility = i == lstCategories.SelectedIndex ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
		{
			DependencyObject parent = e.OriginalSource as UIElement;
			while (parent != null && !(parent is TextBox))
			{
				parent = VisualTreeHelper.GetParent(parent);
			}

			if (parent != null)
			{
				var textBox = (TextBox)parent;
				if (!textBox.IsKeyboardFocusWithin && !textBox.AcceptsReturn)
				{
					textBox.Focus();
					e.Handled = true;
				}
			}
		}

		private void SelectAllText(object sender, RoutedEventArgs e)
		{
			var textBox = e.OriginalSource as TextBox;
			if (textBox != null && !textBox.AcceptsReturn)
			{
				textBox.SelectAll();
			}
		}
	}
}
