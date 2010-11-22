using System;
using System.Configuration;

namespace Floe.Configuration
{
	public sealed class ChannelStateElementCollection : ConfigurationElementCollection
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
				return "channelState";
			}
		}

		public ChannelStateElement this[int index]
		{
			get
			{
				return (ChannelStateElement)this.BaseGet((index));
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

		public new ChannelStateElement this[string key]
		{
			get
			{
				var el = this.BaseGet(key) as ChannelStateElement;
				if(el == null)
				{
					el = new ChannelStateElement();
					el.Name = key;
					this.Add(el);
				}
				return el;
			}
		}

		public bool Exists(string key)
		{
			return this.BaseGet(key) as ChannelStateElement != null;
		}

		public void Add(ChannelStateElement element)
		{
			this.BaseAdd(element);
		}

		public void Clear()
		{
			this.BaseClear();
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ChannelStateElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ChannelStateElement)element).Name.ToLowerInvariant();
		}

		public void Remove(ChannelStateElement element)
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

	public sealed class ChannelStateElement : ConfigurationElement
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

		[ConfigurationProperty("isDetached", DefaultValue=false)]
		public bool IsDetached
		{
			get { return (bool)this["isDetached"]; }
			set { this["isDetached"] = value; }
		}

		[ConfigurationProperty("placement", DefaultValue="")]
		public string Placement
		{
			get { return (string)this["placement"]; }
			set { this["placement"] = value; }
		}

		[ConfigurationProperty("nickListWidth", DefaultValue = 115.0)]
		public double NickListWidth
		{
			get { return (double)this["nickListWidth"]; }
			set { this["nickListWidth"] = value; }
		}

		[ConfigurationProperty("columnWidth", DefaultValue = 125.0)]
		public double ColumnWidth
		{
			get { return (double)this["columnWidth"]; }
			set { this["columnWidth"] = value; }
		}
	}
}
