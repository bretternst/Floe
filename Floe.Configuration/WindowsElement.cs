using System;
using System.ComponentModel;
using System.Configuration;

namespace Floe.Configuration
{
	public class WindowsElement : ConfigurationElement, INotifyPropertyChanged
	{
		[ConfigurationProperty("placement")]
		public string Placement
		{
			get { return (string)this["placement"]; }
			set { this["placement"] = value; }
		}

		[ConfigurationProperty("customColors", DefaultValue = "")]
		public string CustomColors
		{
			get { return (string)this["customColors"]; }
			set { this["customColors"] = value; }
		}

		[ConfigurationProperty("activeOpacity", DefaultValue=1.0)]
		public double ActiveOpacity
		{
			get { return (double)this["activeOpacity"]; }
			set { this["activeOpacity"] = value; this.OnPropertyChanged("ActiveOpacity"); }
		}

		[ConfigurationProperty("inactiveOpacity", DefaultValue = 0.8)]
		public double InactiveOpacity
		{
			get { return (double)this["inactiveOpacity"]; }
			set { var val = Math.Round(value, 2); this["inactiveOpacity"] = val; this.OnPropertyChanged("InactiveOpacity"); }
		}

		[ConfigurationProperty("minimizeToSysTray", DefaultValue=false)]
		public bool MinimizeToSysTray
		{
			get { return (bool)this["minimizeToSysTray"]; }
			set { this["minimizeToSysTray"] = value; this.OnPropertyChanged("MinimizeToSysTray"); }
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
