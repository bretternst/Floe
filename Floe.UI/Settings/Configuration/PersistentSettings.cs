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
		public string BasePath { get { return Path.GetDirectoryName(_exeConfig.FilePath); } }
		public bool IsFirstLaunch { get; private set; }

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
			this.IsFirstLaunch = !File.Exists(path);
			map.RoamingUserConfigFilename = path;

			try
			{
				_exeConfig = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.PerUserRoaming);
				_prefConfigSection = _exeConfig.GetSection(SettingsConfigSectionName) as SettingsSection;
				if (_prefConfigSection == null)
				{
					_prefConfigSection = new SettingsSection();
					_prefConfigSection.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToLocalUser;
					_exeConfig.Sections.Add(SettingsConfigSectionName, _prefConfigSection);
				}
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(string.Format("Unable to load user configuration: {0}. You may want to delete the configuration file and try again.",
					ex.Message, path));
				Environment.Exit(-1);
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
