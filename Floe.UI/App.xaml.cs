using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Floe.UI.Settings;
using Floe.Configuration;

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
		}

		private void App_Startup(object sender, StartupEventArgs e)
		{
			var wndSettings = new Settings.SettingsWindow();
			wndSettings.ShowDialog();
			this.Shutdown();
		}
    }
}
