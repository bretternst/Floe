using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using Floe.Audio.Interop;

namespace Floe.Audio
{
	public class AudioInputClient : AudioClient
	{
		private IAudioCaptureClient _capture;

		internal AudioInputClient(IAudioClient client, WaveFormat format)
			: base(client, format)
		{
			_capture = this.GetService<IAudioCaptureClient>();
		}

		protected override void Loop(object state)
		{
			var args = (LoopArgs)state;

			var handles = new[] { this.BufferEvent, args.CancelToken.WaitHandle };

			this.Client.Start();

			try
			{
				while (true)
				{
					switch (WaitHandle.WaitAny(handles))
					{
						case 0:
							this.ReadBuffer(args.Stream);
							break;
						case 1:
							return;
					}
				}
			}
			finally
			{
				this.Client.Stop();
			}
		}

		private void ReadBuffer(Stream stream)
		{
			IntPtr p;
			int count;
			long devicePosition;
			long qpcPosition;
			AudioClientBufferFlags flags;
			_capture.GetBuffer(out p, out count, out flags, out devicePosition, out qpcPosition);
			Marshal.Copy(p, this.Buffer, 0, count * this.FrameSize);
			_capture.ReleaseBuffer(count);

			stream.Write(this.Buffer, 0, count * this.FrameSize);
		}
	}
}
