using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

using Floe.Net;
using Floe.UI.Interop;

namespace Floe.UI
{
	public partial class ChatWindow : Window
	{
		private const double ResizeTopHeight = 4.0;
		private const double ResizeBottomHeight = 8.0;
		private const double ResizeWidth = 8.0;

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == WindowConstants.WM_NCHITTEST)
			{
				var p = new Point(lParam.ToInt32() & 0xFFFF, lParam.ToInt32() >> 16);
				p = this.PointFromScreen(p);

				var htResult = WindowConstants.HitTestValues.HTCLIENT;

				if (p.X <= ResizeWidth)
				{
					if (p.Y <= ResizeTopHeight)
					{
						htResult = WindowConstants.HitTestValues.HTTOPLEFT;
					}
					else if (this.ActualHeight - p.Y <= ResizeBottomHeight)
					{
						htResult = WindowConstants.HitTestValues.HTBOTTOMLEFT;
					}
					else
					{
						htResult = WindowConstants.HitTestValues.HTLEFT;
					}
				}
				else if (this.ActualWidth - p.X <= ResizeWidth)
				{
					if (p.Y <= ResizeTopHeight)
					{
						htResult = WindowConstants.HitTestValues.HTTOPRIGHT;
					}
					else if (this.ActualHeight - p.Y <= ResizeBottomHeight)
					{
						htResult = WindowConstants.HitTestValues.HTBOTTOMRIGHT;
					}
					else
					{
						htResult = WindowConstants.HitTestValues.HTRIGHT;
					}
				}
				else if (p.Y <= ResizeTopHeight)
				{
					htResult = WindowConstants.HitTestValues.HTTOP;
				}
				else if (this.ActualHeight - p.Y <= ResizeBottomHeight)
				{
					htResult = WindowConstants.HitTestValues.HTBOTTOM;
				}
				else if (p.Y <= grdRoot.RowDefinitions[0].Height.Value &&
					p.X <= grdRoot.ColumnDefinitions[0].ActualWidth)
				{
					htResult = WindowConstants.HitTestValues.HTCAPTION;
				}

				handled = true;
				return (IntPtr)htResult;
			}

			return IntPtr.Zero;
		}

		protected override void OnActivated(EventArgs e)
		{
			this.Opacity = App.Settings.Current.Windows.ActiveOpacity;

			base.OnActivated(e);
		}

		protected override void OnDeactivated(EventArgs e)
		{
			this.Opacity = App.Settings.Current.Windows.InactiveOpacity;

			base.OnDeactivated(e);
		}

		private void btnMinimize_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}

		private void btnMaximize_Click(object sender, RoutedEventArgs e)
		{
			if (this.WindowState == WindowState.Maximized)
			{
				this.WindowState = WindowState.Normal;
			}
			else
			{
				this.WindowState = WindowState.Maximized;
			}
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
