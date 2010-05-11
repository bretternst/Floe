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
	public partial class BufferSettingsControl : UserControl
	{
		public BufferSettingsControl()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{
			App.BrowseTo(App.LoggingPathBase);
		}
	}
}
