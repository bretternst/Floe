using System;
using System.Net;

namespace Floe.Audio
{
	/// <summary>
	/// Defines a method signature for a handler that determines whether audio should be encoded and transmitted.
	/// </summary>
	/// <returns>Returns true if transmission should occur, false otherwise.</returns>
	public delegate bool TransmitPredicate();

	/// <summary>
	/// Defines a method signature for a handler that determines whether to process audio received from a given endpoint.
	/// </summary>
	/// <param name="endpoint">The endpoint from which a packet was received.</param>
	/// <returns>Returns true if the packet should be processed, false otherwise.</returns>
	public delegate bool ReceivePredicate(IPEndPoint endpoint);
}
