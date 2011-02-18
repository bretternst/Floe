using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Floe.Audio.Interop;

namespace Floe.Audio
{
	public static class Test
	{
		public static void Run()
		{
			var device = AudioDevice.DefaultOutputDevice;
			var device2 = AudioDevice.DefaultInputDevice;
			device.Initialize(AudioChannels.Stereo, 44100, BitsPerSample.Sixteen);
			device2.Initialize(AudioChannels.Mono, 44100, BitsPerSample.Sixteen);

			var os = device.GetOutputStream();
			var os2 = device2.GetInputStream();
		}
	}
}
