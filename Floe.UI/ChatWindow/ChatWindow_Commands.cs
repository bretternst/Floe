using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatWindow : Window
	{
		public readonly static RoutedUICommand ChatCommand = new RoutedUICommand("Chat", "Chat", typeof(ChatWindow));
		public readonly static RoutedUICommand CloseTabCommand = new RoutedUICommand("Close", "CloseTab", typeof(ChatWindow));
		public readonly static RoutedUICommand NewTabCommand = new RoutedUICommand("New Server Tab", "NewTab", typeof(ChatWindow));
		public readonly static RoutedUICommand DetachCommand = new RoutedUICommand("Detach", "Detach", typeof(ChatWindow));
		public readonly static RoutedUICommand PreviousTabCommand = new RoutedUICommand("Previous Tab", "PreviousTab", typeof(ChatWindow));
		public readonly static RoutedUICommand NextTabCommand = new RoutedUICommand("Next Tab", "NextTab", typeof(ChatWindow));
		public readonly static RoutedUICommand SettingsCommand = new RoutedUICommand("Settings", "Settings", typeof(ChatWindow));
		public readonly static RoutedUICommand MinimizeCommand = new RoutedUICommand("Minimize", "Minimize", typeof(ChatWindow));
		public readonly static RoutedUICommand MaximizeCommand = new RoutedUICommand("Maximize", "Maximize", typeof(ChatWindow));
		public readonly static RoutedUICommand CloseCommand = new RoutedUICommand("Quit", "Close", typeof(ChatWindow));

		private void ExecuteChat(object sender, ExecutedRoutedEventArgs e)
		{
			var control = tabsChat.SelectedContent as ChatControl;
			App.Create(control.Session, new IrcTarget((string)e.Parameter), true);
		}

		private void ExecuteCloseTab(object sender, ExecutedRoutedEventArgs e)
		{
			var page = e.Parameter as ChatPage;
			if (page != null)
			{
				if (page.Target == null)
				{
					if (page.Session.State == IrcSessionState.Disconnected || 
						App.Settings.Current.Windows.SuppressWarningOnQuit ||
						this.ConfirmQuit(string.Format("Are you sure you want to disconnect from {0}?", page.Session.NetworkName),
						"Close Server Tab"))
					{
						if(page.Session.State != IrcSessionState.Disconnected)
						{
							page.Session.Quit("Leaving");
						}
						var itemsToRemove = (from i in this.Items
											 where i.Page.Session == page.Session
											 select i.Page).ToArray();
						foreach(var p in itemsToRemove)
						{
							this.RemovePage(p);
						}
					}
				}
				else
				{
					if(page.Target.IsChannel && page.Session.State != IrcSessionState.Disconnected)
					{
						page.Session.Part(page.Target.Name);
					}
					this.RemovePage(page);
				}
			}
		}

		private void ExecuteNewTab(object sender, ExecutedRoutedEventArgs e)
		{
			this.AddPage(new ChatControl(new IrcSession((a) => this.Dispatcher.BeginInvoke(a)), null), true);
		}

		private void ExecuteDetach(object sender, ExecutedRoutedEventArgs e)
		{
			var item = e.Parameter as ChatTabItem;
			if (item != null && item.Page.Type != ChatPageType.Server)
			{
				this.Items.Remove(item);
				var ctrl = item.Content;
				item.Content = null;
				var window = new ChannelWindow(item.Page);
				window.Show();
			}
		}

		private void CanExecuteCloseTab(object sender, CanExecuteRoutedEventArgs e)
		{
			var page = e.Parameter as ChatPage;
			if (page != null)
			{
				if (page.Target == null)
				{
					e.CanExecute = this.Items.Count((i) => i.Page.Type == ChatPageType.Server) > 1;
				}
				else
				{
					e.CanExecute = true;
				}
			}
		}

		private void ExecutePreviousTab(object sender, ExecutedRoutedEventArgs e)
		{
			tabsChat.SelectedIndex--;
		}

		private void ExecuteNextTab(object sender, ExecutedRoutedEventArgs e)
		{
			tabsChat.SelectedIndex++;
		}

		private void CanExecutePreviousTab(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = tabsChat.SelectedIndex > 0;
		}

		private void CanExecuteNextTab(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = tabsChat.SelectedIndex < tabsChat.Items.Count - 1;
		}

		private void ExecuteSettings(object sender, ExecutedRoutedEventArgs e)
		{
			App.ShowSettings();
		}

		private void ExecuteMinimize(object sender, ExecutedRoutedEventArgs e)
		{
			_oldWindowState = this.WindowState;
			this.WindowState = WindowState.Minimized;
		}

		private void ExecuteMaximize(object sender, ExecutedRoutedEventArgs e)
		{
			if (this.WindowState == WindowState.Maximized)
			{
				this.WindowState = WindowState.Normal;
			}
			else
			{
				this.WindowState = WindowState.Maximized;
			}
		}

		private void ExecuteClose(object sender, ExecutedRoutedEventArgs e)
		{
			this.Close();
		}
	}
}
