using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

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

	[StructLayout(LayoutKind.Sequential)]
	public struct MINMAXINFO
	{
		public POINT ptReserved;
		public POINT ptMaxSize;
		public POINT ptMaxPosition;
		public POINT ptMinTrackSize;
		public POINT ptMaxTrackSize;
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct MONITORINFO
	{
		public int cbSize;
		public RECT rcMonitor;
		public RECT rcWork;
		public int dwFlags;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FLASHINFO
	{
		public int cbSize;
		public IntPtr hWnd;
		public int dwFlags;
		public uint uCount;
		public int dwTimeout;
	}

	internal static class WindowHelper
	{
		private const int SW_SHOWNORMAL = 1;
		private const int SW_SHOWMINIMIZED = 2;
		private const int SW_SHOWMINNOACTIVE = 7;

		[DllImport("user32")]
		private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

		[DllImport("user32")]
		private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

		[DllImport("user32")]
		private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

		[DllImport("user32")]
		private static extern IntPtr MonitorFromWindow(IntPtr hWnd, int flags);

		[DllImport("user32")]
		private static extern void FlashWindowEx(ref FLASHINFO pfwi);

		public static void Load(Window window, string placement)
		{
			if (string.IsNullOrEmpty(placement))
			{
				return;
			}

			try
			{
				byte[] buf = Convert.FromBase64String(placement);
				int size = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
				IntPtr p = Marshal.AllocHGlobal(size);
				Marshal.Copy(buf, 0, p, size);
				var wp = (WINDOWPLACEMENT)Marshal.PtrToStructure(p, typeof(WINDOWPLACEMENT));
				Marshal.FreeHGlobal(p);

				wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
				wp.flags = 0;
				wp.showCmd = (wp.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : wp.showCmd);
				if (!window.ShowActivated)
				{
					wp.showCmd = SW_SHOWMINNOACTIVE;
				}
				IntPtr hwnd = new WindowInteropHelper(window).Handle;
				SetWindowPlacement(hwnd, ref wp);
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Window placement failed: " + ex.Message);
			}
		}

		public static string Save(Window window)
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
			return Convert.ToBase64String(buf);
		}

		public static void GetMinMaxInfo(Window window, IntPtr hwnd, IntPtr lParam)
		{
			var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
			var hMonitor = MonitorFromWindow(hwnd, 0x02);
			if (hMonitor != IntPtr.Zero)
			{
				var mi = new MONITORINFO();
				mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
				GetMonitorInfo(hMonitor, ref mi);
				RECT rcWorkArea = mi.rcWork;
				RECT rcMonitorArea = mi.rcMonitor;
				mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
				mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
				mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
				mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);

				mmi.ptMinTrackSize.X = (int)window.MinWidth;
				mmi.ptMinTrackSize.Y = (int)window.MinHeight;
			}

			Marshal.StructureToPtr(mmi, lParam, true);
		}

		public static void FlashWindow(Window window)
		{
			if (window != null)
			{
				var fi = new FLASHINFO();
				fi.cbSize = Marshal.SizeOf(typeof(FLASHINFO));
				fi.hWnd = new WindowInteropHelper(window).Handle;
				fi.dwFlags = 0x2 | 0xc;
				FlashWindowEx(ref fi);
			}
		}
	}
}
