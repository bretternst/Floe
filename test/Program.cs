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
			FilePlayer.PlayAsync("c:\\test.mp3");
			Console.ReadLine();
		}
	}
}
