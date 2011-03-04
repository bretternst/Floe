using System;

namespace Floe.Net
{
	/// <summary>
	/// This class represents information that is used to connect to a SOCKS5 proxy server.
	/// </summary>
	public class ProxyInfo
	{
		/// <summary>
		/// Gets or sets the hostname of the proxy.
		/// </summary>
		public string ProxyHostname { get; set; }

		/// <summary>
		/// Gets or sets the proxy port.
		/// </summary>
		public int ProxyPort { get; set; }

		/// <summary>
		/// Gets or sets the proxy username. If this value is null, no authentication will be performed.
		/// </summary>
		public string ProxyUsername { get; set; }

		/// <summary>
		/// Gets or sets the proxy password. If this value is null, no authentication will be performed.
		/// </summary>
		public string ProxyPassword { get; set; }

		/// <summary>
		/// Construct a new ProyInfo object.
		/// </summary>
		/// <param name="proxyHostname">The proxy hostname.</param>
		/// <param name="proxyPort">The proxy port number.</param>
		/// <param name="proxyUsername">An optional proxy username.</param>
		/// <param name="proxyPassword">An optional proxy password.</param>
		public ProxyInfo(string proxyHostname, int proxyPort, string proxyUsername = null, string proxyPassword = null)
		{
			this.ProxyHostname = proxyHostname;
			this.ProxyPort = proxyPort;
			this.ProxyUsername = string.IsNullOrEmpty(proxyUsername) ? null : proxyUsername;
			this.ProxyPassword = proxyPassword;
		}
	}
}
