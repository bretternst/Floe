using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Floe.Net;
using Floe.Audio;

namespace Floe.UI
{
	public partial class VoiceControl : UserControl, IDisposable
	{
		public readonly static RoutedUICommand StartVoiceCommand = new RoutedUICommand("Start Voice Chat", "StartVoice", typeof(VoiceControl));
		public readonly static RoutedUICommand StopVoiceCommand = new RoutedUICommand("Stop Voice Chat", "StopVoice", typeof(VoiceControl));

		private void CanExecuteStartVoice(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = !_isAnyVoiceChatActive;
		}

		private void CanExecuteStopVoice(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.IsChatting;
		}

		private void ExecuteStartVoice(object sender, ExecutedRoutedEventArgs e)
		{
			_isAnyVoiceChatActive = true;
			this.IsChatting = true;
			if (App.Settings.Current.Voice.UseStun)
			{
				var stun = new StunUdpClient(App.Settings.Current.Voice.StunServer, App.Settings.Current.Voice.AltStunServer);
				stun.BeginGetClient((ar) =>
				{
					UdpClient client = null;
					try
					{
						client = stun.EndGetClient(ar, out _publicEndPoint);
					}
					catch (StunException ex)
					{
						System.Diagnostics.Debug.WriteLine("STUN error: " + ex.Message);
					}
					this.Dispatcher.BeginInvoke((Action)(() => this.Start(client)));
				});
			}
			else
			{
				this.Start();
			}
		}

		private void ExecuteStopVoice(object sender, ExecutedRoutedEventArgs e)
		{
			this.UnsubscribeEvents();
			if (_peers.Count > 0 && !_isDisposed)
			{
				_session.SendCtcp(_target, new CtcpCommand("VCHAT", "STOP"), false);
			}
			_voice.Dispose();
			_voice = null;
			this.IsChatting = false;
			_isAnyVoiceChatActive = false;
			foreach (var item in _nickList)
			{
				SetIsVoiceChat(item, false);
				SetIsTalking(item, false);
			}
			_peers.Clear();
		}

		private void Start(UdpClient client = null)
		{
			_voice = new VoiceClient(new CodecInfo(VoiceCodec.Gsm610, App.Settings.Current.Voice.Quality),
				client, this.TransmitPredicate, this.ReceivePredicate);
			_voice.OutputVolume = App.Settings.Current.Voice.PlaybackVolume;
			_voice.OutputGain = App.Settings.Current.Voice.OutputGain;
			_voice.InputGain = App.Settings.Current.Voice.InputGain;
			if (client == null)
			{
				_publicEndPoint = new IPEndPoint(_session.ExternalAddress, _voice.LocalEndPoint.Port);
			}
			this.SubscribeEvents();
			_session.SendCtcp(_target, new CtcpCommand("VCHAT", "START",
				VoiceCodec.Gsm610.ToString(),
				App.Settings.Current.Voice.Quality.ToString(),
				_publicEndPoint.Address.ToString(),
				_publicEndPoint.Port.ToString(),
				_session.InternalAddress.ToString(),
				_voice.LocalEndPoint.Port.ToString()), false);
			if (_self != null)
			{
				SetIsVoiceChat(_self, true);
			}
			_voice.Open();
		}
	}
}
