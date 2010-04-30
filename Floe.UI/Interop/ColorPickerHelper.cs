using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Floe.UI.Interop
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct CHOOSECOLOR
	{
		public int lStructSize;
		public IntPtr hwndOwner;
		public IntPtr hInstance;
		public uint rgbResult;
		public IntPtr lpCustColors;
		public int Flags;
		public Int32 lCustData;
		public IntPtr lpfnHook;
		public IntPtr lpTemplateName;
	}

	public static class ColorPickerHelper
	{
		[DllImport("comdlg32.dll")]
		private static extern bool ChooseColor(ref CHOOSECOLOR pChooseColor);

		public static bool PickColor(Window owner, ref Color color)
		{
			var cs = new CHOOSECOLOR();
			cs.lStructSize = Marshal.SizeOf(typeof(CHOOSECOLOR));
			cs.lpCustColors = Marshal.AllocHGlobal(64);
			Marshal.Copy(new byte[64], 0, cs.lpCustColors, 64);
			cs.hwndOwner = new WindowInteropHelper(owner).Handle;
			cs.rgbResult = (uint)color.R << 16 | (uint)color.G << 8 | (uint)color.B;

			byte[] buf;

			try
			{
				string s = App.Preferences.CustomColors;
				if (!string.IsNullOrEmpty(s))
				{
					buf = Convert.FromBase64String(s);
					Marshal.Copy(buf, 0, cs.lpCustColors, 64);
				}
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Loading custom colors failed: " + ex.Message);
			}

			bool result = ChooseColor(ref cs);
			if (result)
			{
				color = Color.FromRgb((byte)(cs.rgbResult >> 16),
					(byte)(cs.rgbResult >> 8),
					(byte)(cs.rgbResult));
			}

			buf = new byte[64];
			Marshal.Copy(cs.lpCustColors, buf, 0, 64);
			Marshal.FreeHGlobal(cs.lpCustColors);
			App.Preferences.CustomColors = Convert.ToBase64String(buf);

			Marshal.FreeHGlobal(cs.lpCustColors);

			return true;
		}
	}
}
