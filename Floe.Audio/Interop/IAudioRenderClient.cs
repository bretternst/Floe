using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IAudioRenderClient
	{
		void GetBuffer(uint numFramesRequested, out IntPtr data);
		void ReleaseBuffer(uint numFramesWritten, AudioClientBufferFlags flags);
	}
}
