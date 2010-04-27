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
	public enum OutputType
	{
		Client,
		Server,
		SelfMessage,
		PrivateMessage,
		Notice,
		Topic,
		Nick,
		Action,
		Join,
		Part,
		Kicked,
		SelfKicked
	}

	public partial class ChatControl : UserControl, IDisposable
	{
		public ChatContext Context { get; private set; }

		public ChatControl(ChatContext context)
		{
			this.Context = context;

			InitializeComponent();

			this.Loaded += (sender, e) => Keyboard.Focus(txtInput);
			this.Context.Session.StateChanged += new EventHandler<EventArgs>(Session_StateChanged);
			this.Context.Session.Noticed += new EventHandler<IrcDialogEventArgs>(Session_Noticed);
			this.Context.Session.PrivateMessaged += new EventHandler<IrcDialogEventArgs>(Session_PrivateMessaged);
			this.Context.Session.Kicked += new EventHandler<IrcKickEventArgs>(Session_Kicked);
			this.Context.Session.InfoReceived += new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
		}

		private void ParseInput(string text)
		{
			try
			{
				if (CommandParser.Execute(this.Context, text))
				{
					this.Write(OutputType.SelfMessage, text);
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
			}

			Dispatcher.BeginInvoke((Action)(() =>
				{
					if (txtOutput.Text.Length > 0)
					{
						txtOutput.AppendText(Environment.NewLine);
					}
					txtOutput.AppendText(output);
					txtOutput.ScrollToEnd();
				}));
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			e.Handled = true;

			switch (e.Key)
			{
				case Key.PageUp:
					txtOutput.PageUp();
					break;
				case Key.PageDown:
					txtOutput.PageDown();
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
				case Key.PageUp:
					txtOutput.PageUp();
					break;
				case Key.PageDown:
					txtOutput.PageDown();
					break;
			}
		}

		private void Session_StateChanged(object sender, EventArgs e)
		{
			if (this.Context.Session.State == IrcSessionState.Disconnected)
			{
				this.Write(OutputType.Server, "Disconnected");
			}
		}

		private void Session_Noticed(object sender, IrcDialogEventArgs e)
		{
			this.Write(OutputType.Notice, e.From, e.Text);
		}

		private void Session_PrivateMessaged(object sender, IrcDialogEventArgs e)
		{
			if (this.Context.Target.Equals(e.To))
			{
				this.Write(OutputType.PrivateMessage, e.From, e.Text);
			}
		}

		private void Session_Kicked(object sender, IrcKickEventArgs e)
		{
			if (e.IsSelfKicked && this.Context.Target == null)
			{
				this.Write(OutputType.SelfKicked,
					string.Format("You have been kicked from {0} by {1} ({2}",
					e.Channel, e.Kicker, e.Text));
			}
			else if (this.Context.Target.Equals(e.Channel))
			{
				this.Write(OutputType.Kicked,
					string.Format("{0} has been kicked by {1} ({2})",
					e.KickeeNickname, e.Kicker, e.Text));
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
		}

		public void Dispose()
		{
			this.Context.Session.StateChanged -= new EventHandler<EventArgs>(Session_StateChanged);
			this.Context.Session.Noticed -= new EventHandler<IrcDialogEventArgs>(Session_Noticed);
			this.Context.Session.PrivateMessaged -= new EventHandler<IrcDialogEventArgs>(Session_PrivateMessaged);
			this.Context.Session.Kicked -= new EventHandler<IrcKickEventArgs>(Session_Kicked);
			this.Context.Session.InfoReceived -= new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
		}
	}
}
