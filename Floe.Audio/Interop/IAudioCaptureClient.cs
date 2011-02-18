using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("C8ADBD64-E71E-48a0-A4DE-185C395CD317"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IAudioCaptureClient
	{
		void GetBuffer(out IntPtr data, out uint numFramesToRead, out AudioClientBufferFlags flags,
			out ulong devicePosition, out ulong qpcPosition);
		void ReleaseBuffer(uint numFramesRead);
		void GetNextPacketSize(out uint numFramesInNextPacket);
	}
}
