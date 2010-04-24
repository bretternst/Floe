using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Floe.UI
{
	public delegate void InputEventHandler(object sender, InputEventArgs e);

	public sealed class InputEventArgs : RoutedEventArgs
	{
		public ChatController Context { get; private set; }
		public string Text { get; private set; }

		public InputEventArgs(object source, string text)
			: base(ChatControl.InputReceivedEvent, source)
		{
			var control = source as ChatControl;
			if (control != null)
			{
				this.Context = control.Context;
			}
			this.Text = text;
		}
	}
}
