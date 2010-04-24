using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.UI
{
	public class InputException : Exception
	{
		public InputException(string message)
			: base(message)
		{
		}
	}
}
