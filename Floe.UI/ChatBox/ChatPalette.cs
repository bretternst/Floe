using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Floe.UI
{
	public class ChatPalette
	{
		private Dictionary<string, Brush> _brushes;
		private Brush _defaultBrush;

		public ChatPalette(Brush defaultBrush)
		{
			_brushes = new Dictionary<string, Brush>();
			_defaultBrush = defaultBrush;
		}

		public void Add(string key, Brush brush)
		{
			_brushes.Add(key, brush);
		}

		public Brush this[string key]
		{
			get
			{
				if (_brushes.ContainsKey(key))
				{
					return _brushes[key];
				}
				return _defaultBrush;
			}
		}
	}
}
