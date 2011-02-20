using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
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

		internal AudioClient(IAudioClient client, WaveFormat format = null)
		{
			this.Client = client;
			this.Format = format;

			if (format == null)
			{
				IntPtr p;
				client.GetMixFormat(out p);
				format = Marshal.PtrToStructure(p, typeof(WaveFormat)) as WaveFormat;
				Marshal.FreeCoTaskMem(p);
				format = new WaveFormat(format.Channels, format.SampleRate, BitsPerSample.Sixteen);
				var sessionId = Guid.Empty;
			}

			this.Format = format;
			try
			{
				var sessionId = Guid.Empty;
				this.Client.Initialize(AudioShareMode.Shared, AudioStreamFlags.EventCallback, 0, 0, format, ref sessionId);
			}
			catch (COMException ex)
			{
				if ((uint)ex.ErrorCode == ResultCodes.AudioClientFormatNotSupported)
				{
					throw new WaveFormatException("Unsupported wave format.");
				}
				throw;
			}

			this.Initialize();
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
			this.Client.Reset();
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

		private void Initialize()
		{
			this.BufferEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
			this.Client.SetEventHandle(this.BufferEvent.SafeWaitHandle.DangerousGetHandle());
			int bufSize;
			this.Client.GetBufferSize(out bufSize);
			this.BufferSize = bufSize;
			this.Buffer = new byte[bufSize * this.FrameSize];
		}
	}
}
