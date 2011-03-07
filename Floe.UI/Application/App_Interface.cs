using System;
using System.Linq;
using System.Windows;
using Floe.Net;

namespace Floe.UI
{
	public partial class App : Application
	{
		public static ChatWindow ChatWindow
		{
			get
			{
				return App.Current.MainWindow as ChatWindow;
			}
		}

		public static ProxyInfo ProxyInfo
		{
			get
			{
				return App.Settings.Current.Network.UseSocks5Proxy ?
					new ProxyInfo(App.Settings.Current.Network.ProxyHostname,
						App.Settings.Current.Network.ProxyPort,
						App.Settings.Current.Network.ProxyUsername,
						App.Settings.Current.Network.ProxyPassword) : null;
			}
		}

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

		public static string OpenFileDialog(Window owner, string initialDirectory)
		{
			var dialog = new Microsoft.Win32.OpenFileDialog();
			dialog.CheckFileExists = true;
			dialog.Multiselect = false;
			dialog.InitialDirectory = initialDirectory;
			return dialog.ShowDialog(owner) == true ? dialog.FileName : null;
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

		public static void Create(IrcSession session, ChatPage page, bool makeActive)
		{
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
				var window = App.Current.MainWindow as ChatWindow;
				window.AddPage(page, makeActive);
				if (!window.IsActive)
				{
					Interop.WindowHelper.FlashWindow(window);
				}
			}
		}

		public static bool Create(IrcSession session, IrcTarget target, bool makeActive)
		{
			var detached = App.Current.Windows.OfType<ChannelWindow>().Where((cw) => cw.Page.Session == session
				&& target.Equals(cw.Page.Target) && cw.Page.Type == ChatPageType.Chat).FirstOrDefault();
			if (detached != null)
			{
				if (makeActive)
				{
					detached.Activate();
				}
				return false;
			}

			var window = App.ChatWindow;
			var page = window.FindPage(ChatPageType.Chat, session, target);
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
				page = new ChatControl(target == null ? ChatPageType.Server : ChatPageType.Chat, session, target);
				Create(session, page, makeActive);
				return true;
			}
		}

		public static void ClosePage(ChatPage page)
		{
			var window = Window.GetWindow(page);
			if (window is ChannelWindow)
			{
				window.Close();
			}
			else
			{
				App.ChatWindow.RemovePage(page);
			}
		}
	}
}
