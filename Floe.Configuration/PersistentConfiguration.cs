using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Floe.Configuration
{
	public sealed class PersistentConfiguration
	{
		private const string PreferencesConfigSectionName = "floe.preferences";

		private System.Configuration.Configuration _exeConfig;
		private PreferencesSection _prefConfigSection;

		public PreferencesSection Preferences { get { return _prefConfigSection; } }

		public PersistentConfiguration()
		{
			this.Load();
		}

		public void Save()
		{
			_exeConfig.Save();
		}

		public void Load()
		{
			_exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming);
			_prefConfigSection = _exeConfig.GetSection(PreferencesConfigSectionName) as PreferencesSection;
			if (_prefConfigSection == null)
			{
				_prefConfigSection = new PreferencesSection();
				_prefConfigSection.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToLocalUser;
				_exeConfig.Sections.Add(PreferencesConfigSectionName, _prefConfigSection);
			}
		}
	}
}
