using System;
using System.Media;
using System.Windows;

using Floe.Audio;
using Floe.Interop;

namespace Floe.UI
{
	public partial class App : Application
	{
		private static FilePlayer _player;

		public static void DoEvent(string eventName)
		{
			if (App.Settings.Current.Sounds.IsEnabled)
			{
				string path = App.Settings.Current.Sounds.GetPathByName(eventName);
				if (!string.IsNullOrEmpty(path))
				{
					if (_player != null)
					{
						_player.Dispose();
					}
					try
					{
						_player = new FilePlayer(path);
						_player.Start();
					}
					catch (Exception ex)
					{
						_player = null;
						System.Diagnostics.Debug.WriteLine(
							string.Format("Unable to play audio file {0}: {1}", path, ex.Message));
					}
				}
			}
		}
	}
}
