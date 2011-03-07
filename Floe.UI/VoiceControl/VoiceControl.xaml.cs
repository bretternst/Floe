using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

using Floe.Net;
using Floe.Voice;

namespace Floe.UI
{
	public partial class VoiceControl : UserControl, IDisposable
	{
		private IrcSession _session;
		private IrcTarget _target;
		private VoiceSession _voice;
		private IPEndPoint _publicEndPoint;
		private Dictionary<string, IPEndPoint> _peers;

		public VoiceControl(IrcSession session, IrcTarget target)
		{
			_session = session;
			_target = target;
			_peers = new Dictionary<string, IPEndPoint>();
			InitializeComponent();

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

		private void Start(UdpClient client = null)
		{
			_voice = new VoiceSession(VoiceCodec.Gsm610,
				App.Settings.Current.Voice.Quality, client);
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
				_voice.LocalEndPoint.Address.ToString(),
				_voice.LocalEndPoint.Port.ToString()), false);
		}

		private void AddPeer(string name, VoiceCodec codec, VoiceQuality quality, IPEndPoint endpoint)
		{
			if (!_peers.ContainsKey(name))
			{
				_voice.AddPeer(codec, quality, endpoint);
				this.RaiseEvent(new VoiceControlEventArgs(name, PeerAddedEvent));
			}
		}

		private void RemovePeer(string name)
		{
			if (_peers.ContainsKey(name))
			{
				_voice.RemovePeer(_peers[name]);
				_peers.Remove(name);
				this.RaiseEvent(new VoiceControlEventArgs(name, PeerRemovedEvent));
			}
		}

		private void ChangePeerName(string oldName, string newName)
		{
			if(_peers.ContainsKey(oldName))
			{
				_peers.Add(newName, _peers[oldName]);
				_peers.Remove(oldName);
			}
		}

		public void Dispose()
		{
			this.UnsubscribeEvents();
		}
	}
}
