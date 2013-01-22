using System;
using System.IO;
using System.Linq;
using Floe.Interop;

namespace Floe.Audio
{
	public class FilePlayer : IDisposable
	{
		private const int WavBufferSamples = 3000;
		private static readonly byte[] WavFileSignature = { 0x52, 0x49, 0x46, 0x46 }; // RIFF
		private static readonly byte[] Mp3FileSignature = { 0x49, 0x44, 0x33 }; // ID3

		private WaveOut _waveOut;

		public event EventHandler Done;

		public FilePlayer(string fileName)
		{
			var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			var sig = new byte[4];
			try
			{
				fileStream.Read(sig, 0, 4);
				fileStream.Seek(0, SeekOrigin.Begin);
				if (WavFileSignature.SequenceEqual(sig.Take(WavFileSignature.Length)))
				{
					var wavStream = new WavFileStream(fileStream);
					_waveOut = new WaveOut(wavStream, wavStream.Format, WavBufferSamples * wavStream.Format.FrameSize);
				}
				else if (Mp3FileSignature.SequenceEqual(sig.Take(Mp3FileSignature.Length)))
				{
					var mp3Stream = new Mp3FileStream(fileStream);
					_waveOut = new WaveOut(mp3Stream, mp3Stream.Format, mp3Stream.Format.BlockSize + 1);
				}
				else
				{
					throw new FileFormatException("Unrecognized file format.");
				}
			}
			catch (EndOfStreamException)
			{
				throw new FileFormatException("Premature end of file.");
			}
			_waveOut.EndOfStream += (sender, e) =>
				{
					var handler = this.Done;
					if(handler != null)
					{
						handler(this, EventArgs.Empty);
					}
				};
		}

		public void Start()
		{
			_waveOut.Start();
		}

		public void Close()
		{
			_waveOut.Close();
		}

		public void Dispose()
		{
			_waveOut.Dispose();
		}

		public static void PlayAsync(string fileName, Action<object> callback = null, object state = null)
		{
			var player = new FilePlayer(fileName);
			player.Done += (sender, e) =>
				{
					if (callback != null)
					{
						callback(state);
					}
					player.Close();
				};
			player.Start();
		}
	}
}
