using System;
using System.Collections.Generic;
using System.Text;

namespace Floe.Net
{
	/// <summary>
	/// Provides event arguments for a DCC chat message event.
	/// </summary>
	public class DccChatEventArgs : EventArgs
	{
		public string Text { get; private set; }

		internal DccChatEventArgs(string text)
		{
			this.Text = text;
		}
	}

	/// <summary>
	/// A class responsible for sending and receiving textual messages through a direct connection to another host.
	/// </summary>
	public class DccChat : DccOperation
	{
		private List<byte> _input = new List<byte>(512);

		/// <summary>
		/// Fires when a line of text has been received from the remote host.
		/// </summary>
		public EventHandler<DccChatEventArgs> MessageReceived;

		/// <summary>
		/// Enqueue a line of text to be sent to the remote host.
		/// </summary>
		/// <param name="text">The line of text to send.</param>
		public void QueueMessage(string text)
		{
			byte[] data = Encoding.UTF8.GetBytes(text + "\u000d\u000a");
			this.QueueWrite(data, 0, data.Length);
		}

		protected override void OnReceived(byte[] buffer, int count)
		{
			for (int i = 0; i < count; i++)
			{
				switch (buffer[i])
				{
					case 0xa:
						string input = Encoding.UTF8.GetString(_input.ToArray());
						_input.Clear();
						this.RaiseEvent(this.MessageReceived, new DccChatEventArgs(input));
						break;
					case 0xd:
						break;
					default:
						_input.Add(buffer[i]);
						break;
				}
			}
		}
	}
}
