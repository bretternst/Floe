using System;

namespace Floe.Net
{
	public enum IrcTargetType
	{
		Channel,
		Nickname
	}

	public sealed class IrcTarget
	{
		public IrcTargetType Type { get; private set; }

		public string Name { get; private set; }

		public IrcTarget(string name)
		{
			if (IsChannel(name))
			{
				Type = IrcTargetType.Channel;
			}
			else
			{
				Type = IrcTargetType.Nickname;
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

		public static bool IsChannel(string name)
		{
			return name.Length > 1 && name[0] == '#' || name[0] == '+' || name[0] == '&' || name[0] == '!';
		}

		public override bool Equals(object obj)
		{
			var other = obj as IrcTarget;
			return other != null && other.Type == this.Type &&
				string.Compare(other.Name, this.Name, StringComparison.OrdinalIgnoreCase) == 0;
		}

		public override int GetHashCode()
		{
			return (this.Type.ToString() + " " + this.Name).GetHashCode();
		}
	}
}
