using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Floe.Net;

namespace Floe.UI
{
	public enum ChatPageType
	{
		Server,
		Chat,
		DccFile,
		DccChat,
		ChannelList
	}

	public class ChatPage : UserControl, IDisposable
	{
		public IrcSession Session { get; protected set; }
		public IrcTarget Target { get; protected set; }
		public ChatPageType Type { get; protected set; }
		public string Id { get; protected set; }
		public bool IsServer { get { return this.Type == ChatPageType.Server; } }

		public virtual void Dispose() { }

		public readonly static DependencyProperty UIBackgroundProperty = DependencyProperty.Register("UIBackground",
			typeof(SolidColorBrush), typeof(ChatPage));
		public SolidColorBrush UIBackground
		{
			get { return (SolidColorBrush)this.GetValue(UIBackgroundProperty); }
			set { this.SetValue(UIBackgroundProperty, value); }
		}

		public static readonly DependencyProperty HeaderProperty =
			DependencyProperty.Register("Header", typeof(string), typeof(ChatPage));
		public string Header
		{
			get { return (string)this.GetValue(HeaderProperty); }
			set { this.SetValue(HeaderProperty, value); }
		}

		public static readonly DependencyProperty TitleProperty =
			DependencyProperty.Register("Title", typeof(string), typeof(ChatPage));
		public string Title
		{
			get { return (string)this.GetValue(TitleProperty); }
			set { this.SetValue(TitleProperty, value); }
		}

		public static readonly DependencyProperty ProgressProperty =
			DependencyProperty.Register("Progress", typeof(double), typeof(ChatPage));
		public double Progress
		{
			get { return (double)this.GetValue(ProgressProperty); }
			set { this.SetValue(ProgressProperty, value); }
		}

		public static readonly DependencyProperty NotifyStateProperty =
			DependencyProperty.Register("NotifyState", typeof(NotifyState), typeof(ChatPage));
		public NotifyState NotifyState
		{
			get { return (NotifyState)this.GetValue(NotifyStateProperty); }
			set { this.SetValue(NotifyStateProperty, value); }
		}

		public static readonly DependencyProperty IsCloseableProperty =
			DependencyProperty.Register("IsCloseable", typeof(bool), typeof(ChatPage));
		public bool IsCloseable
		{
			get { return (bool)this.GetValue(IsCloseableProperty); }
			set { this.SetValue(IsCloseableProperty, value); System.Windows.Input.CommandManager.InvalidateRequerySuggested(); }
		}

		public ChatPage()
		{
			this.Header = this.Title = "";
			this.Loaded += new RoutedEventHandler(ChatPage_Loaded);
		}

		public ChatPage(ChatPageType type, IrcSession session, IrcTarget target, string id)
			: this()
		{
			this.Type = type;
			this.Session = session;
			this.Target = target;
			this.Id = id;
			this.IsCloseable = true;
		}

		public virtual bool CanClose()
		{
			return true;
		}

		protected override void OnVisualParentChanged(DependencyObject oldParent)
		{
			base.OnVisualParentChanged(oldParent);

			var bgBinding = new Binding();
			bgBinding.Source = Window.GetWindow(this);
			bgBinding.Path = new PropertyPath("UIBackground");
			bgBinding.Mode = BindingMode.OneWay;
			this.SetBinding(ChatPage.UIBackgroundProperty, bgBinding);
		}

		private void ChatPage_Loaded(object sender, RoutedEventArgs e)
		{
			if (this.IsVisible)
			{
				this.NotifyState = NotifyState.None;
			}
		}
	}
}
