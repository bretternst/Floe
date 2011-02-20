using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("56a86897-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IReferenceClock
	{
		void GetTime(out ulong time);
		void AdviseTime(ulong baseTime, ulong streamTime, IntPtr hEvent, out IntPtr adviseCookie);
		void AdvisePeriodic(ulong startTime, ulong periodTime, IntPtr semaphore, out IntPtr adviseCookie);
		void Unadvise(IntPtr adviseCookie);
	}
}
