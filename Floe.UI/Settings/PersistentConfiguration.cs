using System;
using System.IO;
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
			var map = new ExeConfigurationFileMap();
			map.ExeConfigFilename = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Floe.UI.App.Product);
			path = Path.Combine(path, string.Format("{0}.config", Floe.UI.App.Product));
			map.RoamingUserConfigFilename = path;

			_exeConfig = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.PerUserRoaming);
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
