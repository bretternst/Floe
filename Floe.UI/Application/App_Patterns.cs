using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace Floe.UI
{
	public partial class App : Application
	{
		private static List<Regex> _attnPatterns = new List<Regex>();

		private static void RefreshAttentionPatterns()
		{
			_attnPatterns.Clear();
			foreach (var s in App.Settings.Current.Formatting.AttentionPatterns.Split(
				Environment.NewLine.ToCharArray()).Where((s) => s.Trim().Length > 0))
			{
				try
				{
					_attnPatterns.Add(new Regex(s));
				}
				catch (ArgumentException)
				{
				}
			}
		}

		public static bool IsAttentionMatch(string nickname, string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				if (App.Settings.Current.Formatting.AttentionOnOwnNickname && (
					text.IndexOf(nickname, StringComparison.OrdinalIgnoreCase) >= 0 ||
					text.IndexOf(App.Settings.Current.User.Nickname, StringComparison.OrdinalIgnoreCase) >= 0 ||
					(!string.IsNullOrEmpty(App.Settings.Current.User.AlternateNickname) && 
					text.IndexOf(App.Settings.Current.User.AlternateNickname, StringComparison.OrdinalIgnoreCase) >= 0)))
				{
					return true;
				}
				foreach (var pattern in _attnPatterns)
				{
					if (pattern.IsMatch(text))
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
