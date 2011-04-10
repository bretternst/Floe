using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.InteropServices;

using Floe.Audio;
using Floe.Interop;

namespace test
{
	unsafe class Program : Stream
	{
		static byte[] Buffer = new byte[640];

		static void Main(string[] args)
		{
			var con = new AudioConverter(640, new WaveFormatPcm(21760, 16, 1), new WaveFormatGsm610(21760));
			fixed (byte* p = &Buffer[0])
			{
				var ip = (IntPtr)p;
				IntPtr op;
				int count = con.Convert(ip, 640, &op);
				Marshal.Copy(op, Buffer, 0, count);
			}

			var wavOut = new WaveOut(new Program(), new WaveFormatGsm610(21760), 65);
			wavOut.Start();
			Console.ReadLine();
		}

		public override bool CanRead
		{
			get { throw new NotImplementedException(); }
		}

		public override bool CanSeek
		{
			get { throw new NotImplementedException(); }
		}

		public override bool CanWrite
		{
			get { throw new NotImplementedException(); }
		}

		public override void Flush()
		{
			throw new NotImplementedException();
		}

		public override long Length
		{
			get { throw new NotImplementedException(); }
		}

		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			Array.Copy(Buffer, 0, buffer, 0, count);
			Console.WriteLine(count);
			return count;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}
	}
}
