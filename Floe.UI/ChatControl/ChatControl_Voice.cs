using System;
using System.Net;
using System.Windows;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatControl : ChatPage
	{
		private static bool _isInVoiceSession;

		private VoiceControl _voiceControl;

		private void StartVoiceSession()
		{
			if (!this.IsChannel || _isInVoiceSession)
			{
				return;
			}

			_voiceControl = new VoiceControl(this.Session, this.Target);
			_voiceControl.Close += voiceControl_Close;
			pnlVoice.Children.Add(_voiceControl);
			pnlVoice.Visibility = Visibility.Visible;
			_isInVoiceSession = true;
		}

		private void StopVoiceSession()
		{
			if (_voiceControl != null)
			{
				pnlVoice.Visibility = Visibility.Collapsed;
				pnlVoice.Children.Clear();
				_isInVoiceSession = false;
				_voiceControl.Dispose();
				_voiceControl = null;
			}
		}

		private void voiceControl_Close(object sender, RoutedEventArgs e)
		{
			this.StopVoiceSession();
		}
	}
}
