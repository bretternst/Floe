using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Interop;

using Floe.UI.Interop;

namespace Floe.UI
{
	public partial class ChannelWindow : Window
	{
		public readonly static DependencyProperty UIBackgroundProperty = DependencyProperty.Register("UIBackground",
			typeof(System.Windows.Media.SolidColorBrush), typeof(ChannelWindow));
		public SolidColorBrush UIBackground
		{
			get { return (SolidColorBrush)this.GetValue(UIBackgroundProperty); }
			set { this.SetValue(UIBackgroundProperty, value); }
		}

		private const double ResizeHeight = 4.0;
		private const double ResizeWidth = 6.0;
		private IntPtr _hWnd;

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			var state = App.Settings.Current.Windows.States[this.Control.Context.Key];
			if (!string.IsNullOrEmpty(state.Placement))
			{
				Interop.WindowHelper.Load(this, state.Placement);
			}
			if (!this.IsActive)
			{
				WindowHelper.FlashWindow(this);
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
	}
}
