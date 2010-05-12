using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.UI
{
	public class Lazy<T>
	{
		private Func<T> _creator;
		private T _value;

		public T Value
		{
			get
			{
				if (_value == null)
				{
					_value = _creator();
				}
				return _value;
			}
		}

		//public Lazy(Func<T> creator)
		//    : this(creator, true)
		//{
		//}

		public Lazy(Func<T> creator)
		{
			_creator = creator;
		}
	}
}
