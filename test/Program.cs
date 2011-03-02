using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

using Floe.Audio;

namespace test
{
	class MemoryCapture : AudioCaptureClient
	{
		public MemoryCapture(AudioDevice device)
			: base(device, 195, new WaveFormatPcm(Program.SampleRate, 16, 1), new WaveFormatGsm610(Program.SampleRate))
		{
		}

		protected override void OnWritePacket(IntPtr buffer)
		{
			int pos = Program.WritePos + 1;
			if(pos == Program.Packets.Length)
			{
				pos = 0;
			}
			Marshal.Copy(buffer, Program.Packets[pos], 0, this.PacketSize);
			Program.WritePos = pos;
		}
	}

	class MemoryRender : AudioRenderClient
	{
		public MemoryRender(AudioDevice device)
			: base(device, 195, new WaveFormatGsm610(Program.SampleRate), new WaveFormatPcm(Program.SampleRate, 16, 1))
		{
		}

		protected override bool OnReadPacket(IntPtr buffer)
		{
			int pos = Program.ReadPos;
			if (pos == Program.WritePos)
			{
				return false;
			}
			pos++;
			if (pos == Program.Packets.Length)
			{
				pos = 0;
			}
			Marshal.Copy(Program.Packets[pos], 0, buffer, this.PacketSize);
			Program.ReadPos = pos;
			return true;
		}
	}

	class Program
	{
		public const int SampleRate = 21760;
		public static byte[][] Packets = new byte[12][];
		public volatile static int ReadPos = -1;
		public volatile static int WritePos = -1;

		static void Main(string[] args)
		{
			var capture = new MemoryCapture(AudioDevice.DefaultCaptureDevice);
			var render = new MemoryRender(AudioDevice.DefaultRenderDevice);
			for (int i = 0; i < Packets.Length; i++)
			{
				Packets[i] = new byte[capture.PacketSize];
			}
			capture.Start();
			System.Threading.Thread.Sleep(100);
			render.Start();
			while (true)
			{
				System.Threading.Thread.Sleep(50);
				if (ReadPos > WritePos)
				{
					Console.WriteLine(WritePos + Packets.Length - ReadPos);
				}
				else
				{
					Console.WriteLine(WritePos - ReadPos);
				}
			}
			Console.ReadLine();

			capture.Stop();
			render.Stop();
		}
	}
}
