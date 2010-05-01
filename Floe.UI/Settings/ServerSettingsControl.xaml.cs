using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Floe.Configuration;

namespace Floe.UI.Settings
{
	public partial class ServerSettingsControl : UserControl
	{
		public ServerSettingsControl()
		{
			if (App.Settings.Current.Servers.Count == 0)
			{
				var server = new ServerElement();
				server.Name = "New Server";
				server.Port = 6667;
				App.Settings.Current.Servers.Add(server);
			}

			InitializeComponent();

			lstServers.SelectedIndex = 0;
		}

		private void btnNew_Click(object sender, RoutedEventArgs e)
		{
			var server = new ServerElement();

			string newName = server.Name = "New Server";
			int i = 1;
			while (App.Settings.Current.Servers.OfType<ServerElement>().Any((s) => s.Name == (server.Name = string.Format(newName, i))))
			{
				if (++i == 2)
				{
					newName += " {0}";
				}
			}
			server.Port = 6667;

			App.Settings.Current.Servers.Add(server);
			lstServers.Items.Refresh();
			lstServers.SelectedItem = server;
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			int selectedIndex = lstServers.SelectedIndex;
			App.Settings.Current.Servers.RemoveAt(selectedIndex);
			lstServers.Items.Refresh();
			lstServers.SelectedIndex = Math.Min(lstServers.Items.Count-1, selectedIndex);
		}

		private void txtName_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
		{
			int i = 2;
			var server = lstServers.SelectedItem as ServerElement;
			string origName = server.Name;
			while (App.Settings.Current.Servers.OfType<ServerElement>().Any((s) => s.Name == server.Name && s != server))
			{
				server.Name = origName + " " + i++;
			}
			txtName.Text = server.Name;
			lstServers.Items.Refresh();
		}
	}
}
