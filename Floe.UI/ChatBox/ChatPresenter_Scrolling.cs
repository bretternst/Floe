using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Floe.UI
{
	public partial class ChatPresenter : ChatBoxBase, IScrollInfo
	{
		private double _extentHeight, _offset;

		public bool CanHorizontallyScroll { get { return false; } set { } }
		public bool CanVerticallyScroll { get { return true; } set { } }
		public double ExtentHeight { get { return _extentHeight; } }
		public double ExtentWidth { get { return this.ActualWidth; } }
		public ScrollViewer ScrollOwner { get { return _viewer; } set { _viewer = value; } }
		public double ViewportHeight { get { return this.ActualHeight; } }
		public double ViewportWidth { get { return this.ActualWidth; } }
		public double HorizontalOffset { get { return 0.0; } }
		public double VerticalOffset { get { return _offset; } }

		public void LineUp()
		{
			if (_lastLine != null)
			{
				this.SetVerticalOffset(Math.Max(0.0, _offset - _lastLine.Height));
			}
		}

		public void LineDown()
		{
			if (_lastLine != null && _extentHeight > this.ActualHeight)
			{
				this.SetVerticalOffset(Math.Min(_extentHeight - this.ActualHeight, _offset + _lastLine.Height));
			}
		}

		public void MouseWheelUp()
		{
			if (_lastLine != null)
			{
				this.SetVerticalOffset(Math.Max(0.0, _offset - _lastLine.Height * SystemParameters.WheelScrollLines));
			}
		}

		public void MouseWheelDown()
		{
			if (_lastLine != null && _extentHeight > this.ActualHeight)
			{
				this.SetVerticalOffset(Math.Min(_extentHeight - this.ActualHeight,
					_offset + _lastLine.Height * SystemParameters.WheelScrollLines));
			}
		}

		public void PageUp()
		{
			if (_lastLine != null)
			{
				this.SetVerticalOffset(Math.Max(0.0, _offset - _lastLine.Height * (this.VisibleLineCount - 1)));
			}
		}

		public void PageDown()
		{
			if (_lastLine != null && _extentHeight > this.ActualHeight)
			{
				this.SetVerticalOffset(Math.Min(_extentHeight - this.ActualHeight,
					_offset + _lastLine.Height * (this.VisibleLineCount - 1)));
			}
		}

		public void SetVerticalOffset(double offset)
		{
			_offset = offset;
			this.InvalidateVisual();
			if (_viewer != null)
			{
				_viewer.InvalidateScrollInfo();
			}
		}

		public void LineLeft()
		{
			throw new NotImplementedException();
		}

		public void LineRight()
		{
			throw new NotImplementedException();
		}

		public void PageLeft()
		{
			throw new NotImplementedException();
		}

		public void PageRight()
		{
			throw new NotImplementedException();
		}

		public void MouseWheelLeft()
		{
		}

		public void MouseWheelRight()
		{
		}

		public void SetHorizontalOffset(double offset)
		{
			throw new NotImplementedException();
		}

		public Rect MakeVisible(Visual visual, Rect rectangle)
		{
			throw new NotImplementedException();
		}

		public void ScrollToEnd()
		{
			this.SetVerticalOffset(Math.Max(0.0, this.ExtentHeight - this.ActualHeight));
		}

		public int VisibleLineCount
		{
			get
			{
				return _lastLine != null ? (int)Math.Ceiling(this.ActualHeight / _lastLine.Height) : 0;
			}
		}
	}
}
