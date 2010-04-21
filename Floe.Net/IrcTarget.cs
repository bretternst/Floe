using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.Net
{
	public enum IrcTargetType
	{
		Channel,
		NickName
	}

	public class IrcTarget
	{
		public IrcTargetType Type { get; private set; }

		public string Name { get; private set; }

		public IrcTarget(string name)
		{
			if (name.Length > 1 && name[0] == '#' || name[0] == '+' || name[0] == '&' || name[0] == '!')
			{
				Type = IrcTargetType.Channel;
			}
			else
			{
				Type = IrcTargetType.NickName;
			}

			this.Name = name;
		}

		public IrcTarget(IrcTargetType type, string name)
		{
			this.Type = type;
			this.Name = name;
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}
