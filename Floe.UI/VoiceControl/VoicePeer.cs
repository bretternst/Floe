using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.UI
{
	internal class VoicePeer
	{
		public NicknameItem User { get; set; }
		public long LastTransmit { get; set; }
		public bool IsMuted { get; set; }
		public bool IsTalking { get; set; }
	}
}
