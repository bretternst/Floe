using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Media;
using Floe.Net;

namespace Floe.UI
{
	public partial class App : Application
	{
		private static SoundPlayer _player = new SoundPlayer();

		public static void DoEvent(string eventName)
		{
			if (App.Settings.Current.Sounds.IsEnabled)
			{
				string path = App.Settings.Current.Sounds.GetPathByName(eventName);
				if (!string.IsNullOrEmpty(path))
				{
					_player.SoundLocation = path;
					try
					{
						_player.Play();
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine("Unable to play sound: " + ex.Message);
					}
				}
			}
		}
	}
}
