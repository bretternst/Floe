using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.Audio.Interop
{
	enum ClsCtx : uint
	{
		InProcServer = 0x1,
		InProcHandler = 0x2,
		All = InProcServer | InProcHandler
	}
}
