using System;
using System.Windows.Input;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatControl : ChatPage
	{
		private const string SlapStringFormat = "slaps {0} around a bit with {1} {2} {3}!";
		private static Random _slapRandom = new Random((int)DateTime.Now.Ticks);

		private static readonly string[] AquaticLifeForms = new[]
			{
				"trout",
				"tuna fish",
				"goldfish",
				"shrimp",
				"shark",
				"whale",
				"jellyfish",
				"barnacle",
				"clam",
				"oyster",
				"prawn",
				"random crustacean",
				"crab",
				"lobster",
				"octopus",
				"halibut",
				"salmon",
				"smoked salmon",
				"cod",
				"crawfish",
				"starfish",
				"pepper seared ahi tuna",
				"blowfish",
				"fugu",
				"puffer fish"
			};

		private static readonly string[] Sizes = new[]
			{
				"microscopic",
				"teensy weensy",
				"tiny",
				"miniscule",
				"itty bitty",
				"planck length",
				"small",
				"pretty small",
				"medium-sized",
				"average-sized",
				"normal-sized",
				"pretty average",
				"regular-sized",
				"slightly smaller than average",
				"slightly larger than average",
				"pretty big",
				"big",
				"huge",
				"enormous",
				"gigantic",
				"uncommonly large",
				"remarkably huge",
				"fat",
				"skinny",
				"slim",
				"wide",
				"long",
				"short",
				"rotund",
				"nondescript"
			};

		private void ExecuteSlap(object sender, ExecutedRoutedEventArgs e)
		{
			var nick = e.Parameter as string;
			var size = Sizes[_slapRandom.Next(Sizes.Length)];

			string slapString = string.Format(
				SlapStringFormat,
				nick,
				(size[0] == 'a' || size[0] == 'e' || size[0] == 'i' || size[0] == 'o' || size[0] == 'u') ? "an" : "a",
				size,
				AquaticLifeForms[_slapRandom.Next(AquaticLifeForms.Length)]
				);

			if (this.IsConnected)
			{
				this.Session.SendCtcp(this.Target, new CtcpCommand("ACTION", slapString.Split(' ')), false);
				this.Write("Own", string.Format("{0} {1}", this.Session.Nickname, slapString));
			}
		}
	}
}
