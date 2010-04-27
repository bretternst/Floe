using System;
using System.Windows;
using System.Reflection;
using System.Linq;
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

		private static Lazy<string> product = new Lazy<string>(() =>
			typeof(App).Assembly.GetCustomAttributes(
					typeof(AssemblyProductAttribute), false).OfType<AssemblyProductAttribute>().FirstOrDefault().Product,
					true);
		public static string Product
		{
			get
			{
				return product.Value;
			}
		}

		public static string Version
		{
			get
			{
				return typeof(App).Assembly.GetName().Version.ToString();
			}
		}

		public App()
		{
			this.Startup += new StartupEventHandler(App_Startup);
			this.Exit += new ExitEventHandler(App_Exit);
		}

		public void ShowSettings()
		{
			var settings = new Settings.SettingsWindow();
			settings.ShowDialog();
		}

		private void OpenWindow()
		{
			var window = new ChatWindow(new IrcSession(App.Preferences.User.UserName,
				App.Preferences.User.HostName, App.Preferences.User.FullName));
			window.Closed += new EventHandler(window_Closed);
			window.Show();
		}

		private void App_Startup(object sender, StartupEventArgs e)
		{
			this.OpenWindow();

			if (App.Preferences.Servers.Count < 1)
			{
				ShowSettings();
			}
		}

		private void window_Closed(object sender, EventArgs e)
		{
			if (this.Windows.Count == 0)
			{
				this.Shutdown();
			}
		}

		private void App_Exit(object sender, ExitEventArgs e)
		{
			App.Configuration.Save();
		}
	}
}
