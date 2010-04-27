using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using Floe.Net;

namespace Floe.UI
{
	public partial class App : Application
	{
		public void ShowSettings()
		{
			var settings = new Settings.SettingsWindow();
			settings.ShowDialog();
		}

		private static IrcSession CreateSession()
		{
			return new IrcSession(App.Preferences.User.UserName, App.Preferences.User.HostName, App.Preferences.User.FullName);
		}

		private void OpenWindow()
		{
			var window = new ChatWindow(CreateSession());
			window.Closed += new EventHandler(mainWindow_Closed);
			window.Show();
			this.MainWindow = window;
		}
	}
}
