using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IAudioClient
	{
		void Initialize(AudioShareMode shareMode, AudioStreamFlags streamFlags, long bufferDuration,
			long periodicity, [In] ref WaveFormat format, [In] ref Guid audioSessionId);
		void GetBufferSize(out uint numBufferFrames);
		void GetStreamLatency(out long latency);
		void GetCurrentPadding(out uint numPaddingFrames);
		void IsFormatSupported(AudioShareMode shareMode, ref WaveFormat format,
			[MarshalAs(UnmanagedType.LPStruct)] out WaveFormat closestMatch);
		void GetMixFormat([MarshalAs(UnmanagedType.LPStruct)] out WaveFormat deviceFormat);
		void GetDevicePeriod(out long defaultDevicePeriod, out long minimumDevicePeriod);
		void Start();
		void Stop();
		void Reset();
		void SetEventHandle(IntPtr eventHandle);
		void GetService(ref Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object service);
	}
}
