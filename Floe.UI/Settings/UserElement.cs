using System;
using System.Configuration;

namespace Floe.Configuration
{
	public class UserElement : ConfigurationElement
	{
		[ConfigurationProperty("nickname", DefaultValue = "", IsRequired = true)]
		public string Nickname
		{
			get
			{
				if (((string)this["nickname"]).Trim().Length < 1)
				{
					this["nickname"] = Environment.UserName;
				}
				return (string)this["nickname"];
			}
			set { this["nickname"] = value; }
		}

		[ConfigurationProperty("alternateNickname", DefaultValue = "", IsRequired = true)]
		public string AlternateNickname
		{
			get { return (string)this["alternateNickname"]; }
			set { this["alternateNickname"] = value; }
		}

		[ConfigurationProperty("userName", DefaultValue = "", IsRequired = true)]
		public string UserName
		{
			get
			{
				if (((string)this["userName"]).Trim().Length < 1)
				{
					this["userName"] = Environment.UserName;
				}
				return (string)this["userName"]; }
			set
			{
				if (value.Trim().Length == 0) throw new ArgumentException("Nickname cannot be empty.");
				this["userName"] = value;
			}
		}

		[ConfigurationProperty("fullName", DefaultValue = "", IsRequired = true)]
		public string FullName
		{
			get
			{
				if (((string)this["fullName"]).Trim().Length < 1)
				{
					this["fullName"] = this.Nickname;
				}
				return (string)this["fullName"];
			}
			set { this["fullName"] = value; }
		}

		[ConfigurationProperty("hostName", DefaultValue = "", IsRequired = true)]
		public string HostName
		{
			get
			{
				if (((string)this["hostName"]).Trim().Length < 1)
				{
					this["hostName"] = System.Net.Dns.GetHostName();
				}
				return (string)this["hostName"];
			}
			set { this["hostName"] = value; }
		}
	}
}
