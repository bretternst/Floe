using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Floe.Interop;

namespace Floe.Audio
{
	public class WaveLevelEventArgs : EventArgs
	{
		public float Level { get; internal set; }
	}

	public class WaveInMeter : IDisposable
	{
		private class MeterStream : Stream
		{
			private WaveInMeter _meter;

			public MeterStream(WaveInMeter meter)
			{
				_meter = meter;
			}

			public override bool CanRead { get { return true; } }
			public override bool CanSeek { get { return false; } }
			public override bool CanWrite { get { return true; } }
			public override void Flush() { throw new NotImplementedException(); }
			public override long Length { get { throw new NotImplementedException(); } }
			public override long Position { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
			public override long Seek(long offset, SeekOrigin origin) { throw new NotImplementedException(); }
			public override void SetLength(long value) { throw new NotImplementedException(); }
			public override int Read(byte[] buffer, int offset, int count) { throw new NotImplementedException(); }

			public override void Write(byte[] buffer, int offset, int count)
			{
				float sum = 0f;
				for (int i = 0; i < count; i += 2)
				{
					float sample = (float)((short)(buffer[i + 1] << 8) | (short)(buffer[i])) / (float)short.MaxValue;
					sum += sample * sample;
				}
				sum = (float)Math.Sqrt(sum);
				_meter.OnLevelUpdated(sum);
				return;
			}
		}

		private WaveIn _waveIn;
		private WaveLevelEventArgs _args;

		public WaveInMeter(int samples)
		{
			_args = new WaveLevelEventArgs();
			var format = new WaveFormatPcm(44100, 16, 1);
			_waveIn = new WaveIn(new MeterStream(this), format, format.FrameSize * samples);
			_waveIn.Start();
		}

		public event EventHandler<WaveLevelEventArgs> LevelUpdated;

		public void Dispose()
		{
			_waveIn.Dispose();
		}

		internal void OnLevelUpdated(float level)
		{
			var handler = this.LevelUpdated;
			if (handler != null)
			{
				_args.Level = level;
				handler(this, _args);
			}
		}
	}
}
