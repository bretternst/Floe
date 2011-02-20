using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("89c31040-846b-11ce-97d3-00aa0055595a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IEnumMediaTypes
	{
		void Next(int count, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] IntPtr[] mediatypes, out int fetched);
		void Skip(int count);
		void Reset();
		void Clone(out IEnumMediaTypes mediaTypes);
	}

	[Guid("56a86893-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IEnumFilters
	{
		void Next(int count, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IBaseFilter[] mediatypes, out int fetched);
		void Skip(int count);
		void Reset();
		void Clone(out IEnumFilters mediaTypes);
	}

	[Guid("56a86892-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IEnumPins
	{
		void Next(int count, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] IPin[] mediatypes, out int fetched);
		void Skip(int count);
		void Reset();
		void Clone(out IEnumPins mediaTypes);
	}
}
