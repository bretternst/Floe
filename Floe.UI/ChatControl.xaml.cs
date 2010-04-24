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
			if (e.Message != null)
			{
				txtOutput.AppendText(e.Message.ToString() + Environment.NewLine);
			}
			else
			{
				txtOutput.AppendText(e.Text + Environment.NewLine);
			}
		}
	}
}
