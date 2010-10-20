using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Floe.Configuration;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatWindow : Window
	{
		public class ChatTabItem : TabItem
		{
			public ChatControl Control { get { return this.Content as ChatControl; } }

			public ChatTabItem(ChatControl content)
			{
				this.Content = content;
			}
		}

		public ObservableCollection<ChatTabItem> Items { get; private set; }
		public ChatControl ActiveControl { get { return tabsChat.SelectedContent as ChatControl; } }

		public ChatWindow()
		{
			this.Items = new ObservableCollection<ChatTabItem>();
			this.DataContext = this;
			InitializeComponent();

			this.Loaded += new RoutedEventHandler(ChatWindow_Loaded);
		}

		public void AddPage(ChatContext context, bool switchToPage)
		{
			var page = new ChatControl(context);
			var item = new ChatTabItem(page);

			this.SetBindings(page);

			if (context.Target == null)
			{
				this.Items.Add(item);
				this.SubscribeEvents(context.Session);
			}
			else
			{
				for (int i = this.Items.Count - 1; i >= 0; --i)
				{
					if (this.Items[i].Control.Context.Session == context.Session)
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

		public void RemovePage(ChatContext context)
		{
			var item = (from p in this.Items where p.Control.Context == context select p).FirstOrDefault();
			if (item != null)
			{
				if (context.Target == null)
				{
					this.UnsubscribeEvents(context.Session);
				}
				item.Control.Dispose();
				this.Items.Remove(item);
			}
		}

		public void SwitchToPage(ChatContext context)
		{
			var item = (from p in this.Items where p.Control.Context == context select p).FirstOrDefault();
			if (item != null)
			{
				tabsChat.SelectedIndex = this.Items.IndexOf(item);
			}
		}

		public ChatContext FindPage(IrcSession session, IrcTarget target)
		{
			return this.Items.Where((i) => i.Control.Context.Session == session && i.Control.Context.Target != null &&
				i.Control.Context.Target.Equals(target)).Select((p) => p.Control.Context).FirstOrDefault();
		}

		public void Attach(ChatControl control)
		{
			for (int i = this.Items.Count - 1; i >= 0; --i)
			{
				if (this.Items[i].Control.Context.Session == control.Session)
				{
					this.Items.Insert(++i, new ChatTabItem(control));
					tabsChat.SelectedIndex = i;
					break;
				}
			}

			this.SetBindings(control);
		}

		private void ChatWindow_Loaded(object sender, RoutedEventArgs e)
		{
			this.AddPage(new ChatContext(new IrcSession(), null), true);

			if (Application.Current.MainWindow == this)
			{
				if (App.Settings.IsFirstLaunch)
				{
					App.ShowSettings();
				}

				int i = 0;
				foreach (var server in from ServerElement s in App.Settings.Current.Servers
									   where s.ConnectOnStartup == true
									   select s)
				{
					if (i++ > 0)
					{
						this.AddPage(new ChatContext(new IrcSession(), null), false);
					}
					this.Items[this.Items.Count - 1].Control.Connect(server);
				}
			}
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);

			if (!_isShuttingDown && this.Items.Any((i) => i.Control.IsConnected))
			{
				if (!this.Confirm("Are you sure you want to exit?", "Confirm Exit"))
				{
					e.Cancel = true;
					return;
				}
			}

			this.QuitAllSessions();

			foreach (var page in this.Items)
			{
				page.Control.Dispose();
			}

			foreach (var win in App.Current.Windows)
			{
				var channelWindow = win as ChannelWindow;
				if (channelWindow != null)
				{
					channelWindow.Close();
				}
			}

			App.Settings.Current.Windows.Placement = Interop.WindowHelper.Save(this);

			if (_notifyIcon != null)
			{
				_notifyIcon.Dispose();
			}
		}

		private void QuitAllSessions()
		{
			foreach (var page in this.Items.Where((i) => i.Control.Context.Target == null).Select((i) => i.Control))
			{
				if (page.IsConnected)
				{
					page.Context.Session.AutoReconnect = false;
					page.Context.Session.Quit("Leaving");
				}
			}
		}

		private void SetBindings(ChatControl control)
		{
			var bgBinding = new Binding();
			bgBinding.Source = this;
			bgBinding.Path = new PropertyPath("UIBackground");
			control.SetBinding(ChatControl.UIBackgroundProperty, bgBinding);
		}

		private void NotifyQuit_Click(object sender, RoutedEventArgs e)
		{
			this.ExecuteClose(sender, null);
		}
	}
}
