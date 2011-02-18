using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Floe.Audio.Interop;

namespace Floe.Audio
{
	public class InputStream
	{
		private IAudioClient _client;
		private IAudioCaptureClient _capture;

		internal InputStream(IAudioClient client)
		{
			_client = client;
			var iid = Interfaces.IAudioCaptureClient;
			object obj;
			client.GetService(ref iid, out obj);
			_capture = obj as IAudioCaptureClient;
			if (_capture == null)
			{
				throw new InvalidOperationException("The device does not support audio capture.");
			}
		}
	}
}
