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
		public static PersistentSettings Settings { get; private set; }
		public static string Product { get; private set; }
		public static string HelpText { get; private set; }

		private static Lazy<ImageSource> appImage = new Lazy<ImageSource>(() =>
		{
			using (var stream = typeof(App).Assembly.GetManifestResourceStream(
				string.Format("{0}.Resources.App.ico", typeof(App).Namespace)))
			{
				return BitmapFrame.Create(stream);
			}
		});
		public static ImageSource ApplicationImage
		{
			get
			{
				return appImage.Value;
			}
		}

		private static Lazy<Icon> appIcon = new Lazy<Icon>(() =>
			{
				using (var stream = typeof(App).Assembly.GetManifestResourceStream(
					string.Format("{0}.Resources.App.ico", typeof(App).Namespace)))
				{
					return new Icon(stream);
				}
			});
		public static Icon ApplicationIcon
		{
			get
			{
				return appIcon.Value;
			}
		}

		public static string Version
		{
			get
			{
				return typeof(App).Assembly.GetName().Version.ToString();
			}
		}

		public static ChatControl ActiveControl
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
						return channelWindow.Control;
					}
				}

				return null;
			}
		}

		static App()
		{
			App.Product = typeof(App).Assembly.GetCustomAttributes(
					typeof(AssemblyProductAttribute), false).OfType<AssemblyProductAttribute>().FirstOrDefault().Product;

			try
			{
				App.Settings = new PersistentSettings(App.Product);
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(string.Format("Unable to load user configuration. You may want to delete the configuration file and try again.",
					ex.Message));
				Environment.Exit(-1);
			}

			App.RefreshAttentionPatterns();
			App.LoadIgnoreMasks();

			using (var sr = new System.IO.StreamReader(typeof(App).Assembly.GetManifestResourceStream(
				string.Format("{0}.Resources.Help.txt", typeof(App).Namespace))))
			{
				App.HelpText = sr.ReadToEnd();
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
			var detached = App.Current.Windows.OfType<ChannelWindow>().Where((cw) => cw.Control.Session == session
				&& target.Equals(cw.Control.Target)).FirstOrDefault();
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
				var context = new ChatContext(session, target);
				if (App.Settings.Current.Windows.States.Exists(context.Key) ? App.Settings.Current.Windows.States[context.Key].IsDetached : App.Settings.Current.Windows.DefaultQueryDetached)
				{
					var newWin = new ChannelWindow(new ChatControl(context));
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
					window.AddPage(new ChatContext(session, target), makeActive);
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
			this.Dispatcher.UnhandledException += (sender, e) =>
				{
					if (e.Exception is IrcException)
					{
						e.Handled = true;
					}
				};
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
