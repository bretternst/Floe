using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.UI
{
	[Flags]
	public enum ChatMarker
	{
		None = 0,
		NewMarker = 1,
		OldMarker = 2
	}
}
