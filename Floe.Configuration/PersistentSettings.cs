using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;

namespace Floe.Configuration
{
	public sealed class PersistentSettings : INotifyPropertyChanged
	{
		private const string SettingsConfigSectionName = "floe.settings";

		private System.Configuration.Configuration _exeConfig;
		private SettingsSection _prefConfigSection;
		private string _appName;

		public SettingsSection Current { get { return _prefConfigSection; } }
		public string BasePath { get { return Path.GetDirectoryName(_exeConfig.FilePath); } }
		public bool IsFirstLaunch { get; private set; }

		public PersistentSettings(string appName)
		{
			_appName = appName;
			this.Load();
		}

		public void Save()
		{
			try
			{
				_exeConfig.Save();
			}
			catch (ConfigurationErrorsException)
			{
				var _oldSection = _exeConfig.Sections[SettingsConfigSectionName];
				_exeConfig.Sections.Remove(SettingsConfigSectionName);
				this.Load();
				_exeConfig.Sections.Remove(SettingsConfigSectionName);
				_exeConfig.Sections.Add(SettingsConfigSectionName, _oldSection);
				_exeConfig.Save();
			}
		}

		public void Load()
		{
			var map = new ExeConfigurationFileMap();
			map.ExeConfigFilename = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _appName);
#if DEBUG
			path = Path.Combine(path, string.Format("{0}.DEBUG.config", _appName));
#else
			path = Path.Combine(path, string.Format("{0}.config", _appName));
#endif
			this.IsFirstLaunch = !File.Exists(path);
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
