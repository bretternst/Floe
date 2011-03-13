using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.Audio
{
	[Serializable]
	public class FileFormatException : Exception
	{
		public FileFormatException()
		{
		}

		public FileFormatException(string message)
			: base(message)
		{
		}
	}
}
