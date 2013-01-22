﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Floe.UI.Interop;

namespace Floe.UI
{
	public partial class ChatWindow : Window
	{
		public readonly static DependencyProperty UIBackgroundProperty = DependencyProperty.Register("UIBackground",
			typeof(SolidColorBrush), typeof(ChatWindow));
		public SolidColorBrush UIBackground
		{
			get { return (SolidColorBrush)this.GetValue(UIBackgroundProperty); }
			set { this.SetValue(UIBackgroundProperty, value); }
		}

		private const double ResizeHeight = 4.0;
		private const double ResizeWidth = 6.0;
		private bool _isInModalDialog = false, _isShuttingDown = false;
		private NotifyIcon _notifyIcon;
		private WindowState _oldWindowState = WindowState.Normal;
		private IntPtr _hWnd;

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			Interop.WindowHelper.Load(this, App.Settings.Current.Windows.Placement);

			var hwndSrc = PresentationSource.FromVisual(this) as HwndSource;
			hwndSrc.AddHook(new HwndSourceHook(WndProc));
			_hWnd = hwndSrc.Handle;
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case WindowConstants.WM_NCHITTEST:
					{
						var x = (short)(lParam.ToInt32() & 0xFFFF);
						var p = new Point((double)x, lParam.ToInt32() >> 16);
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

						var s = this.InputHitTest(p) as System.Windows.Controls.StackPanel;
						if (s != null && s.TemplatedParent is TabControl)
						{
							htResult = WindowConstants.HitTestValues.HTCAPTION;
						}

						handled = true;
						return (IntPtr)htResult;
					}
				case WindowConstants.WM_GETMINMAXINFO:
					{
						WindowHelper.GetMinMaxInfo(this, _hWnd, lParam);
						handled = true;
					}
					break;
				case WindowConstants.WM_QUERYENDSESSION:
					_isShuttingDown = true;
					handled = true;
					return (IntPtr)1;
				case WindowConstants.WM_ENDSESSION:
					if (wParam == (IntPtr)0)
					{
						_isShuttingDown = false;
					}
					break;
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
			if (this.OwnedWindows.Count == 0 && !_isInModalDialog)
			{
				this.Opacity = App.Settings.Current.Windows.InactiveOpacity;
			}

			base.OnDeactivated(e);
		}

		protected override void OnStateChanged(EventArgs e)
		{
			if (this.WindowState == WindowState.Minimized && App.Settings.Current.Windows.MinimizeToSysTray)
			{
				if (_notifyIcon == null)
				{
					_notifyIcon = new NotifyIcon(this, App.ApplicationIcon);
					_notifyIcon.DoubleClicked += (sender, args) =>
						{
							this.Show();
							this.WindowState = _oldWindowState;
							this.Activate();
							_notifyIcon.Hide();
						};
					_notifyIcon.RightClicked += (sender, args) =>
						{
							var menu = this.FindResource("NotifyMenu") as ContextMenu;
							if (menu != null)
							{
								menu.IsOpen = true;
								System.Windows.Input.CommandManager.InvalidateRequerySuggested();
							}
						};
				}
				this.Hide();
				_notifyIcon.Show();
			}

			base.OnStateChanged(e);
		}

		private bool ConfirmQuit(string text, string caption)
		{
			_isInModalDialog = true;
			bool dontAskAgain = true;
			var result = App.Confirm(this, text, caption, ref dontAskAgain);
			if (dontAskAgain)
			{
				App.Settings.Current.Windows.SuppressWarningOnQuit = true;
			}
			_isInModalDialog = false;
			return result;
		}
	}
}
