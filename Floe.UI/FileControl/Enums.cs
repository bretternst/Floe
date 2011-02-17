using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.UI
{
	public enum FileStatus
	{
		Asking,
		Working,
		Cancelled,
		Received,
		Sent
	}

	public enum DccMethod
	{
		Send,
		Xmit
	}
}
