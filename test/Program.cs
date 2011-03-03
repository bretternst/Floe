using System;
using System.Net;
using System.Net.Sockets;

using Floe.Net;

namespace test
{
	class Program
	{
		static void Main(string[] args)
		{
			var stun = new StunUdpClient("stun01.sipphone.com");
			var ar = stun.BeginGetEndPoint(null, null);
			var si = stun.EndGetEndPoint(ar);
			Console.WriteLine(si.LocalPort + " " + si.PublicEndPoint.ToString());
			Console.ReadLine();
		}
	}
}
