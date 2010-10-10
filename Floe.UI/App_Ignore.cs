using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Floe.Net;

namespace Floe.UI
{
	public partial class App : Application
	{
		private static List<string> _ignoreMasks = new List<string>();
		private static List<Regex> _ignorePatterns = new List<Regex>();

		public static IEnumerable<string> IgnoreMasks
		{
			get
			{
				return _ignoreMasks;
			}
		}

		public static void AddIgnore(string mask)
		{
			mask = mask.Replace(",", "");
			if (!(from m in _ignoreMasks where string.Compare(mask, m, StringComparison.OrdinalIgnoreCase) == 0 select m).Any())
			{
				_ignoreMasks.Add(mask);
				RefreshIgnorePatterns();
				SaveIgnoreMasks();
			}
		}

		public static bool RemoveIgnore(string mask)
		{
			bool removedAny = false;

			for (int i = _ignoreMasks.Count - 1; i >= 0; --i)
			{
				if (string.Compare(mask, _ignoreMasks[i], StringComparison.OrdinalIgnoreCase) == 0)
				{
					_ignoreMasks.RemoveAt(i);
					RefreshIgnorePatterns();
					SaveIgnoreMasks();
					removedAny = true;
				}
			}

			return removedAny;
		}

		public static bool IsIgnoreMatch(IrcPrefix prefix)
		{
			if (prefix == null)
			{
				return false;
			}

			return (from pattern in _ignorePatterns
					where pattern.IsMatch(prefix.Prefix)
					select true).Any();
		}

		private static void LoadIgnoreMasks()
		{
			_ignoreMasks = App.Settings.Current.Ignore.Split(',').Where((s) => s.Length > 0).ToList();
			RefreshIgnorePatterns();
		}

		private static void SaveIgnoreMasks()
		{
			App.Settings.Current.Ignore = string.Join(",", _ignoreMasks);
		}

		private static void RefreshIgnorePatterns()
		{
			_ignorePatterns = (from mask in _ignoreMasks
							   where mask.Length > 0
							   select new Regex("^" + Regex.Escape(mask).Replace("\\*", ".*").Replace("\\?", ".") + "$",
								   RegexOptions.IgnoreCase)).ToList();
		}
	}
}
