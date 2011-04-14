using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.InteropServices;

using Floe.Audio;
using Floe.Interop;

namespace test
{
	class Program
	{
		[Flags]
		private enum KeyStates
		{
			None = 0,
			Down = 1,
			Toggled = 2
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		private static extern short GetKeyState(int keyCode);

		static void Main(string[] args)
		{
			int sampleRate = 21760;
			var client = new VoiceClient(new CodecInfo(VoiceCodec.Gsm610, sampleRate), null,
				() =>
				{
					return true;
					return (GetKeyState(0x41) & 0x8000) > 0;
				});
			client.AddPeer(VoiceCodec.Gsm610, sampleRate, new IPEndPoint(Dns.GetHostEntry("spoon.failurefiles.com").AddressList[0], 57222));
			client.Open();
			Console.ReadLine();
			client.InputGain = 10;
			Console.ReadLine();
		}
	}
}
