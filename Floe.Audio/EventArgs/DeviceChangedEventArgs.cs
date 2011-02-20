using System;

namespace Floe.Audio
{
	public class DeviceChangedEventArgs : EventArgs
	{
		public AudioDevice DefaultDevice { get; private set; }

		public DeviceChangedEventArgs(AudioDevice device)
		{
			this.DefaultDevice = device;
		}
	}
}
