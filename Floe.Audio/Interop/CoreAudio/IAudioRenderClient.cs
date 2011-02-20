using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IAudioRenderClient
	{
		void GetBuffer(int numFramesRequested, out IntPtr data);
		void ReleaseBuffer(int numFramesWritten, AudioClientBufferFlags flags);
	}
}
