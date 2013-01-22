using System;

namespace Floe.UI
{
	[Flags]
	public enum IgnoreActions
	{
		None = 0,
		Join = 1,
		Part = 2,
		Quit = 4,
		Channel = 8,
		Private = 16,
		Notice = 32,
		Ctcp = 64,
		NickChange = 128,
		Invite = 256,
		All = 511
	}
}
