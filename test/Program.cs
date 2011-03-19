using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

using Floe.Audio;
using Floe.Interop;

namespace test
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var meter = new WaveInMeter(1000))
			{
				meter.LevelUpdated += new EventHandler<WaveLevelEventArgs>(meter_LevelUpdated);
				Console.ReadLine();
			}
		}

		static void meter_LevelUpdated(object sender, WaveLevelEventArgs e)
		{
			Console.WriteLine(e.Level);
		}
	}
}
