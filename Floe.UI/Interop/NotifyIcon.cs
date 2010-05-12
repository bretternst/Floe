using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace Floe.UI.Interop
{
	public class NotifyIcon : IDisposable
	{
		[StructLayout(LayoutKind.Sequential)]
		private struct NotifyIconData
		{
			public int cbSize;
			public IntPtr hWnd;
			public int uID;
			public uint uFlags;
			public int uCallbackMessage;
			public IntPtr hIcon;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string szTip;
			public int dwState;
			public int dwStateMask;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string szInfo;
			public int uTimeoutOrVersion;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
			public string szInfoTitle;
			public int dwInfoFlags;
		}

		[DllImport("shell32.dll")]
		private static extern bool Shell_NotifyIcon(uint dwMessage, [In] ref NotifyIconData pnid);

		private static int _id = 1;
		private NotifyIconData _data;
		private HwndSource _src;

		public event EventHandler DoubleClicked;

		public NotifyIcon(Window parent, System.Drawing.Icon icon)
		{
			_data = new NotifyIconData();
			_data.cbSize = Marshal.SizeOf(typeof(NotifyIconData));
			_data.uID = _id++;
			_data.uFlags = 0x8 | 0x2 | 0x1;
			_data.dwState = 0x0;
			_data.hIcon = icon.Handle;
			_data.hWnd = new WindowInteropHelper(parent).Handle;
			_data.uCallbackMessage = 0x5700;
			_src = HwndSource.FromHwnd(_data.hWnd);
			_src.AddHook(new HwndSourceHook(WndProc));
			Shell_NotifyIcon(0x0, ref _data);
		}

		public void Show()
		{
			_data.dwState = 0x0;
			_data.dwStateMask = 0x1;
			Shell_NotifyIcon(0x1, ref _data);
		}

		public void Hide()
		{
			_data.dwState = 0x1;
			_data.dwStateMask = 0x1;
			Shell_NotifyIcon(0x1, ref _data);
		}

		public void Dispose()
		{
			Shell_NotifyIcon(0x2, ref _data);
			_src.RemoveHook(new HwndSourceHook(WndProc));
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if(msg == 0x5700 && (int)wParam == _data.uID && (int)lParam == 0x203)
			{
				var handler = this.DoubleClicked;
				if (handler != null)
				{
					handler(this, EventArgs.Empty);
				}
			}

			return IntPtr.Zero;
		}
	}
}
