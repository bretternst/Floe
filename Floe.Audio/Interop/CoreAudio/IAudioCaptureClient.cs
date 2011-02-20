using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("C8ADBD64-E71E-48a0-A4DE-185C395CD317"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IAudioCaptureClient
	{
		void GetBuffer(out IntPtr data, out int numFramesToRead, out AudioClientBufferFlags flags,
			out long devicePosition, out long qpcPosition);
		void ReleaseBuffer(int numFramesRead);
		void GetNextPacketSize(out int numFramesInNextPacket);
	}
}
