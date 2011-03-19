using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

using Floe.Net;
using Floe.Audio;

namespace Floe.UI
{
	public partial class VoiceControl : UserControl, IDisposable
	{
		private const long TrailTime = 10000000;
		private static bool _isAnyVoiceChatActive;

		private IrcSession _session;
		private IrcTarget _target;
		private NicknameList _nickList;
		private VoiceSession _voice;
		private WaveInMeter _meter;
		private IPEndPoint _publicEndPoint;
		private Dictionary<IPEndPoint, VoicePeer> _peers;
		private NicknameItem _self;
		private bool _isTalkKeyDown;
		private bool _isTransmitting;
		private long _lastTransmit;

		public VoiceControl(IrcSession session, IrcTarget target, NicknameList nickList)
		{
			_session = session;
			_target = target;
			_nickList = nickList;
			_peers = new Dictionary<IPEndPoint, VoicePeer>();
			InitializeComponent();
			this.DataContext = this;
			if (nickList.Contains(_session.Nickname))
			{
				_self = nickList[_session.Nickname];
			}
		}

		public void ToggleMute(string nick)
		{
			var endpoint = this.FindEndPoint(nick);
			if (endpoint != null)
			{
				var peer = _peers[endpoint];
				peer.IsMuted = !peer.IsMuted;
				SetIsMuted(_peers[endpoint].User, peer.IsMuted);
			}
		}

		private void AddPeer(string nick, VoiceCodec codec, VoiceQuality quality, IPEndPoint endpoint)
		{
			if (_nickList.Contains(nick) && !_peers.ContainsKey(endpoint))
			{
				_peers.Add(endpoint, new VoicePeer() { User = _nickList[nick] });
				_voice.AddPeer(codec, quality, endpoint);
				SetIsVoiceChat(_nickList[nick], true);
			}
		}

		private void RemovePeer(string nick)
		{
			var endpoint = this.FindEndPoint(nick);
			if (endpoint != null)
			{
				SetIsVoiceChat(_peers[endpoint].User, false);
				_peers.Remove(endpoint);
				_voice.RemovePeer(endpoint);
			}
		}

		private IPEndPoint FindEndPoint(string nick)
		{
			var item = _nickList[nick];
			return _peers.Where((kvp) => kvp.Value.User == item).Select((kvp) => kvp.Key).FirstOrDefault();
		}

		public void Dispose()
		{
			if (this.IsChatting)
			{
				StopVoiceCommand.Execute(null, this);
			}
		}
	}
}
