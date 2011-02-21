using System;

namespace Floe.Audio.Interop
{
	enum PinDirection
	{
		Input,
		Output
	}

	enum FilterState
	{
		Stopped,
		Paused,
		Running
	}

	enum EventCode
	{
		Complete = 0x1,
		UserAbort = 0x2,
		ErrorAbort = 0x3,
		ErrorStopped = 0x06,
		ErrorStillPlaying = 0x08,
		Paused = 0x0e,
		StreamControlStopped = 0x1a,
		StreamControlStarted = 0x1b,
		StateChange = 0x32,
		GraphChanged = 0x50,
	}
}
