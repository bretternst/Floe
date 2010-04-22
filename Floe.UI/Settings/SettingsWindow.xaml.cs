using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Floe.UI.Settings
{
	public partial class SettingsWindow : Window
	{
		public SettingsWindow()
		{
			InitializeComponent();

			App.Configuration.Save();

			grdSettings.Children.Add(new UserSettingsControl());
			grdSettings.Children.Add(new Canvas());

			if (lstCategories.SelectedIndex < 0)
			{
				lstCategories.SelectedIndex = 0;
			}
		}

		private bool Validate()
		{
			for (var i = 0; i < grdSettings.Children.Count; i++)
			{
				var panel = grdSettings.Children[i] as IValidationPanel;
				if (panel != null && !panel.IsValid)
				{
					lstCategories.SelectedIndex = i;
					return false;
				}
			}
			return true;
		}

		private void btnApply_Click(object sender, RoutedEventArgs e)
		{
			if (this.Validate())
			{
				App.Configuration.Save();
				this.Close();
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			App.Configuration.Load();
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
