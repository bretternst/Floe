using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
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

	[Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IMMDeviceCollection
	{
		void GetCount(out int numDevices);
		void Item(int deviceNum, out IMMDevice device);
	}

	[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IMMDeviceEnumerator
	{
		void EnumAudioEndpoints(DataFlow dataFlow, DeviceState deviceStates, out IMMDeviceCollection devices);
		void GetDefaultAudioEndpoint(DataFlow dataFlow, Role role, out IMMDevice device);
		void GetDevice(string deviceId, out IMMDevice device);
		void RegisterEndpointNotificationCallback(IMMNotificationClient client);
		void UnregisterEndpointNotificationCallback(IMMNotificationClient client);
	}

	static class MMDeviceEnumerator
	{
		[ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
		private class CoClass
		{
		}

		public static IMMDeviceEnumerator Create()
		{
			return new CoClass() as IMMDeviceEnumerator;
		}
	}
}
