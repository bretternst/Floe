using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("56a868b1-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	interface IMediaControl
	{
		void Run();
		void Pause();
		void Stop();
		void GetState(int timeout, out FilterState fs);
		void RenderFile(string fileName);
		void AddSourceFilter(string fileName, [MarshalAs(UnmanagedType.IDispatch)] out object unk);
		void GetFilterCollection([MarshalAs(UnmanagedType.IDispatch)] out object unk);
		void GetRegFilterCollection([MarshalAs(UnmanagedType.IDispatch)] out object unk);
		void StopWhenReady();
	}
}
