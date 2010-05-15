using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

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
			var state = App.Settings.Current.Windows.States[this.Control.Context.Key];

			if (this.Control.Parent != null)
			{
				if (this.Control.IsConnected && this.Control.IsChannel)
				{
					this.Control.Session.Part(this.Control.Target.Name);
				}
				state.IsDetached = true;
			}
			else
			{
				state.IsDetached = false;
			}
			state.Placement = Interop.WindowHelper.Save(this);
		}

		private void btnReattach_Click(object sender, RoutedEventArgs e)
		{
			grdRoot.Children.Remove(this.Control);
			this.Close();
		}
	}
}
