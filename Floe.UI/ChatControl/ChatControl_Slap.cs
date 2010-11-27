using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatControl : UserControl
	{
		private const string SlapStringFormat = "slaps {0} around a bit with a {1} {2}!";
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
				"rotund"
			};

		private void ExecuteSlap(object sender, ExecutedRoutedEventArgs e)
		{
			var nick = e.Parameter as string;
			string slapString = string.Format(
				SlapStringFormat,
				nick,
				Sizes[_slapRandom.Next(Sizes.Length)],
				AquaticLifeForms[_slapRandom.Next(AquaticLifeForms.Length)]
				);

			this.Session.SendCtcp(this.Target, new CtcpCommand("ACTION", slapString.Split(' ')), false);
			this.Write("Own", string.Format("{0} {1}", this.Session.Nickname, slapString));
		}
	}
}
