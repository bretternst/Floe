using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Linq;

namespace Floe.UI
{
	public partial class ChatPresenter : ChatBoxBase, IScrollInfo
	{
		private double _extentHeight, _offset;
		private bool _isAutoScrolling = true;

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
			this.SetVerticalOffset(Math.Max(0.0, _offset - _lineHeight));
		}

		public void LineDown()
		{
			if (_extentHeight > this.ActualHeight)
			{
				this.SetVerticalOffset(Math.Min(_extentHeight - this.ActualHeight, _offset + _lineHeight));
			}
		}

		public void MouseWheelUp()
		{
			this.SetVerticalOffset(Math.Max(0.0, _offset - _lineHeight * SystemParameters.WheelScrollLines));
		}

		public void MouseWheelDown()
		{
			if (_extentHeight > this.ActualHeight)
			{
				this.SetVerticalOffset(Math.Min(_extentHeight - this.ActualHeight,
					_offset + _lineHeight * SystemParameters.WheelScrollLines));
			}
		}

		public void PageUp()
		{
			this.SetVerticalOffset(Math.Max(0.0, _offset - _lineHeight * (this.VisibleLineCount - 1)));
		}

		public void PageDown()
		{
			if (_extentHeight > this.ActualHeight)
			{
				this.SetVerticalOffset(Math.Min(_extentHeight - this.ActualHeight,
					_offset + _lineHeight * (this.VisibleLineCount - 1)));
			}
		}

		public void SetVerticalOffset(double offset)
		{
			var delta = offset - _offset;

			_blocks.ForEach((b) =>
				{
					if (b.Y >= 0.0)
					{
						b.Y += delta;
					}
				});
			_offset = offset;
			this.InvalidateVisual();
			if (_viewer != null)
			{
				_viewer.InvalidateScrollInfo();
			}

			_isAutoScrolling = _offset > _extentHeight - this.ActualHeight - _lineHeight;
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
				return _lineHeight == 0.0 ? 0 : (int)Math.Ceiling(this.ActualHeight / _lineHeight);
			}
		}
	}
}
