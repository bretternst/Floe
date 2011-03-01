using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

using Floe.Audio;

namespace test
{
	class MemoryCapture : VoiceCaptureClient
	{
		private MemoryStream _stream;
		private byte[] _buffer;

		public MemoryCapture(AudioDevice device, MemoryStream stream)
			: base(device)
		{
			_stream = stream;
			_buffer = new byte[this.PacketSize];
		}

		protected override void OnWritePacket(IntPtr buffer)
		{
			Marshal.Copy(buffer, _buffer, 0, this.PacketSize);
			_stream.Write(_buffer, 0, this.PacketSize);
		}
	}

	class MemoryRender : VoiceRenderClient
	{
		private MemoryStream _stream;
		private byte[] _buffer;

		public MemoryRender(AudioDevice device, MemoryStream stream)
			: base(device)
		{
			_stream = stream;
			_buffer = new byte[this.BufferSizeInBytes];
		}

		protected override bool OnReadPacket(IntPtr buffer)
		{
			int size = _stream.Read(_buffer, 0, this.PacketSize);
			Marshal.Copy(_buffer, 0, buffer, size);
			return size == this.PacketSize;
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var stream = new MemoryStream();
			var capture = new MemoryCapture(AudioDevice.DefaultCaptureDevice, stream);
			capture.Start();
			Console.ReadLine();
			capture.Stop();
			stream.Position = 0;
			Console.WriteLine(stream.Length);
			var render = new MemoryRender(AudioDevice.DefaultRenderDevice, stream);
			render.Start();
			Console.ReadLine();
			render.Stop();
		}
	}
}
