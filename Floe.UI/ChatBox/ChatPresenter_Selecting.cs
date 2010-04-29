using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Floe.UI
{
	public partial class ChatPresenter : Control, IScrollInfo
	{
		private bool _isSelecting;

		private int _selStartLine, _selStartChar, _selEndLine, _selEndChar;

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (_lastLine == null)
			{
				return;
			}

			var p = e.GetPosition(this);
			_selStartLine = this.GetLineAt(p.Y);

			if (_selStartLine >= 0 && _selStartLine < _output.Count)
			{
				var ch = _output[_selStartLine].GetCharacterHitFromDistance(p.X);
				if (ch != null)
				{
					_selStartChar = ch.FirstCharacterIndex;
					_isSelecting = true;
					Mouse.OverrideCursor = Cursors.IBeam;
					this.CaptureMouse();
				}
			}

			base.OnMouseDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (!_isSelecting)
			{
				return;
			}

			var p = e.GetPosition(this);
			_selEndLine = this.GetLineAt(p.Y);
			_selEndChar = -1;
			if (_selEndLine >= 0 && _selEndLine < _output.Count)
			{
				var ch = _output[_selEndLine].GetCharacterHitFromDistance(p.X);
				if (ch != null)
				{
					_selEndChar = ch.FirstCharacterIndex;
				}
			}

			this.InvalidateVisual();
			base.OnMouseMove(e);
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			if (!_isSelecting)
			{
				return;
			}

			this.ReleaseMouseCapture();
			Mouse.OverrideCursor = null;
			_isSelecting = false;

			this.InvalidateVisual();
			base.OnMouseMove(e);
		}

		private int GetLineAt(double y)
		{
			y = this.ActualHeight - y;
			if (y < 0.0)
			{
				return int.MaxValue;
			}
			double vPos = 0.0;
			int i = 0;
			for (i = _output.IndexOf(_lastLine); i >= 0; --i)
			{
				vPos += _output[i].Height;
				if (vPos > y)
				{
					break;
				}
			}
			return i;
		}
	}
}
