using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
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
		void GetDevice([MarshalAs(UnmanagedType.LPWStr)] string deviceId, out IMMDevice device);
		void RegisterEndpointNotificationCallback(IMMNotificationClient client);
		void UnregisterEndpointNotificationCallback(IMMNotificationClient client);
	}

	class MMNotificationEventArgs : EventArgs
	{
		public DataFlow DataFlow { get; private set; }
		public Role Role { get; private set; }

		public MMNotificationEventArgs(DataFlow dataFlow, Role role)
		{
			this.DataFlow = dataFlow;
			this.Role = role;
		}
	}

	class MMDeviceEnumerator : IMMDeviceEnumerator, IMMNotificationClient
	{
		[ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
		private class CoClass
		{
		}

		private static Lazy<MMDeviceEnumerator> _current = new Lazy<MMDeviceEnumerator>(() =>
			new MMDeviceEnumerator(new CoClass() as IMMDeviceEnumerator), false);

		public static MMDeviceEnumerator Current { get { return _current.Value; } }

		public event EventHandler<MMNotificationEventArgs> DefaultDeviceChanged;

		private IMMDeviceEnumerator _inner;

		private MMDeviceEnumerator(IMMDeviceEnumerator inner)
		{
			_inner = inner;
			this.RegisterEndpointNotificationCallback(this);
		}

		~MMDeviceEnumerator()
		{
			this.UnregisterEndpointNotificationCallback(this);
		}

		public void EnumAudioEndpoints(DataFlow dataFlow, DeviceState deviceStates, out IMMDeviceCollection devices)
		{
			_inner.EnumAudioEndpoints(dataFlow, deviceStates, out devices);
		}

		public void GetDefaultAudioEndpoint(DataFlow dataFlow, Role role, out IMMDevice device)
		{
			_inner.GetDefaultAudioEndpoint(dataFlow, role, out device);
		}

		public void GetDevice(string deviceId, out IMMDevice device)
		{
			_inner.GetDevice(deviceId, out device);
		}

		public void RegisterEndpointNotificationCallback(IMMNotificationClient client)
		{
			_inner.RegisterEndpointNotificationCallback(client);
		}

		public void UnregisterEndpointNotificationCallback(IMMNotificationClient client)
		{
			_inner.UnregisterEndpointNotificationCallback(client);
		}

		public void OnDeviceStateChanged(string deviceId, DeviceState newState)
		{
		}

		public void OnDeviceAdded(string deviceId)
		{
		}

		public void OnDeviceRemoved(string deviceId)
		{
		}

		public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
		{
			var handler = this.DefaultDeviceChanged;
			if (handler != null)
			{
				handler(this, new MMNotificationEventArgs(flow, role));
			}
		}

		public void OnPropertyValueChanged(string deviceId, PropertyKey key)
		{
		}
	}
}
