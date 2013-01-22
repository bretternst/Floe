using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.Audio
{
	public static class WavProcess
	{
		public static float ApplyGain(float gain, byte[] buffer, int count)
		{
			float sum = 0f;
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
				sum += (float)Math.Pow(2, (double)sample / (double)short.MaxValue);
			}
			return (float)Math.Sqrt(sum);
		}
	}
}
