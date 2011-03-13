using System;
using System.Net;
using System.Net.Sockets;

using Floe.Interop;
using Floe.Audio;
using Floe.Net;

namespace test
{
	class Program
	{
		static void Main(string[] args)
		{
			FilePlayer.PlayAsync("c:\\test.mp3", (o) => Environment.Exit(0));
			Console.ReadLine();
		}
	}
}
