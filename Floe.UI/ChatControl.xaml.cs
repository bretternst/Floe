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
	public partial class ChatControl : UserControl
	{
		public static readonly RoutedEvent InputReceivedEvent = EventManager.RegisterRoutedEvent(
			"InputReceived", RoutingStrategy.Bubble, typeof(InputEventHandler), typeof(ChatControl));

		public ChatController Context { get; private set; }

		public ChatControl(ChatController context)
		{
			this.Context = context;

			InitializeComponent();

			this.Context.OutputReceived += new EventHandler<OutputEventArgs>(Context_OutputReceived);
		}

		private void txtInput_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				string text = txtInput.Text;
				txtInput.Clear();
				this.RaiseEvent(new InputEventArgs(this, text));
			}
		}

		private void Context_OutputReceived(object sender, OutputEventArgs e)
		{
			string output = "";

			var peer = e.From as IrcPeer;

			switch (e.Type)
			{
				case OutputType.Info:
					output = string.Format("*** {0}", e.Text);
					break;
				case OutputType.Client:
					output = e.Text;
					break;
				case OutputType.Action:
					output = string.Format("* {0} {1}", peer.Nickname, e.Text);
					break;
				case OutputType.Join:
					output = string.Format("* {0} ({1}@{2}) has joined channel {3}", peer.Nickname, peer.UserName, peer.HostName,
						this.Context.Target.ToString());
					break;
				case OutputType.Nick:
					output = string.Format("* {0} is now known as {1}", peer.Nickname, e.Text);
					break;
				case OutputType.Notice:
					if (peer != null)
					{
						output = string.Format("-{0}- {1}", peer.Nickname, e.Text);
					}
					else
					{
						output = e.Text;
					}
					break;
				case OutputType.Part:
					output = string.Format("* {0} ({1}@{2}) has left channel {3}", peer.Nickname, peer.UserName, peer.HostName,
						this.Context.Target.ToString());
					break;
				case OutputType.PrivateMessage:
					output = string.Format("<{0}> {1}", peer.Nickname, e.Text);
					break;
				case OutputType.Topic:
					output = string.Format("* {0} changes topic to '{1}'", peer.Nickname, e.Text);
					break;
				case OutputType.Disconnected:
					output = "*** Disconnected";
					break;
			}
			txtOutput.AppendText(output);
			txtOutput.AppendText(Environment.NewLine);
			txtOutput.ScrollToEnd();
		}
	}
}
