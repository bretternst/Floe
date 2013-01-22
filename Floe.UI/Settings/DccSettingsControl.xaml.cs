using System;
using System.Windows;
using System.Windows.Controls;

namespace Floe.UI.Settings
{
	public partial class DccSettingsControl : UserControl
	{
		public DccSettingsControl()
		{
			InitializeComponent();
		}

		private void btnBrowse_Click(object sender, RoutedEventArgs e)
		{
			string path = Interop.FolderBrowser.Show(Window.GetWindow(this), "Select the download location for receiving files.",
				App.Settings.Current.Dcc.DownloadFolder);
			if (!string.IsNullOrEmpty(path))
			{
				App.Settings.Current.Dcc.DownloadFolder = txtDownloadFolder.Text = path;
			}
		}
	}
}
