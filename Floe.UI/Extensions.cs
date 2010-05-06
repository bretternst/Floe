using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Floe.UI
{
	public static class Extensions
	{
		public static void Invoke(this DependencyObject obj, Action action)
		{
			obj.Dispatcher.Invoke(action);
		}

		public static void BeginInvoke(this DependencyObject obj, Action action)
		{
			obj.Dispatcher.BeginInvoke(action);
		}
	}
}
