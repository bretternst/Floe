using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Floe.Net;

namespace Floe.UI
{
	public partial class ChannelWindow : Window
	{
		public ChatPage Page { get; private set; }

		public IrcSession Session { get { return this.Page.Session; } }

		public ChannelWindow(ChatPage page)
		{
			InitializeComponent();
			this.DataContext = this;

			this.Page = page;
			page.SetValue(Grid.RowProperty, 1);
			page.SetValue(Grid.ColumnSpanProperty, 2);
			grdRoot.Children.Add((Control)page);
			page.Session.StateChanged += new EventHandler<EventArgs>(Session_StateChanged);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			this.Page.Session.StateChanged -= new EventHandler<EventArgs>(Session_StateChanged);

			var state = App.Settings.Current.Windows.States[this.Page.Id];

			if (this.Page.Parent != null)
			{
				if (this.Page.Session.State == IrcSessionState.Connected && this.Page.Target.IsChannel)
				{
					this.Page.Session.Part(this.Page.Target.Name);
				}
				state.IsDetached = true;
				this.Page.Dispose();
			}
			else
			{
				state.IsDetached = false;
				var window = App.Current.MainWindow as ChatWindow;
				if (window != null)
				{
					window.Attach(this.Page);
				}
			}
			state.Placement = Interop.WindowHelper.Save(this);
		}

		protected override void OnActivated(EventArgs e)
		{
			this.Opacity = App.Settings.Current.Windows.ActiveOpacity;

			base.OnActivated(e);
		}

		protected override void OnDeactivated(EventArgs e)
		{
			if (this.OwnedWindows.Count == 0)
			{
				this.Opacity = App.Settings.Current.Windows.InactiveOpacity;
			}

			base.OnDeactivated(e);
		}

		private void Session_StateChanged(object sender, EventArgs e)
		{
			if (((IrcSession)sender).State == IrcSessionState.Connecting)
			{
				this.Close();
			}
		}
	}
}
