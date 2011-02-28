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

		public MemoryCapture(MemoryStream stream)
		{
			_stream = stream;
			_buffer = new byte[this.VoiceBufferSize];
		}

		protected override void OnPacketReady(int size, IntPtr buffer)
		{
			Marshal.Copy(buffer, _buffer, 0, size);
			_stream.Write(_buffer, 0, size);
		}
	}

	class MemoryRender : VoiceRenderClient
	{
		private MemoryStream _stream;
		private byte[] _buffer;

		public MemoryRender(MemoryStream stream)
		{
			_stream = stream;
			_buffer = new byte[this.BufferSizeInBytes];
		}

		protected override int OnPacketNeeded(int size, IntPtr buffer)
		{
			size = _stream.Read(_buffer, 0, size);
			Marshal.Copy(_buffer, 0, buffer, size);
			return size;
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var stream = new MemoryStream();
			var capture = new MemoryCapture(stream);
			capture.Start();
			Console.ReadLine();
			capture.Stop();
			stream.Position = 0;
			var render = new MemoryRender(stream);
			render.Start();
			Console.ReadLine();
			render.Stop();
		}
	}
}
