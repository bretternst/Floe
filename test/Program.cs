using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Floe.Audio;

namespace test
{
	class Program
	{
		static void Main(string[] args)
		{
			var sp = new System.Media.SoundPlayer(Environment.ExpandEnvironmentVariables("%SYSTEMROOT%\\Media\\Windows Ding.wav"));
			sp.PlaySync();
		}
	}
}
