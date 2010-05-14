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
using System.Windows.Interop;
using System.Collections.ObjectModel;

using Floe.Net;
using Floe.UI.Interop;
using Floe.Configuration;

namespace Floe.UI
{
	public partial class ChannelWindow : Window
	{
		private const double ResizeHeight = 4.0;
		private const double ResizeWidth = 6.0;
		private NotifyIcon _notifyIcon;
		private WindowState _oldWindowState = WindowState.Normal;
		private IntPtr _hWnd;

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			var placementEl = App.Settings.Current.Windows.States.Get(this.Control.Context.GetKey());
			if (placementEl != null)
			{
				Interop.WindowHelper.Load(this, placementEl.Placement);
			}

			var hwndSrc = PresentationSource.FromVisual(this) as HwndSource;
			hwndSrc.AddHook(new HwndSourceHook(WndProc));
			_hWnd = hwndSrc.Handle;
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == WindowConstants.WM_NCHITTEST)
			{
				var p = new Point(lParam.ToInt32() & 0xFFFF, lParam.ToInt32() >> 16);
				p = this.PointFromScreen(p);

				var htResult = WindowConstants.HitTestValues.HTCLIENT;

				if ((this.ActualWidth - p.X <= ResizeWidth * 2.0 && this.ActualHeight - p.Y <= ResizeHeight) ||
					(this.ActualWidth - p.X <= ResizeWidth && this.ActualHeight - p.Y <= ResizeHeight * 2))
				{
					htResult = WindowConstants.HitTestValues.HTBOTTOMRIGHT;
				}
				else if (p.X <= ResizeWidth)
				{
					if (p.Y <= ResizeHeight)
					{
						htResult = WindowConstants.HitTestValues.HTTOPLEFT;
					}
					else if (this.ActualHeight - p.Y <= ResizeHeight)
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
					if (p.Y <= ResizeHeight)
					{
						htResult = WindowConstants.HitTestValues.HTTOPRIGHT;
					}
					else if (this.ActualHeight - p.Y <= ResizeHeight)
					{
						htResult = WindowConstants.HitTestValues.HTBOTTOMRIGHT;
					}
					else
					{
						htResult = WindowConstants.HitTestValues.HTRIGHT;
					}
				}
				else if (p.Y <= ResizeHeight)
				{
					htResult = WindowConstants.HitTestValues.HTTOP;
				}
				else if (this.ActualHeight - p.Y <= ResizeHeight)
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
			else if (msg == WindowConstants.WM_GETMINMAXINFO)
			{
				WindowHelper.GetMinMaxInfo(this, _hWnd, lParam);
				handled = true;
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
			if (this.OwnedWindows.Count == 0)
			{
				this.Opacity = App.Settings.Current.Windows.InactiveOpacity;
			}

			base.OnDeactivated(e);
		}

		protected override void OnStateChanged(EventArgs e)
		{
			if (this.WindowState == System.Windows.WindowState.Minimized && App.Settings.Current.Windows.MinimizeToSysTray)
			{
				if (_notifyIcon == null)
				{
					_notifyIcon = new NotifyIcon(this, App.ApplicationIcon);
					_notifyIcon.DoubleClicked += (sender, args) =>
					{
						_notifyIcon.Hide();
						this.BeginInvoke(() =>
						{
							this.Show();
							this.WindowState = _oldWindowState;
							this.Activate();
						});
					};
				}
				this.Hide();
				_notifyIcon.Show();
			}

			base.OnStateChanged(e);
		}

		private void btnMinimize_Click(object sender, RoutedEventArgs e)
		{
			var _oldWindowState = this.WindowState;
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

		private void btnSettings_Click(object sender, RoutedEventArgs e)
		{
			App.ShowSettings();
		}
	}
}
