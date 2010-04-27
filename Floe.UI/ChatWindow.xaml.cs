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
		public ObservableCollection<ChatPageInfo> Pages { get; private set; }

		public ChatWindow(IrcSession session)
		{
			this.Pages = new ObservableCollection<ChatPageInfo>();
			this.DataContext = this;
			InitializeComponent();
			this.AddPage(new ChatContext(session, null));
		}

		public ChatControl CurrentControl
		{
			get
			{
				return tabsChat.SelectedContent as ChatControl;
			}
		}

		public void AddPage(ChatContext context)
		{
			var page = new ChatPageInfo(context);

			if (context.Target == null)
			{
				this.Pages.Add(page);
				context.Session.Joined += new EventHandler<IrcChannelEventArgs>(Session_Joined);
				context.Session.Parted += new EventHandler<IrcChannelEventArgs>(Session_Parted);
				context.Session.Kicked += new EventHandler<IrcKickEventArgs>(Session_Kicked);
				context.Session.StateChanged += new EventHandler<EventArgs>(Session_StateChanged);
			}
			else
			{
				for (int i = this.Pages.Count - 1; i >= 0; --i)
				{
					if (this.Pages[i].Context.Session == context.Session)
					{
						this.Pages.Insert(i + 1, page);
						break;
					}
				}
			}
			tabsChat.SelectedItem = page;
			Keyboard.Focus(this.CurrentControl);
		}

		public void RemovePage(ChatPageInfo page)
		{
			page.Content.Dispose();
			this.Pages.Remove(page);
		}

		private ChatPageInfo FindPage(IrcSession session, IrcTarget target)
		{
			return this.Pages.Where((p) => p.Context.Session == session && p.Context.Target != null &&
				p.Context.Target.Equals(target)).FirstOrDefault();
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			Interop.WindowPlacementHelper.Load(this);
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);

			foreach (var page in this.Pages.Where((p) => p.Context.Target == null))
			{
				if (page.Context.IsConnected)
				{
					page.Context.Session.Quit("Leaving");
				}
			}

			Interop.WindowPlacementHelper.Save(this);
		}

		private void Session_Joined(object sender, IrcChannelEventArgs e)
		{
			if (e.IsSelf)
			{
				this.Dispatcher.BeginInvoke((Action)(() =>
				{
					this.AddPage(new ChatContext((IrcSession)sender, e.Channel));
				}));
			}
		}

		private void Session_Parted(object sender, IrcChannelEventArgs e)
		{
			if (e.IsSelf)
			{
				this.Dispatcher.BeginInvoke((Action)(() =>
				{
					var page = this.FindPage((IrcSession)sender, e.Channel);
					if (page != null)
					{
						this.RemovePage(page);
					}
				}));
			}
		}

		private void Session_Kicked(object sender, IrcKickEventArgs e)
		{
			if (e.IsSelfKicked)
			{
				this.Dispatcher.BeginInvoke((Action)(() =>
					{
						var page = this.FindPage((IrcSession)sender, e.Channel);
						if (page != null)
						{
							this.RemovePage(page);
						}
					}));
			}
		}

		private void Session_StateChanged(object sender, EventArgs e)
		{
			if (((IrcSession)sender).State == IrcSessionState.Connecting)
			{
				foreach (var p in (from p in this.Pages
								  where p.Context.Session == sender && p.Context.Target != null
								  select p).ToArray())
				{
					this.RemovePage(p);
				}
			}
		}
	}

	public class ChatPageInfo
	{
		public ChatContext Context { get; private set; }
		public string Header { get; private set; }
		public ChatControl Content { get; private set; }

		public ChatPageInfo(ChatContext context)
		{
			this.Context = context;
			this.Header = context.ToString();
			this.Content = new ChatControl(context);
		}
	}
}
