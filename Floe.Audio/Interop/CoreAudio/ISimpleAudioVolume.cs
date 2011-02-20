using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface ISimpleAudioVolume
	{
		void SetMasterVolume(float level, [In] ref Guid eventContext);
		void GetMasterVolume(out float level);
		void SetMute(bool mute, [In] ref Guid eventContext);
		void GetMute(out bool mute);
	}
}
