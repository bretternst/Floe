using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Linq;
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
			this.SubscribeEvents();
			this.Loaded += (sender, e) =>
			{
				Keyboard.Focus(txtInput);
				this.SetTitle();
			};

			InitializeComponent();

			DataObject.AddPastingHandler(txtInput, new DataObjectPastingEventHandler(txtInput_Pasting));

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
			var line = new ChatLine(styleKey, text);

			Dispatcher.BeginInvoke((Action)(() =>
			{
				boxOutput.AppendLine(line);
			}));
		}

		private void SetInputText(string text)
		{
			Dispatcher.BeginInvoke((Action)(() =>
				{
					txtInput.Text = text;
					txtInput.SelectionStart = text.Length;
				}));
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
					this.SetTitle(string.Format("{0} - Not Connected", App.Product));
				}
				else
				{
					this.SetTitle(string.Format("{0} - {1} ({2}) on {3}", App.Product, this.Session.Nickname,
						userModes, this.Session.NetworkName));
				}
			}
			else if (this.Target.Type == IrcTargetType.Nickname)
			{
				this.SetTitle(string.Format("{0} - {1} ({2}) on {3} - {4}", App.Product, this.Session.Nickname,
					userModes, this.Session.NetworkName, _prefix));
			}
			else if (this.Target.Type == IrcTargetType.Channel)
			{
				this.SetTitle(string.Format("{0} - {1} ({2}) on {3} - {4} ({5}) - {6}", App.Product, this.Session.Nickname,
					userModes, this.Session.NetworkName, this.Target.ToString(), channelModes, _topic));
			}
		}

		private void SetTitle(string text)
		{
			Dispatcher.BeginInvoke((Action)(() => this.Title = text));
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

		private void txtInput_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Enter:
					this.SubmitInput();
					break;
			}
		}

		private void txtInput_Pasting(object sender, DataObjectPastingEventArgs e)
		{
			if (e.DataObject.GetDataPresent(typeof(string)))
			{
				var text = e.DataObject.GetData(typeof(string)) as string;
				if (text.Contains(Environment.NewLine))
				{
					e.CancelCommand();

					this.Dispatcher.BeginInvoke((Action)(() =>
						{
							var parts = text.Split(Environment.NewLine.ToCharArray()).Where((s) => s.Trim().Length > 0).ToArray();
							if (parts.Length > App.Settings.Current.Buffer.MaximumPasteLines)
							{
								if (MessageBox.Show(string.Format("Are you sure you want to paste more than {0} lines?",
									App.Settings.Current.Buffer.MaximumPasteLines), "Paste Warnings", MessageBoxButton.YesNo,
									MessageBoxImage.Question) == MessageBoxResult.No)
								{
									return;
								}
							}
							foreach (var part in parts)
							{
								txtInput.Text = txtInput.Text.Substring(0, txtInput.SelectionStart);
								txtInput.Text += part;
								this.SubmitInput();
							}
						}));
				}
			}
		}
	}
}
