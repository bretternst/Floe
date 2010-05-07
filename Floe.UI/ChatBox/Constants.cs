using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace Floe.UI
{
	public static class Constants
	{
		private static Regex urlRegex = new Regex(
@"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])",
		RegexOptions.IgnoreCase);

		public static Regex UrlRegex { get { return urlRegex; } }

		private static Lazy<Brush[]> brushes = new Lazy<Brush[]>(() =>
		{
			return new[] {
					new SolidColorBrush(Color.FromRgb(255, 255, 255)),
					new SolidColorBrush(Color.FromRgb(0, 0, 0)),
					new SolidColorBrush(Color.FromRgb(0, 0, 127)),
					new SolidColorBrush(Color.FromRgb(0, 147, 0)),
					new SolidColorBrush(Color.FromRgb(255, 0, 0)),
					new SolidColorBrush(Color.FromRgb(127, 0, 0)),
					new SolidColorBrush(Color.FromRgb(156, 0, 156)),
					new SolidColorBrush(Color.FromRgb(252, 127, 0)),
					new SolidColorBrush(Color.FromRgb(255, 255, 0)),
					new SolidColorBrush(Color.FromRgb(0, 252, 0)),
					new SolidColorBrush(Color.FromRgb(0, 147, 147)),
					new SolidColorBrush(Color.FromRgb(0, 255, 255)),
					new SolidColorBrush(Color.FromRgb(0, 0, 252)),
					new SolidColorBrush(Color.FromRgb(255, 0, 255)),
					new SolidColorBrush(Color.FromRgb(127, 127, 127)),
					new SolidColorBrush(Color.FromRgb(210, 210, 210)),
				};
		});

		public static Brush[] Brushes { get { return brushes.Value; } }
	}
}
