using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("56a868a9-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IGraphBuilder : IFilterGraph
	{
		new void AddFilter([In] IBaseFilter filter, [MarshalAs(UnmanagedType.LPWStr)] string name);
		new void RemoveFilter([In] IBaseFilter filter);
		new void EnumFilters(out IEnumFilters filters);
		new void FindFilterByName([MarshalAs(UnmanagedType.LPWStr)] string name, out IBaseFilter filter);
		new void ConnectDirect(IPin pinOut, IPin pinIn, [In] ref MediaType mediaType);
		new void Reconnect(IPin pin);
		new void Disconnect(IPin pin);
		new void SetDefaultSyncSource();

		void Connect(IPin pinOut, IPin pinIn);
		void Render(IPin pinOut);
		void RenderFile([MarshalAs(UnmanagedType.LPWStr)] string file, IntPtr reserved);
		void AddSourceFilter([MarshalAs(UnmanagedType.LPWStr)] string fileName, [MarshalAs(UnmanagedType.LPWStr)] string filterName,
			out IBaseFilter filter);
		void SetLogFile(IntPtr file);
		void Abort();
		int ShouldOperationContinue();
	}

	class FilterGraph
	{
		[ComImport, Guid("e436ebb3-524f-11ce-9f53-0020af0ba770")]
		private class CoClass
		{
		}

		public static IGraphBuilder Create()
		{
			return (IGraphBuilder)(new CoClass());
		}
	}
}
