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

namespace Floe.UI
{
	public partial class ConfirmDialog : Window
	{
		public ConfirmDialog(string title, string message, bool showDontAskAgainCheckbox)
		{
			InitializeComponent();

			this.Title = title;
			txtMessage.Text = message;
			if (!showDontAskAgainCheckbox)
			{
				chkDontAskAgain.Visibility = Visibility.Collapsed;
			}
		}

		public bool IsDontAskAgainChecked { get { return chkDontAskAgain.IsChecked.Value; } }

		private void btnYes_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			this.Close();
		}

		private void btnNo_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			chkDontAskAgain.IsChecked = false;
			this.Close();
		}
	}
}
