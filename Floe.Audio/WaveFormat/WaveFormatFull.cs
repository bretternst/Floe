using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Floe.Audio
{
	public class WaveFormatFull : WaveFormat
	{
		private const int MaxDataSize = 32;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDataSize)]
		private byte[] _data = new byte[MaxDataSize];

		public byte[] Data { get { return _data; } }

		internal WaveFormatFull(BinaryReader reader, int size)
			: base(reader, Math.Min(size, 16))
		{
			if (size > 18)
			{
				if (size - 18 != this.DataSize)
				{
					throw new WaveFormatException("Custom data size does not match total WaveFormat size.");
				}
				if (this.DataSize > MaxDataSize)
				{
					throw new WaveFormatException("Custom WaveFormat data is too large.");
				}

				if (this.DataSize > 0)
				{
					reader.Read(_data, 0, this.DataSize);
				}
			}
		}
	}
}
