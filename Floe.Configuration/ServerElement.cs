using System;
using System.Configuration;

namespace Floe.Configuration
{
	public sealed class ServerElementCollection : ConfigurationElementCollection
	{
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.BasicMap;
			}
		}

		protected override string ElementName
		{
			get
			{
				return "server";
			}
		}

		public ServerElement this[int index]
		{
			get
			{
				return (ServerElement)this.BaseGet((index));
			}
			set
			{
				if (BaseGet(index) != null)
				{
					this.BaseRemoveAt(index);
				}
				this.BaseAdd(index, value);
			}
		}

		public void Add(ServerElement element)
		{
			this.BaseAdd(element);
		}

		public void Clear()
		{
			this.BaseClear();
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ServerElement();
		}

		protected override object  GetElementKey(ConfigurationElement element)
		{
			return ((ServerElement)element).Name;
		}

		public void Remove(ServerElement element)
		{
			this.BaseRemove(element);
		}

		public void Remove(string name)
		{
			this.BaseRemove(name);
		}

		public void RemoveAt(int index)
		{
			this.BaseRemoveAt(index);
		}
	}

	public sealed class ServerElement : ConfigurationElement
	{
		[ConfigurationProperty("name")]
		public string Name
		{
			get { return (string)this["name"]; }
			set
			{
				if (value.Trim().Length == 0) throw new ArgumentException("Name cannot be empty.");
				this["name"] = value;
			}
		}

		[ConfigurationProperty("hostName")]
		public string Hostname
		{
			get
			{
				if (((string)this["hostName"]).Trim().Length < 1)
				{
					this["hostName"] = "irc.";
				}
				return (string)this["hostName"];
			}
			set { this["hostName"] = value; }
		}

		[ConfigurationProperty("port", DefaultValue=6667)]
		[IntegerValidator(MinValue = 0, MaxValue = 65535)]
		public int Port
		{
			get { return (int)this["port"]; }
			set { this["port"] = value; }
		}

		[ConfigurationProperty("isSecure", DefaultValue=false)]
		public bool IsSecure
		{
			get { return (bool)this["isSecure"]; }
			set { this["isSecure"] = value; }
		}

		[ConfigurationProperty("connectOnStartup", DefaultValue=false)]
		public bool ConnectOnStartup
		{
			get { return (bool)this["connectOnStartup"]; }
			set { this["connectOnStartup"] = value; }
		}

		[ConfigurationProperty("autoReconnect", DefaultValue=true)]
		public bool AutoReconnect
		{
			get { return (bool)this["autoReconnect"]; }
			set { this["autoReconnect"] = value; }
		}

		[ConfigurationProperty("onConnect", DefaultValue="")]
		public string OnConnect
		{
			get { return (string)this["onConnect"]; }
			set { this["onConnect"] = value; }
		}
	}
}
