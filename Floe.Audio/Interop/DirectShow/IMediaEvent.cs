using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("56a868b6-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	interface IMediaEvent
	{
		void GetEventHandle(out IntPtr handle);
		void GetEvent(out int eventCode, out IntPtr lParam1, out IntPtr lParam2, int timeout);
		int WaitForCompletion(int timeout, out int eventCode);
		void CancelDefaultHandling(int eventCode);
		void RestoreDefaultHandling(int eventCode);
		void FreeEventParams(int eventCode, IntPtr param1, IntPtr param2);
	}
}
