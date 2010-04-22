using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Floe.Configuration;

namespace Floe.UI.Settings
{
	public partial class UserSettingsControl : UserControl, IValidationPanel
	{
		public bool IsValid
		{
			get
			{
				return !Validation.GetHasError(txtNickname);
			}
		}

		public UserSettingsControl()
		{
			InitializeComponent();
		}
	}
}
