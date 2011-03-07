using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;

using Floe.Net;
using Floe.Voice;

namespace Floe.UI
{
	public class VoiceControlEventArgs : RoutedEventArgs
	{
		public string Name { get; private set; }

		public VoiceControlEventArgs(string name, RoutedEvent evt)
			: base(evt)
		{
			this.Name = name;
		}
	}

	public partial class VoiceControl : UserControl, IDisposable
	{
		public static readonly RoutedEvent CloseEvent = EventManager.RegisterRoutedEvent("Close",
			RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VoiceControl));
		public event RoutedEventHandler Close
		{
			add { this.AddHandler(CloseEvent, value); }
			remove { this.RemoveHandler(CloseEvent, value); }
		}

		public static readonly RoutedEvent PeerAddedEvent = EventManager.RegisterRoutedEvent("PeerAdded",
			RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VoiceControl));
		public event RoutedEventHandler PeerAdded
		{
			add { this.AddHandler(PeerAddedEvent, value); }
			remove { this.RemoveHandler(PeerAddedEvent, value); }
		}

		public static readonly RoutedEvent PeerRemovedEvent = EventManager.RegisterRoutedEvent("PeerRemoved",
			RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VoiceControl));
		public event RoutedEventHandler PeerRemoved
		{
			add { this.AddHandler(PeerRemovedEvent, value); }
			remove { this.RemoveHandler(PeerRemovedEvent, value); }
		}

		private void voice_Error(object sender, ErrorEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine(e.Exception.ToString());
			MessageBox.Show("An error occurred and voice chat must close: " + e.Exception.Message);
			this.RaiseEvent(new RoutedEventArgs(CloseEvent));
		}

		private void btnStopVoice_Click(object sender, RoutedEventArgs e)
		{
			_session.SendCtcp(_target, new CtcpCommand("VCHAT", "STOP"), false);
			this.RaiseEvent(new RoutedEventArgs(CloseEvent));
		}

		private void session_CtcpCommandReceived(object sender, CtcpEventArgs e)
		{
			VoiceCodec codec;
			VoiceQuality quality;
			IPAddress pubAddress, prvAddress;
			int pubPort, prvPort;

			if (string.Compare(e.Command.Command, "VCHAT", StringComparison.OrdinalIgnoreCase) == 0)
			{
				if (e.Command.Arguments.Length == 7 &&
				string.Compare(e.Command.Arguments[0], "START", StringComparison.OrdinalIgnoreCase) == 0 &&
				e.To.Equals(_target) &&
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
							_voice.LocalEndPoint.Address.ToString(),
							_voice.LocalEndPoint.Port.ToString()), true);
					}
				}
				else if (e.Command.Arguments.Length == 1 &&
					string.Compare(e.Command.Arguments[0], "STOP", StringComparison.OrdinalIgnoreCase) == 0 &&
					_peers.ContainsKey(e.From.Nickname))
				{
					this.RemovePeer(e.From.Nickname);
				}
			}
		}

		private void SubscribeEvents()
		{
			_voice.Error += voice_Error;
			_session.CtcpCommandReceived += session_CtcpCommandReceived;
		}

		private void UnsubscribeEvents()
		{
			_voice.Error -= voice_Error;
			_session.CtcpCommandReceived -= session_CtcpCommandReceived;
		}
	}
}
