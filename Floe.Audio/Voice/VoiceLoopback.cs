using System;

using Floe.Interop;

namespace Floe.Audio
{
	/// <summary>
	/// Allows users to test their microphone by hearing themselves speak. This class encodes the audio
	/// using the selected codec and quality and immediate decodes and plays it back.
	/// </summary>
	public class VoiceLoopback : IDisposable
	{
		private WaveIn _waveIn;
		private WaveOut _waveOut;
		private FifoStream _stream;

		/// <summary>
		/// Construct a new voice loopback session.
		/// </summary>
		/// <param name="codec">An audio codec to encode and decode with.</param>
		/// <param name="quality">The codec-specific quality level.</param>
		public VoiceLoopback(VoiceCodec codec, VoiceQuality quality)
		{
			var format = VoiceSession.GetFormat(codec, quality);
			_stream = new FifoStream();
			int bufferSize = VoiceSession.GetBufferSize(codec);
			_waveIn = new WaveIn(_stream, format, bufferSize);
			_waveOut = new WaveOut(_stream, format, bufferSize);
		}

		/// <summary>
		/// Gets or sets the capture volume.
		/// </summary>
//		public float CaptureVolume { get { return _waveIn.Volume; } set { _waveIn.Volume = value; } }

		/// <summary>
		/// Gets or sets the render volume.
		/// </summary>
		public float RenderVolume { get { return _waveOut.Volume; } set { _waveOut.Volume = value; } }

		/// <summary>
		/// Starts the loopback session.
		/// </summary>
		public void Start()
		{
			_waveIn.Start();
			_waveOut.Start();
		}

		/// <summary>
		/// Stops the loopback session.
		/// </summary>
		public void Close()
		{
			_waveIn.Dispose();
			_stream.Dispose();
			_waveOut.Dispose();
		}

		/// <summary>
		/// Dispose the loopback session.
		/// </summary>
		public void Dispose()
		{
			this.Close();
		}
	}
}
