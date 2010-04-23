using System;
using System.Windows.Controls;

namespace Floe.UI.Settings
{
	public partial class UserSettingsControl : UserControl
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
