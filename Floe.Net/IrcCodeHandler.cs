using System;

namespace Floe.Net
{
	/// <summary>
	/// This class represents a handler for a specific IRC code value. It can be used to "intercept" a response
	/// to a command and prevent other components from processing it.
	/// </summary>
	public sealed class IrcCodeHandler
	{
		internal IrcCode[] Codes { get; private set; }
		internal Func<IrcInfoEventArgs, bool> Handler { get; private set; }

		/// <summary>
		/// Primary constructor.
		/// </summary>
		/// <param name="code">The IRC code to handle.</param>
		/// <param name="autoRemove">Whether to automatically remove the handler after first invocation.</param>
		/// <param name="handler">The function to handle the message. If the function returns true, the handler is removed.</param>
		/// <param name="errorHandler">The function to handle an error response. If the function returns true, the handler is removed.</param>
		public IrcCodeHandler(Func<IrcInfoEventArgs, bool> handler, params IrcCode[] codes)
		{
			this.Handler = handler;
			this.Codes = codes;
		}
	}
}
