using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.Audio
{
	public enum WaveEncoding : short
	{
		Pcm = 0x1,
		Adpcm = 0x2,
		MpegLayer3 = 0x55
	}

	public enum AudioChannels : short
	{
		Mono = 1,
		Stereo = 2
	}

	public enum BitsPerSample : short
	{
		Eight = 8,
		Sixteen = 16
	}
}
