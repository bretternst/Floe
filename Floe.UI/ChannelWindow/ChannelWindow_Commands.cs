using System;
using System.Windows;
using System.Windows.Input;

using Floe.Net;

namespace Floe.UI
{
	public partial class ChannelWindow : Window
	{
		public readonly static RoutedUICommand ReattachCommand = new RoutedUICommand("Reattach", "Reattach", typeof(ChatWindow));

		private void ExecuteChat(object sender, ExecutedRoutedEventArgs e)
		{
			this.BeginInvoke(() => App.Create(this.Control.Session, new IrcTarget((string)e.Parameter), true));
		}

		private void ExecuteReattach(object sender, ExecutedRoutedEventArgs e)
		{
			grdRoot.Children.Remove(this.Control);
			this.Close();
		}

		private void ExecuteMinimize(object sender, ExecutedRoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}

		private void ExecuteMaximize(object sender, ExecutedRoutedEventArgs e)
		{
			if (this.WindowState == WindowState.Maximized)
			{
				this.WindowState = WindowState.Normal;
			}
			else
			{
				this.WindowState = WindowState.Maximized;
			}
		}

		private void ExecuteClose(object sender, ExecutedRoutedEventArgs e)
		{
			this.Close();
		}

		private void ExecuteSettings(object sender, ExecutedRoutedEventArgs e)
		{
			App.ShowSettings();
		}
	}
}
