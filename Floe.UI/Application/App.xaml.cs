using System;
using System.Windows;
using Floe.Net;

namespace Floe.UI
{
    public partial class App : Application
    {
		public App()
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				LogUnhandledException(e.ExceptionObject);
			};

			NatHelper.BeginDiscover((ar) => NatHelper.EndDiscover(ar));
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			var window = new ChatWindow();
			window.Closed += new EventHandler(window_Closed);
			window.Show();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			App.Settings.Save();
		}

		private void window_Closed(object sender, EventArgs e)
		{
			if (this.Windows.Count == 0)
			{
				this.Shutdown();
			}
		}
	}
}
