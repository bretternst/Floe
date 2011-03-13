using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.Audio
{
	/// <summary>
	/// Specifies a codec to use for transmitting voice. Currently, only GSM 6.10 is supported.
	/// </summary>
	public enum VoiceCodec
	{
		/// <summary>
		/// Use the GSM 6.10 codec.
		/// </summary>
		Gsm610
	}

	/// <summary>
	/// Specifies the quality level for transmitting voice.
	/// </summary>
	public enum VoiceQuality
	{
		/// <summary>
		/// Low quality. For GSM 6.10, this indicates a sample rate of 8khz.
		/// </summary>
		Low,

		/// <summary>
		/// Medium quality. For GSM 6.10, this indicates a sample rate of 11khz.
		/// </summary>
		Medium,

		/// <summary>
		/// High quality. For GSM 6.10, this indicates a sample rate of 22khz.
		/// </summary>
		High,

		/// <summary>
		/// Ultra-high quality. For GSM 6.10, this indicates a sample rate of 44khz.
		/// </summary>
		Ultra
	}
}
