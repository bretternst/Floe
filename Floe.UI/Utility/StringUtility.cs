using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.UI
{
	internal static class StringUtility
	{
		public static string[] Split(string str, int maxParts)
		{
			if (maxParts == 1)
			{
				str = str.Trim();
				return str.Length > 0 ? new[] { str } : new string[0];
			}

			var parts = new List<string>();
			var part = new StringBuilder();

			for (int i = 0; i < str.Length; i++)
			{
				if (maxParts == 1)
				{
					string remainder = str.Substring(i).Trim();
					if (remainder.Length > 0)
					{
						parts.Add(remainder);
					}
					break;
				}

				if (str[i] == ' ')
				{
					if(part.Length > 0)
					{
						parts.Add(part.ToString());
						part.Clear();
						--maxParts;
					}
				}
				else
				{
					part.Append(str[i]);
				}
			}
			if (part.Length > 0)
			{
				parts.Add(part.ToString());
			}

			return parts.ToArray();
		}
	}
}
