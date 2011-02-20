using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IMMNotificationClient
	{
		void OnDeviceStateChanged(string deviceId, DeviceState newState);
		void OnDeviceAdded(string deviceId);
		void OnDeviceRemoved(string deviceId);
		void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId);
		void OnPropertyValueChanged(string deviceId, PropertyKey key);
	}
}
