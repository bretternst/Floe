using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.Net
{
	public class IrcCodeHandler
	{
		public IrcCode Code { get; private set; }
		public bool AutoRemove { get; private set; }
		public Func<IrcMessage, bool> Handler { get; private set; }

		public IrcCodeHandler(IrcCode code, bool autoRemove, Func<IrcMessage, bool> handler)
		{
			this.Code = code;
			this.AutoRemove = autoRemove;
			this.Handler = handler;
		}
	}
}
