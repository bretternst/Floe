using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IMMDevice
	{
		void Activate(ref Guid id, ClsCtx clsctx, IntPtr activationParams,
			[MarshalAs(UnmanagedType.IUnknown)] out object deviceInterface);
		void OpenPropertyStore(StorageAccessMode access, out IPropertyStore properties);
		void GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);
		void GetState(out DeviceState state);
	}
}
