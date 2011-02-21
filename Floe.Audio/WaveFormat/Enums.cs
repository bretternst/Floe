using System;

namespace Floe.Audio
{
	public enum WaveEncoding : short
	{
		Pcm = 0x1,
		Adpcm = 0x2,
		MpegLayer3 = 0x55
	}
}
