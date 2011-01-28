using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.Net
{
	/// <summary>
	/// This class represents a handler for a specific IRC code value. It can be used to "intercept" a response
	/// to a command and prevent other components from processing it.
	/// </summary>
	public sealed class IrcCodeHandler
	{
		/// <summary>
		/// Gets the IRC code that will be handled.
		/// </summary>
		public IrcCode Code { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the handler will be automatically removed after its first invocation.
		/// </summary>
		public bool AutoRemove { get; private set; }

		internal Func<IrcMessage, bool> Handler { get; private set; }

		/// <summary>
		/// Primary constructor.
		/// </summary>
		/// <param name="code">The IRC code to handle.</param>
		/// <param name="autoRemove">Whether to automatically remove the handler after first invocation.</param>
		/// <param name="handler">The function to handle the message.</param>
		public IrcCodeHandler(IrcCode code, bool autoRemove, Func<IrcMessage, bool> handler)
		{
			this.Code = code;
			this.AutoRemove = autoRemove;
			this.Handler = handler;
		}
	}
}
