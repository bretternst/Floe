using System;
using System.Configuration;

namespace Floe.Configuration
{
	public class SoundsElement : ConfigurationElement
	{
		private const string DefaultDing = "%SYSTEMROOT%\\Media\\Windows Ding.wav";

		[ConfigurationProperty("isEnabled", DefaultValue=true)]
		public bool IsEnabled
		{
			get { return (bool)this["isEnabled"]; }
			set { this["isEnabled"] = value; }
		}

		[ConfigurationProperty("connect", DefaultValue="")]
		public string Connect
		{
			get { return (string)this["connect"]; }
			set { this["connect"] = value; }
		}

		[ConfigurationProperty("disconnect", DefaultValue=DefaultDing)]
		public string Disconnect
		{
			get { return (string)this["disconnect"]; }
			set { this["disconnect"] = value; }
		}

		[ConfigurationProperty("privateMessage", DefaultValue=DefaultDing)]
		public string PrivateMessage
		{
			get { return (string)this["privateMessage"]; }
			set { this["privateMessage"] = value; }
		}

		[ConfigurationProperty("dccRequest", DefaultValue=DefaultDing)]
		public string DccRequest
		{
			get { return (string)this["dccRequest"]; }
			set { this["dccRequest"] = value; }
		}

		[ConfigurationProperty("dccComplete", DefaultValue="")]
		public string DccComplete
		{
			get { return (string)this["dccComplete"]; }
			set { this["dccComplete"] = value; }
		}

		[ConfigurationProperty("dccError", DefaultValue = DefaultDing)]
		public string DccError
		{
			get { return (string)this["dccError"]; }
			set { this["dccError"] = value; }
		}

		[ConfigurationProperty("notice", DefaultValue="")]
		public string Notice
		{
			get { return (string)this["notice"]; }
			set { this["notice"] = value; }
		}

		[ConfigurationProperty("activeAlert", DefaultValue="")]
		public string ActiveAlert
		{
			get { return (string)this["activeAlert"]; }
			set { this["activeAlert"] = value; }
		}

		[ConfigurationProperty("inactiveAlert", DefaultValue = DefaultDing)]
		public string InactiveAlert
		{
			get { return (string)this["inactiveAlert"]; }
			set { this["inactiveAlert"] = value; }
		}

		[ConfigurationProperty("beep", DefaultValue = DefaultDing)]
		public string Beep
		{
			get { return (string)this["beep"]; }
			set { this["beep"] = value; }
		}

		public string GetPathByName(string eventName)
		{
			var path = this[eventName] as string;
			return string.IsNullOrEmpty(path) ? null : Environment.ExpandEnvironmentVariables(path);
		}
	}
}
