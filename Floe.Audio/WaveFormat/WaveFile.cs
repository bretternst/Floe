using System;
using System.IO;

namespace Floe.Audio
{
	public static class WaveFile
	{
		public static WaveFormat Read(Stream stream)
		{
			var br = new BinaryReader(stream);
			int dataSize;
			WaveFormat format;

			try
			{
				if (br.ReadInt32() != 0x46464952 || // "RIFF"
					br.ReadInt32() == 0 || // total size
					br.ReadInt32() != 0x45564157 || // "WAVE"
					br.ReadInt32() != 0x20746d66) // "fmt "
				{
					throw new WaveFormatException("Not a valid wave file.");
				}

				int fmtSize = br.ReadInt32();
				format = new WaveFormatFull(br, fmtSize);

				while (true)
				{
					if (br.ReadInt32() == 0x61746164)
					{
						dataSize = br.ReadInt32();
						break;
					}
					br.ReadBytes(br.ReadInt32());
				}
			}
			catch (EndOfStreamException)
			{
				throw new WaveFormatException("Not a valid wave file.");
			}

			return format;
		}
	}
}
