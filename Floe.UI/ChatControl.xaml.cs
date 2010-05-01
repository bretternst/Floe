using System;
using System.Collections.Generic;
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
			this.Context = context;
			this.Header = context.Target == null ? "Server" : context.Target.ToString();
			this.Loaded += (sender, e) =>
			{
				Keyboard.Focus(txtInput);
			};
			InitializeComponent();
			this.SubscribeEvents();
		}

		public ChatContext Context { get; private set; }

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

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			if (this.Context.Target != null && this.Context.Target.Type == IrcTargetType.Channel)
			{
				this.Write("Join", string.Format("* Now talking on {0}", this.Context.Target.Name));
			}
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ||
				Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
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
							txtInput.Text = _historyNode.Value;
							txtInput.SelectionStart = txtInput.Text.Length;
						}
					}
					else if (_history.First != null)
					{
						_historyNode = _history.First;
						txtInput.Text = _historyNode.Value;
						txtInput.SelectionStart = txtInput.Text.Length;
					}
					break;
				case Key.Down:
					if (_historyNode != null)
					{
						_historyNode = _historyNode.Previous;
						if (_historyNode != null)
						{
							txtInput.Text = _historyNode.Value;
							txtInput.SelectionStart = txtInput.Text.Length;
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

		private void txtInput_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Enter:
					string text = txtInput.Text;
					txtInput.Clear();
					_history.AddFirst(text);
					while(_history.Count > App.Settings.Current.Buffer.InputHistory)
					{
						_history.RemoveLast();
					}
					_historyNode = null;
					this.ParseInput(text);
					break;
			}
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
			var line = new ChatLine(styleKey, text);

			Dispatcher.BeginInvoke((Action)(() =>
			{
				boxOutput.AppendLine(line);
			}));
		}

		public void Dispose()
		{
			this.UnsubscribeEvents();
		}
	}
}
