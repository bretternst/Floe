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

		public ChatWindow()
		{
			this.Pages = new ObservableCollection<ChatPageInfo>();
			this.DataContext = this;
			InitializeComponent();
		}

		public void AddPage(ChatController context)
		{
			this.Pages.Add(new ChatPageInfo(context));
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			Interop.WindowPlacementHelper.Load(this);
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);

			foreach (var page in this.Pages)
			{
				page.Context.Close();
			}

			Interop.WindowPlacementHelper.Save(this);
		}
	}

	public class ChatPageInfo
	{
		public ChatController Context { get; private set; }
		public string Header { get; private set; }
		public ChatControl Content { get; private set; }

		public ChatPageInfo(ChatController context)
		{
			this.Context = context;
			this.Header = context.ToString();
			this.Content = new ChatControl(context);
		}
	}
}
