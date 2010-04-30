using System;
using System.Configuration;
using System.ComponentModel;

namespace Floe.Configuration
{
	public class TextElement : ConfigurationElement, INotifyPropertyChanged
	{
		[ConfigurationProperty("bufferLines", DefaultValue=500)]
		[IntegerValidator(MinValue=100, MaxValue=int.MaxValue)]
		public int BufferLines
		{
			get { return (int)this["bufferLines"]; }
			set { this["bufferLines"] = value; this.OnPropertyChanged("BufferLines"); }
		}

		[ConfigurationProperty("inputHistory", DefaultValue = 10)]
		[IntegerValidator(MinValue = 1, MaxValue = 100)]
		public int InputHistory
		{
			get { return (int)this["inputHistory"]; }
			set { this["inputHistory"] = value; this.OnPropertyChanged("InputHistory"); }
		}

		[ConfigurationProperty("minimumCopyLength", DefaultValue=3)]
		[IntegerValidator(MinValue=1, MaxValue=100)]
		public int MinimumCopyLength
		{
			get { return (int)this["minimumCopyLength"]; }
			set { this["minimumCopyLength"] = value; this.OnPropertyChanged("MinimumCopyLength"); }
		}

		[ConfigurationProperty("fontFamily", DefaultValue = "Consolas")]
		public string FontFamily
		{
			get { return (string)this["fontFamily"]; }
			set { this["fontFamily"] = value; this.OnPropertyChanged("FontFamily"); }
		}

		[ConfigurationProperty("fontSize", DefaultValue = 14.0)]
		public double FontSize
		{
			get { return (double)this["fontSize"]; }
			set
			{
				if (value < 0.1 || value > 200.0)
				{
					throw new ArgumentException("Font size is not valid.");
				}
				this["fontSize"] = value;
				this.OnPropertyChanged("FontSize");
			}
		}

		[ConfigurationProperty("fontStyle", DefaultValue="Normal")]
		public string FontStyle
		{
			get { return (string)this["fontStyle"]; }
			set { this["fontStyle"] = value; this.OnPropertyChanged("FontStyle"); }
		}

		[ConfigurationProperty("fontWeight", DefaultValue="Black")]
		public string FontWeight
		{
			get { return (string)this["fontWeight"]; }
			set { this["fontWeight"] = value; this.OnPropertyChanged("FontWeight"); }
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
