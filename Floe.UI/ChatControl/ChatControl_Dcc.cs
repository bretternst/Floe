using System;
using System.Net;
using System.Windows;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatControl : ChatPage
	{
		private DccChat _dcc;
		private IPAddress _address;
		private int _port;
		private bool _isPortForwarding;

		public void StartListen(Action<int> readyCallback)
		{
			_dcc = new DccChat();
			_dcc.Connected += dcc_Connected;
			_dcc.Disconnected += dcc_Disconnected;
			_dcc.Error += dcc_Error;
			_dcc.MessageReceived += dcc_MessageReceived;

			try
			{
				_port = _dcc.Listen(App.Settings.Current.Dcc.LowPort, App.Settings.Current.Dcc.HighPort);
			}
			catch (InvalidOperationException)
			{
				this.Write("Error", "No available ports.");
				_port = 0;
			}

			if (App.Settings.Current.Dcc.EnableUpnp && NatHelper.IsAvailable)
			{
				this.Write("Client", "Forwarding port...");
				NatHelper.BeginAddForwardingRule(_port, System.Net.Sockets.ProtocolType.Tcp, "Floe DCC", (o) =>
				{
					this.Dispatcher.BeginInvoke((Action)(() =>
					{
						this.Write("Client", "Waiting for connection...");
						readyCallback(_port);
					}));
				});
				_isPortForwarding = true;
			}
			else
			{
				this.Write("Client", "Waiting for connection...");
				readyCallback(_port);
			}
		}

		public void StartAccept(IPAddress address, int port)
		{
			_address = address;
			_port = port;
			if (App.Settings.Current.Dcc.AutoAccept)
			{
				this.AcceptChat();
			}
			else
			{
				lblDccChat.Content = string.Format("Do you want DCC chat with {0}?", this.Target.Name);
				pnlDccChat.Visibility = Visibility.Visible;
			}
			App.DoEvent("dccRequest");
		}

		private void DeletePortForwarding()
		{
			if (_isPortForwarding)
			{
				NatHelper.BeginDeleteForwardingRule(_port, System.Net.Sockets.ProtocolType.Tcp, (ar) => NatHelper.EndDeleteForwardingRule(ar));
				_isPortForwarding = false;
			}
		}

		private void dcc_Connected(object sender, EventArgs e)
		{
			this.Write("Client", "Connected");
			this.IsConnected = true;
		}

		private void dcc_Disconnected(object sender, EventArgs e)
		{
			this.Write("Error", "Disconnected");
			this.IsConnected = false;
			this.DeletePortForwarding();
		}

		private void dcc_Error(object sender, ErrorEventArgs e)
		{
			this.Write("Error", e.Exception.Message);
			this.IsConnected = false;
			this.DeletePortForwarding();
		}

		private void dcc_MessageReceived(object sender, DccChatEventArgs e)
		{
			string text = e.Text;
			if (text.StartsWith("\u0001ACTION ") && text.EndsWith("\u0001") &&
				text.Length > 9)
			{
				text = text.Substring(8, text.Length - 9);
				this.Write("Default", string.Format("{0} {1}", this.Target.Name, text));
			}
			else
			{
				this.Write("Default", 0, this.Target.Name, e.Text, false);
			}
		}

		private void AcceptChat()
		{
			_dcc = new DccChat();
			_dcc.Connected += dcc_Connected;
			_dcc.Disconnected += dcc_Disconnected;
			_dcc.Error += dcc_Error;
			_dcc.MessageReceived += dcc_MessageReceived;
			this.Write("Client", "Connecting...");
			_dcc.Connect(_address, _port);
		}

		private void btnAccept_Click(object sender, RoutedEventArgs e)
		{
			pnlDccChat.Visibility = Visibility.Collapsed;
			this.AcceptChat();
		}

		private void btnDecline_Click(object sender, RoutedEventArgs e)
		{
			this.Session.SendCtcp(this.Target, new CtcpCommand("ERRMSG", "DCC", "CHAT", "declined"), true);
			App.ClosePage(this);
		}
	}
}
