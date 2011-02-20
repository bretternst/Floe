using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("56a86899-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IMediaFilter : IPersist
	{
		new void GetClassID(out Guid classId);

		void Stop();
		void Pause();
		void Run(ulong start);
		void GetState(int msTimeout, out FilterState state);
		void SetSyncSource(IReferenceClock clock);
		void GetSyncSource(out IReferenceClock clock);
	}
}
