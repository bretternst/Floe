using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IPropertyStore
	{
		void GetCount(out int count);
		void GetAt(int index, out PropertyKey key);
		void GetValue(ref PropertyKey key, out PropertyVariant value);
		void SetValue(ref PropertyKey key, ref PropertyVariant value);
		void Commit();
	}
}
