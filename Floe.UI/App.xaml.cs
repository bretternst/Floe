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
		private static Lazy<PersistentSettings> _config =
			new Lazy<PersistentSettings>(() => new PersistentSettings(), true);

		public static PersistentSettings Settings
		{
			get
			{
				return _config.Value;
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

		public static void ShowSettings()
		{
			var settings = new Settings.SettingsWindow();
			settings.Owner = Application.Current.MainWindow;
			settings.ShowDialog();
		}

		public static void BrowseTo(string url)
		{
			System.Diagnostics.Process.Start(url);
		}

		public App()
		{
			this.Startup += new StartupEventHandler(App_Startup);
			this.Exit += new ExitEventHandler(App_Exit);
		}

		private void OpenWindow()
		{
			var window = new ChatWindow();
			window.Closed += new EventHandler(window_Closed);
			window.Show();
		}

		private void App_Startup(object sender, StartupEventArgs e)
		{
			this.OpenWindow();
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
			App.Settings.Save();
		}
	}
}
