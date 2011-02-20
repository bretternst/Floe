using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("56a86895-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IBaseFilter : IMediaFilter
	{
		new void GetClassID(out Guid classId);

		new void Stop();
		new void Pause();
		new void Run(ulong start);
		new void GetState(int msTimeout, out FilterState state);
		new void SetSyncSource(IReferenceClock clock);
		new void GetSyncSource(out IReferenceClock clock);

		void EnumPins(out IEnumPins pins);
		void FindPin([MarshalAs(UnmanagedType.LPWStr)] string id, out IPin pin);
		void QueryFilterInfo(out FilterInfo info);
		void JoinFilterGraph(IFilterGraph graph, [MarshalAs(UnmanagedType.LPWStr)] string name);
		void QueryVendorInfo([MarshalAs(UnmanagedType.LPWStr)] out string vendorInfo);
	}
}
