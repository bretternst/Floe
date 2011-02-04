using System;
using System.Windows;
using System.Reflection;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using Floe.Configuration;
using Floe.Net;

namespace Floe.UI
{
    public partial class App : Application
    {
		public static ChatPage ActiveChatPage
		{
			get
			{
				var chatWindow = Application.Current.Windows.OfType<ChatWindow>().FirstOrDefault((w) => w.IsActive);
				if (chatWindow != null)
				{
					return chatWindow.ActiveControl;
				}
				else
				{
					var channelWindow = Application.Current.Windows.OfType<ChannelWindow>().FirstOrDefault((w) => w.IsActive);
					if (channelWindow != null)
					{
						return channelWindow.Page;
					}
				}

				return null;
			}
		}

		public static void ShowSettings()
		{
			var settings = new Settings.SettingsWindow();
			settings.Owner = Application.Current.MainWindow;
			settings.ShowDialog();
			App.RefreshAttentionPatterns();
		}

		public static bool Confirm(Window owner, string text, string caption)
		{
			bool dummy = false;
			return Confirm(owner, text, caption, ref dummy);
		}

		public static bool Confirm(Window owner, string text, string caption, ref bool dontAskAgain)
		{
			var confirm = new ConfirmDialog(caption, text, dontAskAgain);
			confirm.Owner = owner;
			bool result = confirm.ShowDialog().Value;
			dontAskAgain = confirm.IsDontAskAgainChecked;
			return result;
		}

		public static void Alert(Window window, string text)
		{
			Interop.WindowHelper.FlashWindow(window);
			var chatWindow = window as ChatWindow;
			if (chatWindow != null)
			{
				chatWindow.Alert(text);
			}
		}

		public static void BrowseTo(string url)
		{
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error running browser process: " + ex.Message);
            }
		}

		public static bool Create(IrcSession session, IrcTarget target, bool makeActive)
		{
			var detached = App.Current.Windows.OfType<ChannelWindow>().Where((cw) => cw.Page.Session == session
				&& target.Equals(cw.Page.Target)).FirstOrDefault();
			if (detached != null)
			{
				if (makeActive)
				{
					detached.Activate();
				}
				return false;
			}

			var window = App.Current.MainWindow as ChatWindow;

			var page = window.FindPage(session, target);
			if (page != null)
			{
				if (makeActive)
				{
					window.Show();
					if (window.WindowState == WindowState.Minimized)
					{
						window.WindowState = WindowState.Normal;
					}
					window.Activate();
					window.SwitchToPage(page);
				}
				return false;
			}
			else
			{
				page = new ChatControl(session, target);
				if (App.Settings.Current.Windows.States.Exists(page.Id) ? 
					App.Settings.Current.Windows.States[page.Id].IsDetached : App.Settings.Current.Windows.DefaultQueryDetached)
				{
					var newWin = new ChannelWindow(page);
					if (!makeActive)
					{
						newWin.ShowActivated = false;
						newWin.WindowState = WindowState.Minimized;
					}
					newWin.Show();

					if (makeActive)
					{
						newWin.Activate();
					}
					else
					{
						Interop.WindowHelper.FlashWindow(newWin);
					}
				}
				else
				{
					window.AddPage(page, makeActive);
					if (!window.IsActive)
					{
						Interop.WindowHelper.FlashWindow(window);
					}
				}
				return true;
			}
		}

		public App()
		{
			this.Startup += new StartupEventHandler(App_Startup);
			this.Exit += new ExitEventHandler(App_Exit);
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
				{
					LogUnhandledException(e.ExceptionObject);
				};
		}

		private void App_Startup(object sender, StartupEventArgs e)
		{
			var window = new ChatWindow();
			window.Closed += new EventHandler(window_Closed);
			window.Show();
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
