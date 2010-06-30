using System;

namespace Floe.UI.Interop
{
	public static class WindowConstants
	{
		public enum HitTestValues
		{
			HTERROR = -2,
			HTTRANSPARENT = -1,
			HTNOWHERE = 0,
			HTCLIENT = 1,
			HTCAPTION = 2,
			HTSYSMENU = 3,
			HTGROWBOX = 4,
			HTMENU = 5,
			HTHSCROLL = 6,
			HTVSCROLL = 7,
			HTMINBUTTON = 8,
			HTMAXBUTTON = 9,
			HTLEFT = 10,
			HTRIGHT = 11,
			HTTOP = 12,
			HTTOPLEFT = 13,
			HTTOPRIGHT = 14,
			HTBOTTOM = 15,
			HTBOTTOMLEFT = 16,
			HTBOTTOMRIGHT = 17,
			HTBORDER = 18,
			HTOBJECT = 19,
			HTCLOSE = 20,
			HTHELP = 21
		}

		public const int WM_NCHITTEST = 0x0084;
		public const int WM_GETMINMAXINFO = 0x0024;
		public const int WM_SIZE = 0x0005;
		public const int WM_QUERYENDSESSION = 0x0011;
		public const int WM_ENDSESSION = 0x0016;
	}
}
