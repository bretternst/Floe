using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace Floe.UI
{
	public static class VisualExtensions
	{
		public static ScrollViewer FindScrollViewer(this FlowDocumentScrollViewer viewer)
		{
			DependencyObject obj = viewer;
			do
			{
				if (VisualTreeHelper.GetChildrenCount(obj) > 0)
				{
					obj = VisualTreeHelper.GetChild(obj as Visual, 0);
				}
				else
				{
					return null;
				}
			}
			while (!(obj is ScrollViewer));

			return obj as ScrollViewer;
		}
	}
}
