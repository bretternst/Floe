using System;
using System.Windows;
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
			grdSettings.Children.Add(new TextSettingsControl());
			grdSettings.Children.Add(new ColorsSettingsControl());
			grdSettings.Children.Add(new BufferSettingsControl());

			if (lstCategories.SelectedIndex < 0)
			{
				lstCategories.SelectedIndex = 0;
			}
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
	}
}
