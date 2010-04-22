using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Floe.Configuration
{
	public class UserElement : ConfigurationElement
	{
		[ConfigurationProperty("nickname", DefaultValue = "", IsRequired = true)]
		public string Nickname
		{
			get { return (string)this["nickname"]; }
			set { this["nickname"] = value; }
		}

		[ConfigurationProperty("alternateNickname", DefaultValue = "", IsRequired = true)]
		public string AlternateNickname
		{
			get { return (string)this["alternateNickname"]; }
			set { this["alternateNickname"] = value; }
		}

		[ConfigurationProperty("userName", DefaultValue = "user", IsRequired = true)]
		public string UserName
		{
			get { return (string)this["userName"]; }
			set { this["userName"] = value; }
		}

		[ConfigurationProperty("fullName", DefaultValue = "", IsRequired = true)]
		public string FullName
		{
			get { return (string)this["fullName"]; }
			set { this["fullName"] = value; }
		}

		[ConfigurationProperty("hsotName", DefaultValue = "", IsRequired = true)]
		public string HostName
		{
			get { return (string)this["hostName"]; }
			set { this["hostName"] = value; }
		}
	}
}
