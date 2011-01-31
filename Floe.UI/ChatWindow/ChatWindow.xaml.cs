using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Floe.Net;

namespace Floe.UI
{
	public partial class ChatWindow : Window
	{
		public ObservableCollection<ChatTabItem> Items { get; private set; }
		public ChatControl ActiveControl { get { return tabsChat.SelectedContent as ChatControl; } }

		public ChatWindow()
		{
			this.Items = new ObservableCollection<ChatTabItem>();
			this.DataContext = this;
			InitializeComponent();

			this.Loaded += new RoutedEventHandler(ChatWindow_Loaded);
		}

		public void AddPage(ChatPage page, bool switchToPage)
		{
			var item = new ChatTabItem(page);

			if (page.Type == ChatPageType.Server || page.Type == ChatPageType.Chat)
			{
				this.SetBindings(page);
			}

			if (page.Type == ChatPageType.Server)
			{
				this.Items.Add(item);
				this.SubscribeEvents(page.Session);
			}
			else
			{
				for (int i = this.Items.Count - 1; i >= 0; --i)
				{
					if (this.Items[i].Page.Session == page.Session)
					{
						this.Items.Insert(i + 1, item);
						break;
					}
				}
			}
			if (switchToPage)
			{
				var oldItem = tabsChat.SelectedItem as TabItem;
				if (oldItem != null)
				{
					oldItem.IsSelected = false;
				}
				item.IsSelected = true;
			}
		}

		public void RemovePage(ChatPage page)
		{
			if (page.Type == ChatPageType.Server)
			{
				this.UnsubscribeEvents(page.Session);
			}
			page.Dispose();
			this.Items.Remove(this.Items.Where((i) => i.Page == page).FirstOrDefault());
		}

		public void SwitchToPage(ChatPage page)
		{
			var index = this.Items.Where((tab) => tab.Page == page).Select((t,i) => i).FirstOrDefault();
			tabsChat.SelectedIndex = index;
		}

		public ChatPage FindPage(IrcSession session, IrcTarget target)
		{
			return this.Items.Where((i) => i.Page.Session == session && i.Page.Target != null &&
				i.Page.Target.Equals(target)).Select((i) => i.Page).FirstOrDefault();
		}

		public void Attach(ChatPage page)
		{
			for (int i = this.Items.Count - 1; i >= 0; --i)
			{
				if (this.Items[i].Page.Session == page.Session)
				{
					this.Items.Insert(++i, new ChatTabItem(page));
					tabsChat.SelectedIndex = i;
					break;
				}
			}

			this.SetBindings(page);
			this.SwitchToPage(page);
		}

		public void Alert(string text)
		{
			if (_notifyIcon != null && _notifyIcon.IsVisible)
			{
				_notifyIcon.Show("IRC Alert", text);
			}
		}

		private void QuitAllSessions()
		{
			foreach (var i in this.Items.Where((i) => i.Page.Type == ChatPageType.Server).Select((i) => i))
			{
				if (i.Page.Session.State == IrcSessionState.Connected)
				{
					i.Page.Session.AutoReconnect = false;
					i.Page.Session.Quit("Leaving");
				}
			}
		}

		private void SetBindings(ChatPage control)
		{
			var bgBinding = new Binding();
			bgBinding.Source = this;
			bgBinding.Path = new PropertyPath("UIBackground");
			control.SetBinding(ChatPage.UIBackgroundProperty, bgBinding);
		}
	}
}
