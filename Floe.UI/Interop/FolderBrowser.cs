using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace Floe.UI.Interop
{
	public class FolderBrowser
	{
		private const int MAX_PATH = 260;

		private delegate int BrowseCallbackProc(IntPtr hWnd, int msg, IntPtr lp, IntPtr wp);

		[StructLayout(LayoutKind.Sequential)]
		private struct BROWSEINFO
		{
			public IntPtr hwndOwner;
			public IntPtr pidlRoot;
			public string pszDisplayName;
			public string lpszTitle;
			public uint ulFlags;
			public BrowseCallbackProc lpfn;
			public IntPtr lParam;
			public int iImage;
		}

		[DllImport("shell32.dll")]
		private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		private static extern uint SHGetPathFromIDList(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszPath);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, string lParam);

		public static string Show(Window parent, string caption, string path)
		{
			var sb = new StringBuilder(MAX_PATH);
			var bi = new BROWSEINFO();
			bi.hwndOwner = new WindowInteropHelper(parent).Handle;
			bi.pszDisplayName = path;
			bi.lpszTitle = caption;
			bi.ulFlags = 0x40;
			bi.lpfn = (hWnd, msg, lp, wp) =>
				{
					switch (msg)
					{
						case 0x1:
							SendMessage(new HandleRef(null, hWnd), 0x400 + 103, 1, path);
							break;
					}

					return 0;
				};

			IntPtr pidl = IntPtr.Zero;
			try
			{
				pidl = SHBrowseForFolder(ref bi);
				if (SHGetPathFromIDList(pidl, sb) == 0)
				{
					return null;
				}
			}
			finally
			{
				Marshal.FreeCoTaskMem(pidl);
			}

			return sb.ToString();
		}
	}
}
