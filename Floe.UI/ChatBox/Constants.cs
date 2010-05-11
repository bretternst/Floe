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
		private static Regex urlRegex = new Regex(@"(www\.|(http|https|ftp)+\:\/\/)[^\s]+", RegexOptions.IgnoreCase);

		public static Regex UrlRegex { get { return urlRegex; } }
	}
}
