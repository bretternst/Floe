using System;
using System.Net;
using System.Net.Sockets;

using Floe.Interop;
using Floe.Voice;
using Floe.Net;

namespace test
{
	class Program
	{
		static void Main(string[] args)
		{
			foreach (var ip in Dns.GetHostEntry("").AddressList)
			{
				Console.WriteLine(ip.ToString());
			}
		}
	}
}
