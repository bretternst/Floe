using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Floe.Audio.Interop;

namespace Floe.Audio
{
	public class OutputStream
	{
		private IAudioClient _client;
		private IAudioRenderClient _render;

		internal OutputStream(IAudioClient client)
		{
			_client = client;
			var iid = Interfaces.IAudioRenderClient;
			object obj;
			client.GetService(ref iid, out obj);
			_render = obj as IAudioRenderClient;
			if (_render == null)
			{
				throw new InvalidOperationException("The device does not support audio rendering.");
			}
		}
	}
}
