﻿using System;
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
			App.Create(this.Page.Session, new IrcTarget((string)e.Parameter), true);
		}

		private void ExecuteReattach(object sender, ExecutedRoutedEventArgs e)
		{
			grdRoot.Children.Remove(this.Page);
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
			if (this.Page.Session.State == IrcSessionState.Connected && this.Page.Target.IsChannel)
			{
				this.Page.Session.Part(this.Page.Target.Name);
			}
			this.Close();
		}

		private void CanExecuteClose(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.Page.IsCloseable;
		}

		private void ExecuteSettings(object sender, ExecutedRoutedEventArgs e)
		{
			App.ShowSettings();
		}
	}
}
