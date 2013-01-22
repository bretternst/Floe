using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Floe.Configuration;

namespace Floe.UI.Settings
{
	public partial class SoundSettingsControl : UserControl
	{
		public SoundSettingsControl()
		{
			InitializeComponent();

			lstEvents.SelectedIndex = 0;
		}

		private void btnBrowse_Click(object sender, RoutedEventArgs e)
		{
			string path;
			try
			{
				path = System.IO.Path.GetDirectoryName(Environment.ExpandEnvironmentVariables(txtPath.Text));
			}
			catch (ArgumentException)
			{
				path = "";
			}
			path = App.OpenFileDialog(Window.GetWindow(this), path);
			if (!string.IsNullOrEmpty(path))
			{
				((ListBoxItem)lstEvents.SelectedItem).Tag = txtPath.Text = path;
			}
		}
	}
}
