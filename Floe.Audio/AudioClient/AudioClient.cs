using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using Floe.Audio.Interop;

namespace Floe.Audio
{
	public abstract class AudioClient
	{
		protected struct LoopArgs
		{
			public CancellationToken CancelToken;
			public Stream Stream;
		}

		private Task _task;
		private CancellationTokenSource _cts;

		public WaveFormat Format { get; private set; }
		protected EventWaitHandle BufferEvent { get; private set; }
		protected int BufferSize { get; private set; }
		internal IAudioClient Client { get; private set; }
		protected int FrameSize { get { return this.Format.BlockAlign; } }
		protected byte[] Buffer { get; private set; }

		internal AudioClient(IAudioClient client, WaveFormat format)
		{
			this.Client = client;
			this.Format = format;

			var sessionId = Guid.Empty;

			if (format.Encoding != WaveEncoding.Pcm)
			{
				throw new NotSupportedException("Only PCM formats are supported.");
			}

			this.Client.Initialize(AudioShareMode.Shared, AudioStreamFlags.EventCallback, 0, 0, format, ref sessionId);
			this.BufferEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
			this.Client.SetEventHandle(this.BufferEvent.SafeWaitHandle.DangerousGetHandle());
			int bufSize;
			this.Client.GetBufferSize(out bufSize);
			this.BufferSize = bufSize;
			this.Buffer = new byte[bufSize * this.FrameSize];
		}

		public void Start(Stream stream)
		{
			if (_task != null && (_task.Status == TaskStatus.Running || _task.Status == TaskStatus.WaitingForActivation ||
				_task.Status == TaskStatus.WaitingToRun))
			{
				throw new InvalidOperationException("The client has already been started.");
			}

			_cts = new CancellationTokenSource();
			_task = Task.Factory.StartNew(Loop, new LoopArgs { CancelToken = _cts.Token, Stream = stream });
		}

		public void Stop()
		{
			_cts.Cancel();
			_task.Wait();
		}

		protected virtual void Loop(object state)
		{
		}

		protected T GetService<T>() where T : class
		{
			var t = typeof(T);
			if (t.IsInterface)
			{
				var attr = t.GetCustomAttributes(typeof(GuidAttribute), false).FirstOrDefault() as GuidAttribute;
				if (attr != null)
				{
					var iid = new Guid(attr.Value);
					object obj;
					this.Client.GetService(ref iid, out obj);
					return obj as T;
				}
			}
			return null;
		}
	}
}
