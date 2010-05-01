using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows;

namespace Floe.UI.Interop
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;

		public RECT(int left, int top, int right, int bottom)
		{
			this.Left = left;
			this.Top = top;
			this.Right = right;
			this.Bottom = bottom;
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct POINT
	{
		public int X;
		public int Y;

		public POINT(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct WINDOWPLACEMENT
	{
		public int length;
		public int flags;
		public int showCmd;
		public POINT minPosition;
		public POINT maxPosition;
		public RECT normalPosition;
	}

	internal static class WindowPlacementHelper
	{
		private const int SW_SHOWNORMAL = 1;
		private const int SW_SHOWMINIMIZED = 2;

		[DllImport("user32.dll")]
		private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

		[DllImport("user32.dll")]
		private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

		public static void Load(Window window)
		{
			try
			{
				string s = App.Settings.Current.WindowPlacement;
				byte[] buf = Convert.FromBase64String(s);
				int size = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
				IntPtr p = Marshal.AllocHGlobal(size);
				Marshal.Copy(buf, 0, p, size);
				var wp = (WINDOWPLACEMENT)Marshal.PtrToStructure(p, typeof(WINDOWPLACEMENT));
				Marshal.FreeHGlobal(p);

				wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
				wp.flags = 0;
				wp.showCmd = (wp.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : wp.showCmd);
				IntPtr hwnd = new WindowInteropHelper(window).Handle;
				SetWindowPlacement(hwnd, ref wp);
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Window placement failed: " + ex.Message);
			}
		}

		public static void Save(Window window)
		{
			WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
			IntPtr hwnd = new WindowInteropHelper(window).Handle;
			GetWindowPlacement(hwnd, out wp);
			int size = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
			var buf = new byte[size];
			IntPtr p = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(wp, p, true);
			Marshal.Copy(p, buf, 0, size);
			Marshal.FreeHGlobal(p);
			App.Settings.Current.WindowPlacement = Convert.ToBase64String(buf);
		}
	}
}
