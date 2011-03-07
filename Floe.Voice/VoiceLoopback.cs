using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using Floe.Interop;

namespace Floe.Voice
{
	/// <summary>
	/// Allows users to test their microphone by hearing themselves speak. This class encodes the audio
	/// using the selected codec and quality and immediate decodes and plays it back.
	/// </summary>
	public class VoiceLoopback : IDisposable
	{
		private class CaptureClient : AudioCaptureClient
		{
			private ConcurrentStack<byte[]> _pool;
			private ConcurrentQueue<byte[]> _stream;

			public CaptureClient(VoiceCodec codec, VoiceQuality quality, ConcurrentStack<byte[]> pool, ConcurrentQueue<byte[]> stream)
				: base(AudioDevice.DefaultCaptureDevice,
				VoiceSession.GetPacketSize(codec) * 4,
				VoiceSession.GetBufferSize(codec, quality, false) * 4,
				VoiceSession.GetConversions(codec, quality, false))
			{
				_pool = pool;
				_stream = stream;
			}

			protected override void OnWritePacket(IntPtr buffer)
			{
				byte[] packet;
				if (!_pool.TryPop(out packet))
				{
					packet = new byte[this.PacketSize];
				}
				Marshal.Copy(buffer, packet, 0, packet.Length);
				_stream.Enqueue(packet);
			}
		}
		
		private class RenderClient : AudioRenderClient
		{
			private ConcurrentStack<byte[]> _pool;
			private ConcurrentQueue<byte[]> _stream;

			public RenderClient(VoiceCodec codec, VoiceQuality quality, ConcurrentStack<byte[]> pool, ConcurrentQueue<byte[]> stream)
				: base(AudioDevice.DefaultRenderDevice,
				VoiceSession.GetPacketSize(codec) * 4,
				VoiceSession.GetBufferSize(codec, quality, true) * 4,
				VoiceSession.GetConversions(codec, quality, true))
			{
				_pool = pool;
				_stream = stream;
			}

			protected override bool OnReadPacket(IntPtr buffer)
			{
				byte[] packet;
				if (_stream.TryDequeue(out packet))
				{
					Marshal.Copy(packet, 0, buffer, packet.Length);
					_pool.Push(packet);
					return true;
				}
				return false;
			}
		}

		private CaptureClient _capture;
		private RenderClient _render;

		/// <summary>
		/// Construct a new voice loopback session.
		/// </summary>
		/// <param name="codec">An audio codec to encode and decode with.</param>
		/// <param name="quality">The codec-specific quality level.</param>
		public VoiceLoopback(VoiceCodec codec, VoiceQuality quality)
		{
			var pool = new ConcurrentStack<byte[]>();
			var stream = new ConcurrentQueue<byte[]>();
			_capture = new CaptureClient(codec, quality, pool, stream);
			_render = new RenderClient(codec, quality, pool, stream);
		}

		/// <summary>
		/// Gets or sets the capture volume.
		/// </summary>
		public float CaptureVolume { get { return _capture.Volume; } set { _capture.Volume = value; } }

		/// <summary>
		/// Gets or sets the render volume.
		/// </summary>
		public float RenderVolume { get { return _render.Volume; } set { _render.Volume = value; } }

		/// <summary>
		/// Starts the loopback session.
		/// </summary>
		public void Start()
		{
			_capture.Start();
			_render.Start();
		}

		/// <summary>
		/// Stops the loopback session.
		/// </summary>
		public void Stop()
		{
			_capture.Stop();
			_render.Stop();
		}

		/// <summary>
		/// Dispose the loopback session.
		/// </summary>
		public void Dispose()
		{
			_capture.Dispose();
			_render.Dispose();
		}
	}
}
