using System;
using System.Windows;
using System.Windows.Input;

using Floe.Net;

namespace Floe.UI
{
	public partial class ChannelWindow : Window
	{
		private void ExecuteChat(object sender, ExecutedRoutedEventArgs e)
		{
			this.BeginInvoke(() => App.Create(this.Control.Session, new IrcTarget((string)e.Parameter), true));
		}
	}
}
