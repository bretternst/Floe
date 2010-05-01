using System;
using System.IO;
using System.Configuration;
using System.ComponentModel;

namespace Floe.Configuration
{
	public sealed class PersistentSettings : INotifyPropertyChanged
	{
		private const string SettingsConfigSectionName = "floe.settings";

		private System.Configuration.Configuration _exeConfig;
		private SettingsSection _prefConfigSection;

		public SettingsSection Current { get { return _prefConfigSection; } }

		public PersistentSettings()
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
			_prefConfigSection = _exeConfig.GetSection(SettingsConfigSectionName) as SettingsSection;
			if (_prefConfigSection == null)
			{
				_prefConfigSection = new SettingsSection();
				_prefConfigSection.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToLocalUser;
				_exeConfig.Sections.Add(SettingsConfigSectionName, _prefConfigSection);
			}

			this.OnPropertyChanged("Current");
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string name)
		{
			var handler = this.PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}
