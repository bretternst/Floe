using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Floe.UI.Settings
{
	internal interface IValidationPanel
	{
		bool IsValid { get; }
	}
}
