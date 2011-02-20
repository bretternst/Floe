using System;

namespace Floe.Audio.Interop
{
	static class PropertyKeys
	{
		public static readonly PropertyKey DeviceFriendlyName = new PropertyKey(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 14);
	}

	static class Interfaces
	{
		public static readonly Guid IAudioclient = new Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2");
		public static readonly Guid IAudioRenderClient = new Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2");
		public static readonly Guid IAudioCaptureClient = new Guid("C8ADBD64-E71E-48a0-A4DE-185C395CD317");
		public static readonly Guid ISimpleAudioVolume = new Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8");
	}

	static class ResultCodes
	{
		public const uint AudioClientFormatNotSupported = 0x88890008;
	}
}
