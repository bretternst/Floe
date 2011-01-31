using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Floe.Net;

namespace Floe.UI
{
	public enum ChatPageType
	{
		Server,
		Chat,
		DccFile
	}

	public class ChatPage : UserControl, IDisposable
	{
		public IrcSession Session { get; protected set; }
		public IrcTarget Target { get; protected set; }
		public ChatPageType Type { get; protected set; }
		public string Id { get; protected set; }

		public virtual void Dispose() { }

		public readonly static DependencyProperty UIBackgroundProperty = DependencyProperty.Register("UIBackground",
			typeof(SolidColorBrush), typeof(ChatControl));
		public SolidColorBrush UIBackground
		{
			get { return (SolidColorBrush)this.GetValue(UIBackgroundProperty); }
			set { this.SetValue(UIBackgroundProperty, value); }
		}

		public static readonly DependencyProperty HeaderProperty =
			DependencyProperty.Register("Header", typeof(string), typeof(ChatControl));
		public string Header
		{
			get { return (string)this.GetValue(HeaderProperty); }
			set { this.SetValue(HeaderProperty, value); }
		}

		public static readonly DependencyProperty TitleProperty =
			DependencyProperty.Register("Title", typeof(string), typeof(ChatControl));
		public string Title
		{
			get { return (string)this.GetValue(TitleProperty); }
			set { this.SetValue(TitleProperty, value); }
		}

		public static readonly DependencyProperty NotifyStateProperty =
			DependencyProperty.Register("NotifyState", typeof(NotifyState), typeof(ChatControl));
		public NotifyState NotifyState
		{
			get { return (NotifyState)this.GetValue(NotifyStateProperty); }
			set { this.SetValue(NotifyStateProperty, value); }
		}

		public ChatPage()
		{
			this.Header = this.Title = "";
		}

		public ChatPage(ChatPageType type, IrcSession session, IrcTarget target, string id)
			: this()
		{
			this.Type = type;
			this.Session = session;
			this.Target = target;
			this.Id = id;
		}
	}
}
