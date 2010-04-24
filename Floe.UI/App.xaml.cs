using System;
using System.Windows;
using Floe.Configuration;
using Floe.Net;

namespace Floe.UI
{
    public partial class App : Application
    {
		private static Lazy<PersistentConfiguration> _config =
			new Lazy<PersistentConfiguration>(() => new PersistentConfiguration(), true);

		public static PersistentConfiguration Configuration
		{
			get
			{
				return _config.Value;
			}
		}

		public static PreferencesSection Preferences
		{
			get
			{
				return Configuration.Preferences;
			}
		}

		public App()
		{
			this.Startup += new StartupEventHandler(App_Startup);
			this.Exit += new ExitEventHandler(App_Exit);
		}

		private void App_Startup(object sender, StartupEventArgs e)
		{
			this.OpenWindow();

			if (App.Preferences.Servers.Count < 1)
			{
				ShowSettings();
			}
		}

		private void mainWindow_Closed(object sender, EventArgs e)
		{
			this.Shutdown();
		}

		private void App_Exit(object sender, ExitEventArgs e)
		{
			App.Configuration.Save();
		}
	}
}
