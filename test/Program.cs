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
			using (var voice = new VoiceSession(VoiceCodec.Gsm610, VoiceQuality.High))
			{
				voice.AddPeer(VoiceCodec.Gsm610, VoiceQuality.High,
					new IPEndPoint(Dns.GetHostEntry("spoon.failurefiles.com").AddressList[0], 57222));
				voice.Open();
				Console.ReadLine();
			}
		}
	}
}
