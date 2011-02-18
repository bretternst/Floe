using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
