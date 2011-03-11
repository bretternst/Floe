using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;

using Floe.Net;
using Floe.Voice;
using Floe.Interop;

namespace Floe.UI
{
	public partial class VoiceControl : UserControl, IDisposable
	{
		private void voice_Error(object sender, ErrorEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine(e.Exception.ToString());
			MessageBox.Show("An error occurred and voice chat must stop: " + e.Exception.Message);
			this.IsChatting = false;
		}

		private void session_CtcpCommandReceived(object sender, CtcpEventArgs e)
		{
			VoiceCodec codec;
			VoiceQuality quality;
			IPAddress pubAddress, prvAddress;
			int pubPort, prvPort;

			if (!this.IsChatting)
			{
				return;
			}

			if (string.Compare(e.Command.Command, "VCHAT", StringComparison.OrdinalIgnoreCase) == 0)
			{
				if (e.Command.Arguments.Length == 7 &&
				string.Compare(e.Command.Arguments[0], "START", StringComparison.OrdinalIgnoreCase) == 0 &&
				(!e.To.IsChannel || e.To.Equals(_target)) &&
				Enum.TryParse(e.Command.Arguments[1], out codec) &&
				Enum.TryParse(e.Command.Arguments[2], out quality) &&
				IPAddress.TryParse(e.Command.Arguments[3], out pubAddress) &&
				int.TryParse(e.Command.Arguments[4], out pubPort) &&
				IPAddress.TryParse(e.Command.Arguments[5], out prvAddress) &&
				int.TryParse(e.Command.Arguments[6], out prvPort) &&
				pubPort > 0 && pubPort <= ushort.MaxValue &&
				prvPort > 0 && prvPort <= ushort.MaxValue)
				{
					// if they're behind the same NAT, use the local address
					if (pubAddress.Equals(_publicEndPoint.Address))
					{
						pubAddress = prvAddress;
						pubPort = prvPort;
					}
					this.AddPeer(e.From.Nickname, codec, quality, new IPEndPoint(pubAddress, pubPort));
					if (!e.IsResponse && e.To.IsChannel)
					{
						_session.SendCtcp(new IrcTarget(e.From), new CtcpCommand("VCHAT", "START",
							VoiceCodec.Gsm610.ToString(),
							App.Settings.Current.Voice.Quality.ToString(),
							_publicEndPoint.Address.ToString(),
							_publicEndPoint.Port.ToString(),
							_session.InternalAddress.ToString(),
							_voice.LocalEndPoint.Port.ToString()), true);
					}
				}
				else if (e.Command.Arguments.Length == 1 &&
					string.Compare(e.Command.Arguments[0], "STOP", StringComparison.OrdinalIgnoreCase) == 0)
				{
					this.RemovePeer(e.From.Nickname);
				}
			}
		}

		private void RawInput_ButtonDown(object sender, RawInputEventArgs e)
		{
			if (e.Button == App.Settings.Current.Voice.TalkKey)
			{
				_isTalkKeyDown = true;
			}
		}

		private void RawInput_ButtonUp(object sender, RawInputEventArgs e)
		{
			if (e.Button == App.Settings.Current.Voice.TalkKey)
			{
				_isTalkKeyDown = false;
			}
		}

		private bool TransmitPredicate(float peak)
		{
			bool isTransmitting = false;

			if (App.Settings.Current.Voice.PushToTalk)
			{
				isTransmitting = _isTalkKeyDown;
			}
			else if (peak >= App.Settings.Current.Voice.TalkLevel)
			{
				isTransmitting = true;
				_lastTransmit = DateTime.Now.Ticks;
			}
			else if (DateTime.Now.Ticks - _lastTransmit < TrailTime)
			{
				isTransmitting = true;
			}
			if (isTransmitting != _isTransmitting)
			{
				_isTransmitting = isTransmitting;
				this.Dispatcher.BeginInvoke((Action)(() => SetIsTalking(_self, (this.IsTransmitting = _isTransmitting))));
			}

			var ticks = DateTime.Now.Ticks;
			foreach (var peer in _peers.Values)
			{
				if (peer.IsTalking && ticks - peer.LastTransmit > TrailTime)
				{
					peer.IsTalking = false;
					this.Dispatcher.BeginInvoke((Action<NicknameItem>)((o) => SetIsTalking((NicknameItem)o, false)), peer.User);
				}
			}

			return isTransmitting;
		}

		private bool ReceivePredicate(IPEndPoint endpoint)
		{
			var peer = _peers[endpoint];
			peer.LastTransmit = DateTime.Now.Ticks;
			if (!peer.IsTalking)
			{
				peer.IsTalking = true;
				this.Dispatcher.BeginInvoke((Action<NicknameItem>)((o) => SetIsTalking((NicknameItem)o, true)), peer.User);
			}
			return !peer.IsMuted;
		}

		private void SubscribeEvents()
		{
			_voice.Error += voice_Error;
			_session.CtcpCommandReceived += session_CtcpCommandReceived;
			RawInput.ButtonDown += RawInput_ButtonDown;
			RawInput.ButtonUp += RawInput_ButtonUp;
		}

		private void UnsubscribeEvents()
		{
			_voice.Error -= voice_Error;
			_session.CtcpCommandReceived -= session_CtcpCommandReceived;
			RawInput.ButtonDown -= RawInput_ButtonDown;
			RawInput.ButtonUp -= RawInput_ButtonUp;
		}
	}
}
