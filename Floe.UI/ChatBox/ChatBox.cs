using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Floe.UI
{
	[TemplatePart(Name="PART_ChatPresenter", Type=typeof(ChatPresenter))]
	public class ChatBox : Control
	{
		private ChatPresenter _presenter;

		public static DependencyProperty BufferLinesProperty = DependencyProperty.Register("BufferLines",
			typeof(int), typeof(ChatBox));
		public int BufferLines
		{
			get { return (int)this.GetValue(BufferLinesProperty); }
			set { this.SetValue(BufferLinesProperty, value); }
		}

		public ChatBox()
		{
			this.DefaultStyleKey = typeof(ChatBox);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_presenter = base.GetTemplateChild("PART_ChatPresenter") as ChatPresenter;
			if (_presenter == null)
			{
				throw new Exception("Missing template part.");
			}
		}

		public void AppendLine(string text)
		{
			_presenter.AppendLine(text);
		}

		public void PageUp()
		{
			_presenter.PageUp();
		}

		public void PageDown()
		{
			_presenter.PageDown();
		}

		public void MouseWheelUp()
		{
			_presenter.MouseWheelUp();
		}

		public void MouseWheelDown()
		{
			_presenter.MouseWheelDown();
		}
	}
}
