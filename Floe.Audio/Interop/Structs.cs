using System;
using System.Runtime.InteropServices;

namespace Floe.Audio.Interop
{
	struct WaveFormat
	{
		public const ushort Pcm = 0x0001;

		public ushort FormatTag;
		public ushort Channels;
		public uint SamplesPerSecond;
		public uint AvgBytesPerSecond;
		public ushort BlockAlign;
		public ushort BitsPerSample;
		public ushort Size;

		public WaveFormat(ushort channels, uint samplesPerSecond, ushort bitsPerSample)
		{
			this.FormatTag = WaveFormat.Pcm;
			this.Channels = channels;
			this.SamplesPerSecond = samplesPerSecond;
			this.BitsPerSample = bitsPerSample;
			this.BlockAlign = (ushort)((channels * bitsPerSample) / 8);
			this.AvgBytesPerSecond = samplesPerSecond * this.BlockAlign;
			this.Size = 0;
		}
	}

	struct PropertyKey
	{
		public Guid FormatId;
		public int PropertyId;

		public PropertyKey(Guid fmtId, int id)
		{
			this.FormatId = fmtId;
			this.PropertyId = id;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	struct PropertyVariant
	{
		[FieldOffset(0)]
		private ushort type;
		[FieldOffset(2)]
		private ushort reserved1;
		[FieldOffset(4)]
		private ushort reserved2;
		[FieldOffset(6)]
		private ushort reserved3;

		[FieldOffset(8)]
		private byte i1Value;
		[FieldOffset(8)]
		private sbyte ui1Value;
		[FieldOffset(8)]
		private short i2Value;
		[FieldOffset(8)]
		private ushort ui2Value;
		[FieldOffset(8)]
		private int i4Value;
		[FieldOffset(8)]
		private uint ui4Value;
		[FieldOffset(8)]
		private long i8Value;
		[FieldOffset(8)]
		private ulong ui8Value;
		[FieldOffset(8)]
		private float r4Value;
		[FieldOffset(8)]
		private double r8Value;
		[FieldOffset(8)]
		private DateTime dateValue;
		[FieldOffset(8)]
		private bool boolValue;
		[FieldOffset(8)]
		private IntPtr ptrValue;
		[FieldOffset(12)]
		private IntPtr dataValue;

		public VarEnum Type { get { return (VarEnum)type; } }

		public object Value
		{
			get
			{
				return this.GetValue();
			}
		}

		private object GetValue()
		{
			switch ((VarEnum)type)
			{
				case VarEnum.VT_EMPTY:
				case VarEnum.VT_NULL:
					return null;
				case VarEnum.VT_I1:
					return i1Value;
				case VarEnum.VT_I2:
					return i2Value;
				case VarEnum.VT_INT:
				case VarEnum.VT_I4:
				case VarEnum.VT_HRESULT:
					return i4Value;
				case VarEnum.VT_I8:
					return i8Value;
				case VarEnum.VT_UI1:
					return ui1Value;
				case VarEnum.VT_UI2:
					return ui2Value;
				case VarEnum.VT_UI4:
				case VarEnum.VT_UINT:
					return ui4Value;
				case VarEnum.VT_UI8:
					return ui8Value;
				case VarEnum.VT_BOOL:
					return boolValue;
				case VarEnum.VT_DATE:
					return dateValue;
				case VarEnum.VT_PTR:
					return ptrValue;
				case VarEnum.VT_R4:
					return r4Value;
				case VarEnum.VT_R8:
					return r8Value;
				case VarEnum.VT_BSTR:
					return Marshal.PtrToStringBSTR(ptrValue);
				case VarEnum.VT_LPSTR:
					return Marshal.PtrToStringAnsi(ptrValue);
				case VarEnum.VT_LPWSTR:
					return Marshal.PtrToStringUni(ptrValue);
				case VarEnum.VT_BLOB:
					var blob = new byte[i4Value];
					Marshal.Copy(dataValue, blob, 0, i4Value);
					return blob;
				default:
					throw new NotImplementedException("No support for variant type: " + type.ToString());
			}
		}
	}
}
