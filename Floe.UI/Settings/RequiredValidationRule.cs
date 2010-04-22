using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Floe.UI
{
	public class RequiredValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
		{
			return new ValidationResult(value.ToString().Trim().Length > 0, null);
		}
	}
}
