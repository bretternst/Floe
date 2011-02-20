using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	[StructLayout(LayoutKind.Sequential)]
	class MediaType
	{
		public Guid MajorType;
		public Guid SubType;
		public bool FixedSizeSamples;
		public bool TemporalCompression;
		public int SampleSize;
		public Guid FormatType;
		private object Unused1;
		public int FormatSize;
		public IntPtr Format;

		public void Free()
		{
			if (this.Format != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(this.Format);
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
	struct PinInfo
	{
		public IBaseFilter Filter;
		public PinDirection Direction;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
		private string RawName;

		public string Name { get { return this.RawName.Split('\0')[0]; } }
	}

	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
	struct FilterInfo
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
		public string RawName;
		public IFilterGraph graph;

		public string Name { get { return this.RawName.Split('\0')[0]; } }
	}
}
