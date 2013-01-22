using System;

namespace Floe.UI
{
	public enum SearchDirection
	{
		Previous,
		Next
	}

	[Flags]
	public enum SearchOptions
	{
		None = 0,
		MatchCase = 1
	}
}
