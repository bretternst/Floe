using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Floe.UI.Interop
{
	public class NotifyIcon : IDisposable
	{
		private const int CallbackMessage = 0x5700;
		private const int WM_LBUTTONDBLCLK = 0x203;
		private const int WM_RBUTTONDOWN = 0x204;

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

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		private static int _id = 1;
		private NotifyIconData _data;
		private HwndSource _src;

		public bool IsVisible { get; private set; }

		public event EventHandler DoubleClicked;
		public event EventHandler RightClicked;

		public NotifyIcon(Window parent, System.Drawing.Icon icon)
		{
			_data = new NotifyIconData();
			_data.cbSize = Marshal.SizeOf(typeof(NotifyIconData));
			_data.uID = _id++;
			_data.uFlags = 0x8 | 0x2 | 0x1;
			_data.dwState = 0x0;
			_data.hIcon = icon.Handle;
			_data.hWnd = new WindowInteropHelper(parent).Handle;
			_data.uCallbackMessage = CallbackMessage;
			_src = HwndSource.FromHwnd(_data.hWnd);
			_src.AddHook(new HwndSourceHook(WndProc));
			Shell_NotifyIcon(0x0, ref _data);
			this.IsVisible = true;
		}

		public void Show()
		{
			this.Show(null, null);
		}

		public void Show(string balloonTitle, string balloonMessage)
		{
			_data.dwState = 0x0;
			_data.dwStateMask = 0x1;
			if (!string.IsNullOrEmpty(balloonTitle) && !string.IsNullOrEmpty(balloonMessage))
			{
				_data.uFlags |= 0x10;
				_data.szInfo = balloonMessage;
				_data.szInfoTitle = balloonTitle;
				_data.uTimeoutOrVersion = 0;
				_data.dwInfoFlags = 0x1;
			}
			else
			{
				_data.uFlags &= ~(uint)0x10;
			}
			Shell_NotifyIcon(0x1, ref _data);
			this.IsVisible = true;
		}

		public void Hide()
		{
			_data.dwState = 0x1;
			_data.dwStateMask = 0x1;
			Shell_NotifyIcon(0x1, ref _data);
			this.IsVisible = false;
		}

		public void Dispose()
		{
			Shell_NotifyIcon(0x2, ref _data);
			_src.RemoveHook(new HwndSourceHook(WndProc));
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == CallbackMessage && (int)wParam == _data.uID)
			{
				switch ((int)lParam)
				{
					case WM_LBUTTONDBLCLK:
						{
							var handler = this.DoubleClicked;
							if (handler != null)
							{
								handler(this, EventArgs.Empty);
							}
						}
						break;
					case WM_RBUTTONDOWN:
						{
							var handler = this.RightClicked;
							if (handler != null)
							{
								handler(this, EventArgs.Empty);
							}

							SetForegroundWindow(_src.Handle);
						}
						break;
				}
			}

			return IntPtr.Zero;
		}
	}
}
