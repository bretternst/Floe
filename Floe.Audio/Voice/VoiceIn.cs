using System;
using System.IO;

using Floe.Interop;
using Floe.Net;

namespace Floe.Audio
{
	class VoiceIn : Stream
	{
		private CodecInfo _codec;
		private AudioConverter _encoder;
		private RtpClient _client;
		private TransmitPredicate _predicate;
		private int _timeStamp;
		private WaveIn _waveIn;

		public VoiceIn(CodecInfo codec, RtpClient client, TransmitPredicate predicate)
		{
			_codec = codec;
			_client = client;
			_predicate = predicate;			
			this.InitAudio();
		}

		public float Level { get; private set; }

		public float Gain { get; set; }

		public void Start()
		{
			_waveIn.Start();
		}

		public override void Close()
		{
			base.Close();
			_waveIn.Dispose();
		}

		private void InitAudio()
		{
			if (_waveIn != null)
			{
				_waveIn.Close();
			}
			_waveIn = new WaveIn(this, _codec.DecodedFormat, _codec.DecodedBufferSize);
			_encoder = new AudioConverter(_codec.DecodedBufferSize, _codec.DecodedFormat, _codec.EncodedFormat);
		}

		public override bool CanRead { get { return true; } }
		public override bool CanSeek { get { return false; } }
		public override bool CanWrite { get { return false; } }
		public override void Flush() { throw new NotImplementedException(); }
		public override long Length { get { throw new NotImplementedException(); } }
		public override long Position { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		public override long Seek(long offset, SeekOrigin origin) { throw new NotImplementedException(); }
		public override void SetLength(long value) { throw new NotImplementedException(); }
		public override int Read(byte[] buffer, int offset, int count) { throw new NotImplementedException(); }

		public override void Write(byte[] buffer, int offset, int count)
		{
			float gain = this.Gain != 0f ? (float)Math.Pow(10, this.Gain / 20f) : 1f;
			this.Level = WavProcess.ApplyGain(gain, buffer, count);

			if (_client != null)
			{
				count = _encoder.Convert(buffer, count, buffer);
				if (count >= _client.PayloadSize && (_predicate == null || _predicate()))
				{
					_client.Send(_timeStamp, buffer);
				}
			}
			_timeStamp += _codec.SamplesPerPacket;
		}
	}
}
