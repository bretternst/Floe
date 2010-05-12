using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;
using System.Collections.ObjectModel;

using Floe.Net;

namespace Floe.UI
{
	public partial class ChatWindow : Window
	{
		public readonly static RoutedUICommand ChatCommand = new RoutedUICommand("Chat", "Chat", typeof(ChatWindow));
		public readonly static RoutedUICommand CloseCommand = new RoutedUICommand("Close", "Close", typeof(ChatWindow));
		public readonly static RoutedUICommand NewTabCommand = new RoutedUICommand("New Server Tab", "NewTab", typeof(ChatWindow));

		private void ExecuteChat(object sender, ExecutedRoutedEventArgs e)
		{
			var control = tabsChat.SelectedContent as ChatControl;
			if (control != null)
			{
				var target = new IrcTarget((string)e.Parameter);
				var context = this.FindPage(control.Session, target);
				this.BeginInvoke(() =>
				{
					if (context != null)
					{
						this.SwitchToPage(context);
					}
					else
					{
						this.AddPage(new ChatContext(control.Session, target), true);
					}
				});
			}
		}

		private void ExecuteClose(object sender, ExecutedRoutedEventArgs e)
		{
			var context = e.Parameter as ChatContext;
			if (context != null)
			{
				if (context.Target == null)
				{
					if (context.Session.State == IrcSessionState.Disconnected || 
						this.Confirm(string.Format("Are you sure you want to disconnect from {0}?", context.Session.NetworkName),
						"Close Server Tab"))
					{
						if(context.Session.State != IrcSessionState.Disconnected)
						{
							context.Session.Quit("Leaving");
						}
						var itemsToRemove = (from i in this.Items
											 where i.Control.Session == context.Session
											 select i.Control.Context).ToArray();
						foreach(var item in itemsToRemove)
						{
							this.RemovePage(item);
						}
					}
				}
				else
				{
					if(context.Target.Type == IrcTargetType.Channel && context.Session.State != IrcSessionState.Disconnected)
					{
						context.Session.Part(context.Target.Name);
					}
					this.RemovePage(context);
				}
			}
		}

		private void ExecuteNewTab(object sender, ExecutedRoutedEventArgs e)
		{
			this.AddPage(new ChatContext(new IrcSession(), null), true);
		}

		private void CanExecuteClose(object sender, CanExecuteRoutedEventArgs e)
		{
			var context = e.Parameter as ChatContext;
			if (context != null)
			{
				if (context.Target == null)
				{
					e.CanExecute = this.Items.Count((i) => i.Control.IsServer) > 1;
				}
				else
				{
					e.CanExecute = true;
				}
			}
		}
	}
}
