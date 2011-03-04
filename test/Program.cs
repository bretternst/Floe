using System;
using System.Net;
using System.Net.Sockets;

using Floe.Net;

namespace test
{
	class VoiceClient : RtpClient
	{
		public VoiceClient()
			: base(3, 130)
		{
		}

		protected override void OnError(Exception ex)
		{
			Console.WriteLine("Error: " + ex.ToString());
		}

		protected override void OnReceived(short payloadType, int seqNumber, int timeStamp, byte[] payload)
		{
			Console.WriteLine(string.Format("type={0} seq={1} time={2} payload={3}", payloadType, seqNumber, timeStamp, payload.Length));
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var payload = new byte[130];

			var rtp1 = new VoiceClient();
			var rtp2 = new VoiceClient();
			rtp1.AddPeer(new IPEndPoint(IPAddress.Loopback, rtp2.LocalEndPoint.Port));
			rtp2.AddPeer(new IPEndPoint(IPAddress.Loopback, rtp1.LocalEndPoint.Port));
			rtp1.Open();
			rtp2.Open();
			int timestamp = 0;
			for(int i = 0; i < 1000; i++)
			{
				System.Threading.Thread.Sleep(1);
				rtp2.Send(timestamp, payload);
				timestamp += 320;
			}
			rtp1.Close();
			rtp2.Close();

			Console.ReadLine();
		}
	}
}
