using System;
using System.Collections.Generic;
using System.Linq;

namespace Floe.Net
{
	public class IrcEventArgs : EventArgs
	{
		public IrcMessage Message { get; private set; }

		internal IrcEventArgs(IrcMessage message)
		{
			this.Message = message;
		}
	}

	public class ErrorEventArgs : EventArgs
	{
		public Exception Exception { get; private set; }

		internal ErrorEventArgs(Exception ex)
		{
			this.Exception = ex;
		}
	}

	public class IrcNickEventArgs : IrcEventArgs
	{
		public string OldNickname { get; private set; }
		public string NewNickname { get; private set; }
		public bool IsSelf { get; private set; }

		public IrcNickEventArgs(IrcMessage message, string ownNickname)
			: base(message)
		{
			var peer = message.From as IrcPeer;
			this.OldNickname = peer != null ? peer.Nickname : null;
			this.NewNickname = message.Parameters.Count > 0 ? message.Parameters[0] : null;
			this.IsSelf = string.Compare(this.OldNickname, ownNickname, StringComparison.OrdinalIgnoreCase) == 0;
		}
	}

	public class IrcDialogEventArgs : IrcEventArgs
	{
		public IrcPeer From { get; private set; }
		public IrcTarget To { get; private set; }
		public string Text { get; private set; }

		public IrcDialogEventArgs(IrcMessage message)
			: base(message)
		{
			this.From = message.From as IrcPeer;
			this.To = message.Parameters.Count > 0 ? new IrcTarget(message.Parameters[0]) : null;
			this.Text = message.Parameters.Count > 1 ? message.Parameters[1] : null;
		}
	}

	public class IrcQuitEventArgs : IrcEventArgs
	{
		public IrcPeer Who { get; private set; }
		public string Text { get; private set; }

		public IrcQuitEventArgs(IrcMessage message)
			: base(message)
		{
			this.Who = message.From as IrcPeer;
			this.Text = message.Parameters.Count > 0 ? message.Parameters[0] : null;
		}
	}

	public class IrcChannelEventArgs : IrcEventArgs
	{
		public IrcPeer Who { get; private set; }
		public string Channel { get; private set; }
		public string Text { get; private set; }

		public IrcChannelEventArgs(IrcMessage message, string ownNick)
			: base(message)
		{
			this.Who = message.From as IrcPeer;
			this.Channel = message.Parameters.Count > 0 ? message.Parameters[0] : null;
			this.Text = message.Parameters.Count > 1 ? message.Parameters[1] : null;
		}
	}

	public class IrcInviteEventArgs : IrcEventArgs
	{
		public IrcPeer From { get; private set; }
		public string Channel { get; private set; }

		public IrcInviteEventArgs(IrcMessage message)
			: base(message)
		{
			this.From = message.From as IrcPeer;
			this.Channel = message.Parameters.Count > 1 ? message.Parameters[1] : null;
		}
	}

	public class IrcKickEventArgs : IrcEventArgs
	{
		public IrcPeer Kicker { get; private set; }
		public string Channel { get; private set; }
		public string KickeeNickname { get; private set; }
		public bool IsSelfKicker { get; private set; }
		public bool IsSelfKicked { get; private set; }

		public IrcKickEventArgs(IrcMessage message, string ownNickname)
			: base(message)
		{
			this.Kicker = message.From as IrcPeer;
			this.Channel = message.Parameters.Count > 0 ? message.Parameters[0] : null;
			this.KickeeNickname = message.Parameters.Count > 1 ? message.Parameters[1] : null;
			this.IsSelfKicker = this.Kicker != null && 
				string.Compare(this.Kicker.Nickname, ownNickname, StringComparison.OrdinalIgnoreCase) == 0;
			this.IsSelfKicked = string.Compare(this.KickeeNickname, ownNickname, StringComparison.OrdinalIgnoreCase) == 0;
		}
	}

	public class IrcChannelModeEventArgs : IrcEventArgs
	{
		public IrcPeer Who { get; private set; }
		public string Channel { get; private set; }
		public ICollection<IrcChannelMode> Modes { get; private set; }

		public IrcChannelModeEventArgs(IrcMessage message)
			: base(message)
		{
			this.Who = message.From as IrcPeer;
			this.Channel = message.Parameters.Count > 0 ? message.Parameters[0] : null;
			this.Modes = message.Parameters.Count > 1 ? IrcChannelMode.ParseModes(message.Parameters.Skip(1)) : null;
		}
	}

	public class IrcUserModeEventArgs : IrcEventArgs
	{
		public IrcTarget Who { get; private set; }
		public bool IsSelf { get; private set; }
		public ICollection<IrcUserMode> Modes { get; private set; }

		public IrcUserModeEventArgs(IrcMessage message, string ownNickname)
			: base(message)
		{
			this.Who = message.Parameters.Count > 0 ? new IrcTarget(message.Parameters[0]) : null;
			this.IsSelf = this.Who != null && this.Who.Type == IrcTargetType.Nickname &&
				string.Compare(this.Who.Name, ownNickname, StringComparison.OrdinalIgnoreCase) == 0;
			this.Modes = message.Parameters.Count > 1 ? IrcUserMode.ParseModes(message.Parameters.Skip(1)) : null;
		}
	}

	public class IrcInfoEventArgs : IrcEventArgs
	{
		public IrcCode Code { get; private set; }
		public IrcTarget To { get; private set; }
		public string Text { get; private set; }
		public bool IsError { get; private set; }

		public IrcInfoEventArgs(IrcMessage message)
			: base(message)
		{
			int code;
			if (int.TryParse(message.Command, out code))
			{
				this.Code = (IrcCode)code;
			}

			this.To = message.Parameters.Count > 0 ? new IrcTarget(message.Parameters[0]) : null;
			this.Text = message.Parameters.Count > 1 ? message.Parameters[1] : null;
			this.IsError = (int)this.Code >= 400;
		}
	}

	public class CtcpEventArgs : IrcEventArgs
	{
		public IrcPeer From { get; private set; }
		public IrcTarget To { get; private set; }
		public CtcpCommand Command { get; private set; }

		public CtcpEventArgs(IrcMessage message)
			: base(message)
		{
			this.From = message.From as IrcPeer;
			this.To = message.Parameters.Count > 0 ? new IrcTarget(message.Parameters[0]) : null;
			this.Command = message.Parameters.Count > 1 ? CtcpCommand.Parse(message.Parameters[1]) : null;
		}
	}
}
