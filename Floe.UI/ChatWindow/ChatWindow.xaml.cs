using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Collections.ObjectModel;

using Floe.Net;
using Floe.Configuration;

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
		public ChatControl CurrentControl { get { return tabsChat.SelectedContent as ChatControl; } }

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
				tabsChat.SelectedItem = item;
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

		private void ChatWindow_Loaded(object sender, RoutedEventArgs e)
		{
			var hwndSrc = PresentationSource.FromVisual(this) as HwndSource;
			hwndSrc.AddHook(new HwndSourceHook(WndProc));

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

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			Interop.WindowPlacementHelper.Load(this);
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);

			if (this.Items.Any((i) => i.Control.IsConnected))
			{
				if (this.Confirm("Are you sure you want to exit?", "Confirm Exit"))
				{
					e.Cancel = true;
					return;
				}
			}

			foreach (var page in this.Items.Where((i) => i.Control.Context.Target == null).Select((i) => i.Control))
			{
				if (page.IsConnected)
				{
					page.Context.Session.AutoReconnect = false;
					page.Context.Session.Quit("Leaving");
				}
			}

			foreach (var page in this.Items)
			{
				page.Control.Dispose();
			}

			Interop.WindowPlacementHelper.Save(this);

			if (_notifyIcon != null)
			{
				_notifyIcon.Dispose();
			}
		}
	}
}
