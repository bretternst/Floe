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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
