using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Floe.Net;

namespace Floe.UI
{
	public partial class ListControl : ChatPage
	{
		public class ChannelItem : IComparable<ChannelItem>
		{
			public string Name { get; private set; }
			public int Count { get; private set; }
			public string Topic { get; private set; }

			public ChannelItem(string name, int count, string topic)
			{
				this.Name = name;
				this.Count = count;
				this.Topic = topic;
			}

			public int CompareTo(ChannelItem other)
			{
				return string.Compare(this.Name, other.Name);
			}
		}

		public static readonly DependencyProperty CountProperty =
			DependencyProperty.Register("Count", typeof(int), typeof(ListControl));
		public int Count
		{
			get { return (int)this.GetValue(CountProperty); }
			set { this.SetValue(CountProperty, value); }
		}

		private List<ChannelItem> _channels;

		public ListControl(IrcSession session)
			: base(ChatPageType.ChannelList, session, null, "chan-list")
		{
			_channels = new List<ChannelItem>();
			InitializeComponent();
			this.Header = "Channel List";
			this.Title = string.Format("{0} - {1} - {2} Channel List", App.Product, this.Session.Nickname, this.Session.NetworkName);
			this.IsCloseable = false;
			this.Session.InfoReceived += new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);

			var menu = this.Resources["cmChannels"] as ContextMenu;
			if (menu != null)
			{
				NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
			}
		}

		public void ExecuteJoin(object sender, ExecutedRoutedEventArgs e)
		{
			string channel = e.Parameter as string;
			if (!string.IsNullOrEmpty(channel))
			{
				this.Session.Join(channel);
			}
		}

		private void Session_InfoReceived(object sender, IrcInfoEventArgs e)
		{
			switch (e.Code)
			{
				case IrcCode.RPL_LIST:
					int count;
					if (e.Message.Parameters.Count == 4 &&
						int.TryParse(e.Message.Parameters[2], out count))
					{
						_channels.Add(new ChannelItem(e.Message.Parameters[1], count, e.Message.Parameters[3]));
						this.Count++;
					}
					break;
				case IrcCode.RPL_LISTEND:
					this.IsCloseable = true;
					this.Session.InfoReceived -= new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
					_channels.Sort();
					foreach (var c in _channels)
					{
						lstChannels.Items.Add(c);
					}
					break;
			}
		}

		private void lstChannels_MouseDoubleClick(object sender, RoutedEventArgs e)
		{
			var chanItem = ((ListBoxItem)e.Source).Content as ChannelItem;
			if (chanItem != null)
			{
				ChatControl.JoinCommand.Execute(chanItem.Name, this);
			}
		}
	}
}
