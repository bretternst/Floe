using System;
using System.Collections.Generic;
using System.Linq;

namespace Floe.Net
{
	/// <summary>
	/// Provides event arguments describing an IRC event.
	/// </summary>
	public class IrcEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the raw IRC message received or sent.
		/// </summary>
		public IrcMessage Message { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether the message has been handled.
		/// </summary>
		public bool Handled { get; set; }

		internal IrcEventArgs(IrcMessage message)
		{
			this.Message = message;
		}
	}

	/// <summary>
	/// Provides event arguments describing an IRC error.
	/// </summary>
	public sealed class ErrorEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the original exception that resulted from the error.
		/// </summary>
		public Exception Exception { get; private set; }

		internal ErrorEventArgs(Exception ex)
		{
			this.Exception = ex;
		}
	}

	/// <summary>
	/// Provides event arguments describing a nickname change event.
	/// </summary>
	public sealed class IrcNickEventArgs : IrcEventArgs
	{
		/// <summary>
		/// Gets the user's old nickname.
		/// </summary>
		public string OldNickname { get; private set; }

		/// <summary>
		/// Gets the user's new nickname.
		/// </summary>
		public string NewNickname { get; private set; }

		internal IrcNickEventArgs(IrcMessage message)
			: base(message)
		{
			var peer = message.From as IrcPeer;
			this.OldNickname = peer != null ? peer.Nickname : null;
			this.NewNickname = message.Parameters.Count > 0 ? message.Parameters[0] : null;
		}
	}

	/// <summary>
	/// Provides event arguments describing a chat message, such as a private message or message to a channel.
	/// </summary>
	public sealed class IrcMessageEventArgs : IrcEventArgs
	{
		/// <summary>
		/// Gets the user who sent the message.
		/// </summary>
		public IrcPeer From { get; private set; }

		/// <summary>
		/// Gets the target of the message. It could be sent to a channel or directly to the user who owns the IRC session.
		/// </summary>
		public IrcTarget To { get; private set; }

		/// <summary>
		/// Gets the message text.
		/// </summary>
		public string Text { get; private set; }

		internal IrcMessageEventArgs(IrcMessage message)
			: base(message)
		{
			this.From = message.From as IrcPeer;
			this.To = message.Parameters.Count > 0 ? new IrcTarget(message.Parameters[0]) : null;
			this.Text = message.Parameters.Count > 1 ? message.Parameters[1] : null;
		}
	}

	/// <summary>
	/// Provides event arguments describing a quit event.
	/// </summary>
	public sealed class IrcQuitEventArgs : IrcEventArgs
	{
		/// <summary>
		/// Gets the user who quit.
		/// </summary>
		public IrcPeer Who { get; private set; }

		/// <summary>
		/// Gets the quit message.
		/// </summary>
		public string Text { get; private set; }

		internal IrcQuitEventArgs(IrcMessage message)
			: base(message)
		{
			this.Who = message.From as IrcPeer;
			this.Text = message.Parameters.Count > 0 ? message.Parameters[0] : null;
		}
	}

	/// <summary>
	/// Provides event arguments describing a join event.
	/// </summary>
    public sealed class IrcJoinEventArgs : IrcEventArgs
    {
		/// <summary>
		/// Gets the user who joined.
		/// </summary>
        public IrcPeer Who { get; private set; }

		/// <summary>
		/// Gets the channel that the user joined.
		/// </summary>
        public IrcTarget Channel { get; private set; }

        internal IrcJoinEventArgs(IrcMessage message)
            : base(message)
        {
            this.Who = message.From as IrcPeer;
            this.Channel = message.Parameters.Count > 0 ? new IrcTarget(message.Parameters[0]) : null;
        }
    }

	/// <summary>
	/// Provides event arguments describing a part (leave) event.
	/// </summary>
    public sealed class IrcPartEventArgs : IrcEventArgs
	{
		/// <summary>
		/// Gets the user who left the channel.
		/// </summary>
		public IrcPeer Who { get; private set; }

		/// <summary>
		/// Gets the channel that the user left.
		/// </summary>
		public IrcTarget Channel { get; private set; }

		/// <summary>
		/// Gets the part text, if any exists.
		/// </summary>
		public string Text { get; private set; }

		internal IrcPartEventArgs(IrcMessage message)
			: base(message)
		{
			this.Who = message.From as IrcPeer;
			this.Channel = message.Parameters.Count > 0 ? new IrcTarget(message.Parameters[0]) : null;
			this.Text = message.Parameters.Count > 1 ? message.Parameters[1] : null;
		}
	}

	/// <summary>
	/// Provides event arguments describing a topic change event.
	/// </summary>
    public sealed class IrcTopicEventArgs : IrcEventArgs
    {
		/// <summary>
		/// Gets the user who changed the topic.
		/// </summary>
        public IrcPeer Who { get; private set; }

		/// <summary>
		/// Gets the channel in which the topic was changed.
		/// </summary>
        public IrcTarget Channel { get; private set; }

		/// <summary>
		/// Gets the new topic text.
		/// </summary>
        public string Text { get; private set; }

        internal IrcTopicEventArgs(IrcMessage message)
            : base(message)
        {
            this.Who = message.From as IrcPeer;
            this.Channel = message.Parameters.Count > 0 ? new IrcTarget(message.Parameters[0]) : null;
            this.Text = message.Parameters.Count > 1 ? message.Parameters[1] : null;
        }
    }

	/// <summary>
	/// Provides event arguments for an invite event.
	/// </summary>
	public sealed class IrcInviteEventArgs : IrcEventArgs
	{
		/// <summary>
		/// Gets the user who sent the invite.
		/// </summary>
		public IrcPeer From { get; private set; }

		/// <summary>
		/// Gets the channel to which the target was invited.
		/// </summary>
		public string Channel { get; private set; }

		internal IrcInviteEventArgs(IrcMessage message)
			: base(message)
		{
			this.From = message.From as IrcPeer;
			this.Channel = message.Parameters.Count > 1 ? message.Parameters[1] : null;
		}
	}

	/// <summary>
	/// Provides event arguments describing a kick event.
	/// </summary>
	public sealed class IrcKickEventArgs : IrcEventArgs
	{
		/// <summary>
		/// Gets the user who performed the kick.
		/// </summary>
		public IrcPeer Kicker { get; private set; }

		/// <summary>
		/// Gets the channel from which someone has been kicked.
		/// </summary>
		public IrcTarget Channel { get; private set; }

		/// <summary>
		/// Gets the nickname of the user who was kicked.
		/// </summary>
		public string KickeeNickname { get; private set; }

		/// <summary>
		/// Gets the associated kick text, typically describing the reason for the kick.
		/// </summary>
		public string Text { get; private set; }

		internal IrcKickEventArgs(IrcMessage message)
			: base(message)
		{
			this.Kicker = message.From as IrcPeer;
			this.Channel = message.Parameters.Count > 0 ? new IrcTarget(message.Parameters[0]) : null;
			this.KickeeNickname = message.Parameters.Count > 1 ? message.Parameters[1] : null;
			this.Text = message.Parameters.Count > 2 ? message.Parameters[2] : null;
		}
	}

	/// <summary>
	/// Provides event arguments describing a channel mode change event.
	/// </summary>
	public sealed class IrcChannelModeEventArgs : IrcEventArgs
	{
		/// <summary>
		/// Gets the user who performed the mode change.
		/// </summary>
		public IrcPeer Who { get; private set; }

		/// <summary>
		/// Gets the channel on which the modes were changed.
		/// </summary>
		public IrcTarget Channel { get; private set; }

		/// <summary>
		/// Gets the list of changed channel modes.
		/// </summary>
		public ICollection<IrcChannelMode> Modes { get; private set; }

		internal IrcChannelModeEventArgs(IrcMessage message)
			: base(message)
		{
			this.Who = message.From as IrcPeer;
			this.Channel = message.Parameters.Count > 0 ? new IrcTarget(message.Parameters[0]) : null;
			this.Modes = message.Parameters.Count > 1 ? IrcChannelMode.ParseModes(message.Parameters.Skip(1)) : null;
		}
	}

	/// <summary>
	/// Provides event arguments describing a user mode change event. This event always applies to the user who
	/// owns the IRC session.
	/// </summary>
	public sealed class IrcUserModeEventArgs : IrcEventArgs
	{
		/// <summary>
		/// Gets the list of changed user modes.
		/// </summary>
		public ICollection<IrcUserMode> Modes { get; private set; }

		internal IrcUserModeEventArgs(IrcMessage message)
			: base(message)
		{
			this.Modes = message.Parameters.Count > 1 ? IrcUserMode.ParseModes(message.Parameters.Skip(1)) : null;
		}
	}

	/// <summary>
	/// Provides event arguments describing miscellaneous information received from an IRC server. A numeric code
	/// is assigned to each message, typically describing the results of a command or an error that occurred.
	/// </summary>
	public sealed class IrcInfoEventArgs : IrcEventArgs
	{
		/// <summary>
		/// Gets the numeric IRC code.
		/// </summary>
		public IrcCode Code { get; private set; }

		/// <summary>
		/// Gets the text following the numeric code.
		/// </summary>
		public string Text { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the numeric code indicates than an error has occurred.
		/// </summary>
		public bool IsError { get; private set; }

		internal IrcInfoEventArgs(IrcMessage message)
			: base(message)
		{
			int code;
			if (int.TryParse(message.Command, out code))
			{
				this.Code = (IrcCode)code;
			}

			this.Text = message.Parameters.Count > 1 ? string.Join(" ", message.Parameters.Skip(1).ToArray()) : null;
			this.IsError = (int)this.Code >= 400;
		}
	}

	/// <summary>
	/// Provides event arguments describing a CTCP command sent from one client to another.
	/// </summary>
	public sealed class CtcpEventArgs : IrcEventArgs
	{
		/// <summary>
		/// Gets the user who sent the command.
		/// </summary>
		public IrcPeer From { get; private set; }

		/// <summary>
		/// Gets the target to which the command was sent. It could be sent to a channel or directly to the user who owns the IRC session.
		/// </summary>
		public IrcTarget To { get; private set; }

		/// <summary>
		/// Gets the CTCP command that was received.
		/// </summary>
		public CtcpCommand Command { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the received CTCP command is a response to a command that was previously sent.
		/// </summary>
		public bool IsResponse { get; private set; }

		internal CtcpEventArgs(IrcMessage message)
			: base(message)
		{
			this.From = message.From as IrcPeer;
			this.To = message.Parameters.Count > 0 ? new IrcTarget(message.Parameters[0]) : null;
			this.Command = message.Parameters.Count > 1 ? CtcpCommand.Parse(message.Parameters[1]) : null;
			this.IsResponse = message.Command == "NOTICE";
		}
	}
}
