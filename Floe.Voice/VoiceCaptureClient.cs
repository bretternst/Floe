using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Floe.Interop;

namespace Floe.Voice
{
	public class VoiceCaptureClient : AudioCaptureClient
	{
		private const int TrailTime = 100000000;

		private AudioMeter _meter;
		private TransmitPredicate _transmitPredicate;

		public VoiceCaptureClient(AudioDevice device, VoiceCodec codec, VoiceQuality quality, TransmitPredicate transmitPredicate)
			: base(device,
			VoiceSession.GetPacketSize(codec),
			VoiceSession.GetBufferSize(codec, quality, false),
			VoiceSession.GetConversions(codec, quality, false))
		{
			_meter = new AudioMeter(device);
			_transmitPredicate = transmitPredicate;
		}

		protected override void OnCapture(int count, IntPtr buffer)
		{
			if (_transmitPredicate == null || _transmitPredicate(_meter.Peak))
			{
				base.OnCapture(count, buffer);
			}
		}
	}
}
