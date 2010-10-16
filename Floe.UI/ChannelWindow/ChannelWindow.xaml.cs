using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Floe.Net;

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
			var bgBinding = new Binding();
			bgBinding.Source = this;
			bgBinding.Path = new PropertyPath("UIBackground");
			control.SetBinding(ChatControl.UIBackgroundProperty, bgBinding);

			control.SetValue(Grid.RowProperty, 1);
			control.SetValue(Grid.ColumnSpanProperty, 2);
			grdRoot.Children.Add(control);
			control.Session.StateChanged += new EventHandler<EventArgs>(Session_StateChanged);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			this.Control.Session.StateChanged -= new EventHandler<EventArgs>(Session_StateChanged);

			var state = App.Settings.Current.Windows.States[this.Control.Context.Key];

			if (this.Control.Parent != null)
			{
				if (this.Control.IsConnected && this.Control.IsChannel)
				{
					this.Control.Session.Part(this.Control.Target.Name);
				}
				state.IsDetached = true;
				this.Control.Dispose();
			}
			else
			{
				state.IsDetached = false;
				var window = App.Current.MainWindow as ChatWindow;
				if (window != null)
				{
					window.Attach(this.Control);
				}
			}
			state.Placement = Interop.WindowHelper.Save(this);
		}

		private void Session_StateChanged(object sender, EventArgs e)
		{
			if (((IrcSession)sender).State == IrcSessionState.Connecting)
			{
				this.Invoke(() => this.Close());
			}
		}

		private void btnReattach_Click(object sender, RoutedEventArgs e)
		{
			grdRoot.Children.Remove(this.Control);
			this.Close();
		}
	}
}
