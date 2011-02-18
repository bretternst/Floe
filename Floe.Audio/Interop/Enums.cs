using System;

namespace Floe.Audio.Interop
{
	enum ClsCtx : uint
	{
		InProcServer = 0x1,
		InProcHandler = 0x2,
		All = InProcServer | InProcHandler
	}

	enum StorageAccessMode
	{
		Read = 0,
		Write = 1,
		ReadWrite = 2
	}

	enum DataFlow
	{
		Reader = 0,
		Capture = 1,
		All = 2
	}

	[Flags]
	enum DeviceState
	{
		Active = 0x01,
		Disabled = 0x02,
		NotPresent = 0x04,
		Unplugged = 0x08,
		All = 0x0f
	}

	enum Role
	{
		Console = 0,
		Multimedia = 1,
		Communications = 2
	}

	enum AudioShareMode
	{
		Shared,
		Exclusive
	}

	[Flags]
	enum AudioStreamFlags
	{
		None = 0,
		CrossProcess = 0x00010000,
		Loopback = 0x00020000,
		EventCallback = 0x00040000,
		NoPersist = 0x00080000,
		RateAdjust = 0x00100000,
		ExpireWhenUnowned = 0x10000000,
		DisplayHide = 0x20000000,
		HideWhenExpired = 0x40000000
	}

	[Flags]
	enum AudioClientBufferFlags
	{
		DataDiscontinuity = 0x1,
		Silent = 0x2,
		TimestampError = 0x4
	}
}
