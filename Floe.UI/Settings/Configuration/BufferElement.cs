using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.ComponentModel;

namespace Floe.Configuration
{
	public class BufferElement : ConfigurationElement, INotifyPropertyChanged
	{
		[ConfigurationProperty("bufferLines", DefaultValue = 500)]
		[IntegerValidator(MinValue = 100, MaxValue = int.MaxValue)]
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

		[ConfigurationProperty("minimumCopyLength", DefaultValue = 3)]
		[IntegerValidator(MinValue = 1, MaxValue = 100)]
		public int MinimumCopyLength
		{
			get { return (int)this["minimumCopyLength"]; }
			set { this["minimumCopyLength"] = value; this.OnPropertyChanged("MinimumCopyLength"); }
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
