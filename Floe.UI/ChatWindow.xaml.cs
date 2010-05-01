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
using System.Collections.ObjectModel;

using Floe.Net;

namespace Floe.UI
{
	public partial class ChatWindow : Window
	{
		public class ChatTabItem
		{
			public ChatControl Content { get; private set; }

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
		}

		public void AddPage(ChatContext context)
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
					if (this.Items[i].Content.Context.Session == context.Session)
					{
						this.Items.Insert(i + 1, item);
						break;
					}
				}
			}
			tabsChat.SelectedItem = item;
			Keyboard.Focus(this.CurrentControl);
		}

		public void RemovePage(ChatContext context)
		{
			var item = (from p in this.Items where p.Content.Context == context select p).FirstOrDefault();
			if (item != null)
			{
				if (context.Target == null)
				{
					this.UnsubscribeEvents(context.Session);
				}
				item.Content.Dispose();
				this.Items.Remove(item);
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

			foreach (var page in this.Items.Where((i) => i.Content.Context.Target == null).Select((i) => i.Content))
			{
				if (page.Context.IsConnected)
				{
					page.Context.Session.Quit("Leaving");
				}
			}

			Interop.WindowPlacementHelper.Save(this);
		}

		private ChatControl FindPage(IrcSession session, IrcTarget target)
		{
			return this.Items.Where((i) => i.Content.Context.Session == session && i.Content.Context.Target != null &&
				i.Content.Context.Target.Equals(target)).Select((p) => p.Content).FirstOrDefault();
		}
	}
}
