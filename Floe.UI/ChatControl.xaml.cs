using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatControl : UserControl, IDisposable
	{
		private LinkedList<string> _history;
		private LinkedListNode<string> _historyNode;

		public ChatControl(ChatContext context)
		{
			_history = new LinkedList<string>();
			this.Nicknames = new ObservableCollection<NicknameItem>();
			this.Context = context;
			this.Header = context.Target == null ? "Server" : context.Target.ToString();

			InitializeComponent();
			this.SubscribeEvents();

			if (this.IsChannel)
			{
				this.Session.Mode(this.Target);
				splitter.IsEnabled = true;
				colNickList.Width = new GridLength(App.Settings.Current.Windows.NickListWidth);
			}
			else if (this.IsNickname)
			{
				_prefix = this.Target.Name;
			}
			else if (this.IsServer)
			{
				this.IsDefault = true;
			}
		}

		public ChatContext Context { get; private set; }
		public IrcSession Session { get { return this.Context.Session; } }
		public IrcTarget Target { get { return this.Context.Target; } }
		public bool IsServer { get { return this.Target == null; } }
		public bool IsChannel { get { return this.Target != null && this.Target.Type == IrcTargetType.Channel; } }
		public bool IsNickname { get { return this.Target != null && this.Target.Type == IrcTargetType.Nickname; } }

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

		public static readonly DependencyProperty IsDefaultProperty =
			DependencyProperty.Register("IsDefault", typeof(bool), typeof(ChatControl));
		public bool IsDefault
		{
			get { return (bool)this.GetValue(IsDefaultProperty); }
			set { this.SetValue(IsDefaultProperty, value); }
		}

		public void Connect(string hostname, int port)
		{
			this.Session.Open(hostname, port,
				!string.IsNullOrEmpty(this.Session.Nickname) ?
					this.Session.Nickname : App.Settings.Current.User.Nickname,
				App.Settings.Current.User.Username,
				App.Settings.Current.User.Hostname,
				App.Settings.Current.User.FullName);
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			if (this.IsChannel)
			{
				this.Write("Join", string.Format("* Now talking on {0}", this.Target.Name));
			}
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
			{
				return;
			}

			e.Handled = true;

			switch (e.Key)
			{
				case Key.PageUp:
					boxOutput.PageUp();
					break;
				case Key.PageDown:
					boxOutput.PageDown();
					break;
				case Key.Up:
					if (_historyNode != null)
					{
						if (_historyNode.Next != null)
						{
							_historyNode = _historyNode.Next;
							this.SetInputText(_historyNode.Value);
						}
					}
					else if (_history.First != null)
					{
						_historyNode = _history.First;
						this.SetInputText(_historyNode.Value);
					}
					break;
				case Key.Down:
					if (_historyNode != null)
					{
						_historyNode = _historyNode.Previous;
						if (_historyNode != null)
						{
							this.SetInputText(_historyNode.Value);
						}
						else
						{
							txtInput.Clear();
						}
					}
					break;
				default:
					Keyboard.Focus(txtInput);
					e.Handled = false;
					break;
			}

			base.OnPreviewKeyDown(e);
		}

		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			if (e.Delta > 0)
			{
				boxOutput.MouseWheelUp();
			}
			else
			{
				boxOutput.MouseWheelDown();
			}
			e.Handled = true;

			base.OnPreviewMouseWheel(e);
		}

		private void ParseInput(string text)
		{
			try
			{
				this.Execute(text);
			}
			catch (IrcException ex)
			{
				this.Write("Error", ex.Message);
			}
		}

		private void Write(string styleKey, string text)
		{
			boxOutput.AppendLine(new ChatLine(styleKey, text));
		}

		private void SetInputText(string text)
		{
			txtInput.Text = text;
			txtInput.SelectionStart = text.Length;
		}

		private void SetTitle()
		{
			string userModes = this.Session.UserModes.Length > 0 ?
				string.Format("+{0}", string.Join("", this.Session.UserModes)) : "";
			string channelModes = _channelModes.Length > 0 ?
				string.Format("+{0}", string.Join("", _channelModes)) : "";

			if(this.IsServer)
			{
				if (this.Session.State == IrcSessionState.Disconnected)
				{
					this.Title = string.Format("{0} - Not Connected", App.Product);
				}
				else
				{
					this.Title = string.Format("{0} - {1} ({2}) on {3}", App.Product, this.Session.Nickname,
						userModes, this.Session.NetworkName);
				}
			}
			else if (this.Target.Type == IrcTargetType.Nickname)
			{
				this.Title = string.Format("{0} - {1} ({2}) on {3} - {4}", App.Product, this.Session.Nickname,
					userModes, this.Session.NetworkName, _prefix);
			}
			else if (this.Target.Type == IrcTargetType.Channel)
			{
				this.Title = string.Format("{0} - {1} ({2}) on {3} - {4} ({5}) - {6}", App.Product, this.Session.Nickname,
					userModes, this.Session.NetworkName, this.Target.ToString(), channelModes, _topic);
			}
		}

		private void SubmitInput()
		{
			string text = txtInput.Text;
			txtInput.Clear();
			if (_history.Count == 0 || _history.First.Value != text)
			{
				_history.AddFirst(text);
			}
			while (_history.Count > App.Settings.Current.Buffer.InputHistory)
			{
				_history.RemoveLast();
			}
			_historyNode = null;
			this.ParseInput(text);
		}

		public void Dispose()
		{
			this.UnsubscribeEvents();

			if (this.IsChannel)
			{
				App.Settings.Current.Windows.NickListWidth = colNickList.Width.Value;
			}
		}
	}
}
