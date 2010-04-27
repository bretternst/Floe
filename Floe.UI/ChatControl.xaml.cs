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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatControl : UserControl, IDisposable
	{
		private ScrollViewer _scrollViewer;
		private double _scrollPos = 0.0;

		public ChatControl(ChatContext context)
		{
			this.Context = context;

			InitializeComponent();

			this.Header = context.Target == null ? "Server" : context.Target.ToString();

			this.Loaded += (sender, e) =>
			{
				Keyboard.Focus(txtInput);
				if (_scrollViewer == null)
				{
					_scrollViewer = vwOutput.FindScrollViewer();
				}
			};
			this.Context.Session.StateChanged += new EventHandler<EventArgs>(Session_StateChanged);
			this.Context.Session.Noticed += new EventHandler<IrcDialogEventArgs>(Session_Noticed);
			this.Context.Session.PrivateMessaged += new EventHandler<IrcDialogEventArgs>(Session_PrivateMessaged);
			this.Context.Session.Kicked += new EventHandler<IrcKickEventArgs>(Session_Kicked);
			this.Context.Session.InfoReceived += new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
			this.Context.Session.CtcpCommandReceived += new EventHandler<CtcpEventArgs>(Session_CtcpCommandReceived);
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

		private void ParseInput(string text)
		{
			try
			{
				CommandOutput output;
				if ((output = CommandParser.Execute(this.Context, text)) != null)
				{
					this.Write(output.Type, output.Text);
				}
			}
			catch (IrcException ex)
			{
				this.Write(OutputType.Client, null, ex.Message);
			}
			catch (InputException ex)
			{
				this.Write(OutputType.Client, null, ex.Message);
			}
		}

		private void Write(OutputType type, string text)
		{
			this.Write(type, null, text);
		}

		private void Write(OutputType type, IrcPrefix from, string text)
		{
			this.Write(type, from, null, text);
		}

		private void Write(OutputType type, IrcPrefix from, string arg, string text)
		{
			string output = string.Empty;
			var peer = from as IrcPeer;

			switch (type)
			{
				case OutputType.Server:
					output = string.Format("*** {0}", text);
					break;
				case OutputType.Client:
					output = text;
					break;
				case OutputType.SelfMessage:
					output = string.Format("<{0}> {1}", this.Context.Session.Nickname, text);
					break;
				case OutputType.SelfAction:
					output = string.Format("* {0} {1}", this.Context.Session.Nickname, text);
					break;
				case OutputType.Action:
					output = string.Format("* {0} {1}", peer.Nickname, text);
					break;
				case OutputType.Join:
					output = string.Format("* {0} ({1}@{2}) has joined channel {3}", peer.Nickname, peer.UserName, peer.HostName,
						this.Context.Target.ToString());
					break;
				case OutputType.Nick:
					output = string.Format("* {0} is now known as {1}", peer.Nickname, text);
					break;
				case OutputType.Notice:
					if (peer != null)
					{
						output = string.Format("-{0}- {1}", peer.Nickname, text);
					}
					else
					{
						output = text;
					}
					break;
				case OutputType.Part:
					output = string.Format("* {0} ({1}@{2}) has left channel {3}", peer.Nickname, peer.UserName, peer.HostName,
						this.Context.Target.ToString());
					break;
				case OutputType.PrivateMessage:
					output = string.Format("<{0}> {1}", peer.Nickname, text);
					break;
				case OutputType.Topic:
					output = string.Format("* {0} changes topic to '{1}'", peer.Nickname, text);
					break;
				case OutputType.SelfKicked:
					output = string.Format("* You have been kicked from {0} by {1} ({2})", arg, peer.Nickname, text);
					break;
				case OutputType.Kicked:
					output = string.Format("* {0} has been kicked by {1} ({2})", arg, peer.Nickname, text);
					break;
				default:
					return;
			}

			Dispatcher.BeginInvoke((Action)(() =>
				{
					vwOutput.Document.Blocks.Add(new Paragraph(new Run(output)));
					if (_scrollViewer.VerticalOffset - _scrollPos >= -1.0)
					{
						_scrollViewer.ScrollToBottom();
						_scrollPos = _scrollViewer.VerticalOffset;
					}
				}));
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			e.Handled = true;

			switch (e.Key)
			{
				case Key.PageUp:
					_scrollViewer.PageUp();
					break;
				case Key.PageDown:
					_scrollViewer.PageDown();
					break;
				case Key.LeftCtrl:
				case Key.RightCtrl:
				case Key.LeftAlt:
				case Key.RightAlt:
					e.Handled = false;
					return;
				default:
					if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl) &&
						!Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt))
					{
						Keyboard.Focus(txtInput);
					}
					e.Handled = false;
					break;
			}

			base.OnPreviewKeyDown(e);
		}

		private void txtInput_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Enter:
					string text = txtInput.Text;
					txtInput.Clear();
					this.ParseInput(text);
					break;
			}
		}

		private void Session_StateChanged(object sender, EventArgs e)
		{
			if (this.Context.Session.State == IrcSessionState.Disconnected)
			{
				this.Write(OutputType.Server, "Disconnected");
				if(this.Context.Target == null)
				{
					this.Dispatcher.BeginInvoke((Action)(() => this.Header = "Server"));
				}
			}
			else if (this.Context.Session.State == IrcSessionState.Connecting &&
				this.Context.Target == null)
			{
				this.Dispatcher.BeginInvoke((Action)(() => this.Header = this.Context.Session.Server ));
			}
		}

		private void Session_Noticed(object sender, IrcDialogEventArgs e)
		{
			this.Write(OutputType.Notice, e.From, e.Text);
		}

		private void Session_PrivateMessaged(object sender, IrcDialogEventArgs e)
		{
			if (this.Context.Target != null && this.Context.Target.Equals(e.To))
			{
				this.Write(OutputType.PrivateMessage, e.From, e.Text);
			}
		}

		private void Session_Kicked(object sender, IrcKickEventArgs e)
		{
			if (e.IsSelfKicked && this.Context.Target == null)
			{
				this.Write(OutputType.SelfKicked, e.Kicker, e.Channel.ToString(), e.Text);
			}
			else if (this.Context.Target != null && this.Context.Target.Equals(e.Channel))
			{
				this.Write(OutputType.Kicked, e.Kicker, e.KickeeNickname, e.Text);
			}
		}

		private void Session_InfoReceived(object sender, IrcInfoEventArgs e)
		{
			if ((int)e.Code < 200 && this.Context.Target == null)
			{
				this.Write(OutputType.Server, e.Text);
			}
			else if (this.IsVisible)
			{
				this.Write(OutputType.Server, e.Text);
			}

			if (e.Code == IrcCode.Welcome && this.Context.Target == null)
			{
				if (e.Text.StartsWith("Welcome to the "))
				{
					var parts = e.Text.Split(' ');
					if (parts.Length > 3)
					{
						this.Dispatcher.BeginInvoke((Action)(() => this.Header = parts[3]));
					}
				}
			}
		}

		private void Session_CtcpCommandReceived(object sender, CtcpEventArgs e)
		{
			if (this.Context.Target != null && this.Context.Target.Equals(e.To))
			{
				this.Write(OutputType.Action, e.From, string.Join(" ", e.Command.Arguments));
			}
		}

		public void Dispose()
		{
			this.Context.Session.StateChanged -= new EventHandler<EventArgs>(Session_StateChanged);
			this.Context.Session.Noticed -= new EventHandler<IrcDialogEventArgs>(Session_Noticed);
			this.Context.Session.PrivateMessaged -= new EventHandler<IrcDialogEventArgs>(Session_PrivateMessaged);
			this.Context.Session.Kicked -= new EventHandler<IrcKickEventArgs>(Session_Kicked);
			this.Context.Session.InfoReceived -= new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
			this.Context.Session.CtcpCommandReceived -= new EventHandler<CtcpEventArgs>(Session_CtcpCommandReceived);
		}
	}
}
