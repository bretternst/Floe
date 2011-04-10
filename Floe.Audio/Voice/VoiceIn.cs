using System;
using System.IO;

using Floe.Interop;
using Floe.Net;

namespace Floe.Audio
{
	public class VoiceIn : Stream
	{
		private CodecInfo _codec;
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
			_waveIn = new WaveIn(this, _codec.EncodedFormat, _codec.EncodedBufferSize);
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
			if (count >= _client.PayloadSize && (_predicate == null || _predicate()))
			{
				_client.Send(_timeStamp, buffer);
			}
			_timeStamp += _codec.SamplesPerPacket;
		}
	}
}
