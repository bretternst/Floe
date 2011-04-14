using System;

using Floe.Interop;

namespace Floe.Audio
{
	/// <summary>
	/// Allows users to test their microphone by hearing themselves speak. This class encodes the audio
	/// using the selected codec and quality and immediate decodes and plays it back.
	/// </summary>
	public class VoiceLoopback : FifoStream, IDisposable
	{
		private WaveIn _waveIn;
		private WaveOut _waveOut;
		private AudioConverter _encoder;

		/// <summary>
		/// Construct a new voice loopback session.
		/// </summary>
		/// <param name="codec">An audio codec to encode and decode with.</param>
		/// <param name="quality">The encoding quality (usually the sample rate).</param>
		public VoiceLoopback(VoiceCodec codec, int quality)
		{
			var info = new CodecInfo(codec, quality);
			_waveIn = new WaveIn(this, info.DecodedFormat, info.DecodedBufferSize);
			_waveOut = new WaveOut(this, info.EncodedFormat, info.EncodedBufferSize);
			_encoder = new AudioConverter(info.DecodedBufferSize, info.DecodedFormat, info.EncodedFormat);
		}

		/// <summary>
		/// Gets or sets the render volume.
		/// </summary>
		public float RenderVolume { get { return _waveOut.Volume; } set { _waveOut.Volume = value; } }

		/// <summary>
		/// Gets or sets the microphone input gain.
		/// </summary>
		public float InputGain { get; set; }

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
		public override void Close()
		{
			base.Close();
			_waveIn.Dispose();
			_waveOut.Dispose();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return base.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			float gain = this.InputGain != 0f ? (float)Math.Pow(10, this.InputGain / 20f) : 1f;
			double min = (double)short.MinValue;
			double max = (double)short.MaxValue; 
			for (int i = 0; i < count; i += 2)
			{
				short sample = BitConverter.ToInt16(buffer, i);
				if (gain != 1f)
				{
					double adj = Math.Max(min, Math.Min(max, (double)sample * gain));
					sample = (short)adj;
					buffer[i] = (byte)sample;
					buffer[i + 1] = (byte)(sample >> 8);
				}
			}

			if (offset != 0)
			{
				throw new ArgumentException("Offsets are not supported.");
			}
			count = _encoder.Convert(buffer, count, buffer);
			base.Write(buffer, offset, count);
		}
	}
}
