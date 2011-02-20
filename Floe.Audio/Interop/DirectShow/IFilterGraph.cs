using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("56a8689f-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IFilterGraph
	{
		void AddFilter([In] IBaseFilter filter, [MarshalAs(UnmanagedType.LPWStr)] string name);
		void RemoveFilter([In] IBaseFilter filter);
		void EnumFilters(out IEnumFilters filters);
		void FindFilterByName([MarshalAs(UnmanagedType.LPWStr)] string name, out IBaseFilter filter);
		void ConnectDirect(IPin pinOut, IPin pinIn, [In] ref MediaType mediaType);
		void Reconnect(IPin pin);
		void Disconnect(IPin pin);
		void SetDefaultSyncSource();
	}
}
