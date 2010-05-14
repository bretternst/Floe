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
using System.ComponentModel;

namespace Floe.UI
{
	public partial class ChannelWindow : Window
	{
		public ChatControl Control { get; private set; }

		public ChannelWindow(ChatControl control)
		{
			InitializeComponent();
			this.DataContext = this;

			this.Control = control;

			control.SetValue(Grid.RowProperty, 1);
			control.SetValue(Grid.ColumnSpanProperty, 2);
			grdRoot.Children.Add(control);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (this.Control.Parent != null)
			{
				if (this.Control.IsConnected && this.Control.IsChannel)
				{
					this.Control.Session.Part(this.Control.Target.Name);
				}
				App.Settings.Current.Windows.States.Set(this.Control.Context.GetKey(),
					Interop.WindowHelper.Save(this));
			}
			else
			{
				App.Settings.Current.Windows.States.Delete(this.Control.Context.GetKey());
			}
		}

		private void btnReattach_Click(object sender, RoutedEventArgs e)
		{
			grdRoot.Children.Remove(this.Control);
			this.Close();
		}
	}
}
