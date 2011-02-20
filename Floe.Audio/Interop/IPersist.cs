using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("0000010c-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IPersist
	{
		void GetClassID(out Guid classId);
	}
}
