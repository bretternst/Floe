using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("56a86891-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IPin
	{
		void Connect(IPin receivePin, [In] MediaType mediaType);
		void ReceiveConnection([In] IPin connector, [In] MediaType mediaType);
		void Disconnect();
		void ConnectTo([In] IPin pin);
		void ConnectionMediaType(out MediaType mediaType);
		void QueryPinInfo(out PinInfo info);
		void QueryDirection(out PinDirection pinDir);
		void QueryId([MarshalAs(UnmanagedType.LPWStr)] out string id);
		int QueryAccept([In] ref MediaType mediaType);
		void EnumMediaTypes(out IEnumMediaTypes types);
		void QueryInternalConnections([Out] IPin[] pins, ref int count);
		void EndOfStream();
		void BeginFlush();
		void EndFlush();
		void NewSegment(ulong start, ulong stop, double rate);
	}
}
