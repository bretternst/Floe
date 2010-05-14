using System;
using System.Linq;
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

		public ChannelStateElement Get(string name)
		{
			name = name.ToLowerInvariant();
			return this.OfType<ChannelStateElement>().Where((s) => name == s.Name).FirstOrDefault();
		}

		public void Set(string name, string placement)
		{
			var el = this.Get(name);
			if (el != null)
			{
				el.Placement = placement;
			}
			else
			{
				this.Add(new ChannelStateElement() { Name = name.ToLowerInvariant(), Placement = placement });
			}
		}

		public void Delete(string name)
		{
			var el = this.Get(name);
			if (el != null)
			{
				this.Remove(el);
			}
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

		[ConfigurationProperty("placement", DefaultValue="")]
		public string Placement
		{
			get { return (string)this["placement"]; }
			set { this["placement"] = value; }
		}
	}
}
