using System;
using System.Configuration;
using System.Windows.Media;
using System.Collections.Generic;
using System.ComponentModel;

using Floe.UI;

namespace Floe.Configuration
{
	public class ColorsElement : ConfigurationElement, INotifyPropertyChanged
	{
		[ConfigurationProperty("background", DefaultValue = "Black")]
		public string Background
		{
			get { return (string)this["background"]; }
			set { this["background"] = value; OnPropertyChanged("Background"); }
		}

		[ConfigurationProperty("editBackground", DefaultValue = "Black")]
		public string EditBackground
		{
			get { return (string)this["editBackground"]; }
			set { this["editBackground"] = value; OnPropertyChanged("EditBackground"); }
		}

		[ConfigurationProperty("edit", DefaultValue = "White")]
		public string Edit
		{
			get { return (string)this["edit"]; }
			set { this["edit"] = value; OnPropertyChanged("Edit"); }
		}

		[ConfigurationProperty("default", DefaultValue = "White")]
		public string Default
		{
			get { return (string)this["default"]; }
			set { this["default"] = value; OnPropertyChanged("Default"); }
		}

		[ConfigurationProperty("action", DefaultValue = "White")]
		public string Action
		{
			get { return (string)this["action"]; }
			set { this["action"] = value; OnPropertyChanged("Action"); }
		}

		[ConfigurationProperty("ctcp", DefaultValue = "Yellow")]
		public string Ctcp
		{
			get { return (string)this["ctcp"]; }
			set { this["ctcp"] = value; OnPropertyChanged("Ctcp"); }
		}

		[ConfigurationProperty("info", DefaultValue = "White")]
		public string Info
		{
			get { return (string)this["info"]; }
			set { this["info"] = value; OnPropertyChanged("Info"); }
		}

		[ConfigurationProperty("invite", DefaultValue = "Yellow")]
		public string Invite
		{
			get { return (string)this["invite"]; }
			set { this["invite"] = value; OnPropertyChanged("Invite"); }
		}

		[ConfigurationProperty("join", DefaultValue = "Lavender")]
		public string Join
		{
			get { return (string)this["join"]; }
			set { this["join"] = value; OnPropertyChanged("Join"); }
		}

		[ConfigurationProperty("kick", DefaultValue = "Lavender")]
		public string Kick
		{
			get { return (string)this["kick"]; }
			set { this["kick"] = value; OnPropertyChanged("Kick"); }
		}

		[ConfigurationProperty("mode", DefaultValue = "Yellow")]
		public string Mode
		{
			get { return (string)this["mode"]; }
			set { this["mode"] = value; OnPropertyChanged("Mode"); }
		}

		[ConfigurationProperty("nick", DefaultValue = "Yellow")]
		public string Nick
		{
			get { return (string)this["nick"]; }
			set { this["nick"] = value; OnPropertyChanged("Nick"); }
		}

		[ConfigurationProperty("notice", DefaultValue = "Yellow")]
		public string Notice
		{
			get { return (string)this["notice"]; }
			set { this["notice"] = value; OnPropertyChanged("Notice"); }
		}

		[ConfigurationProperty("own", DefaultValue = "Gray")]
		public string Own
		{
			get { return (string)this["own"]; }
			set { this["own"] = value; OnPropertyChanged("Own"); }
		}

		[ConfigurationProperty("part", DefaultValue = "Lavender")]
		public string Part
		{
			get { return (string)this["part"]; }
			set { this["part"] = value; OnPropertyChanged("Part"); }
		}

		[ConfigurationProperty("quit", DefaultValue = "Lavender")]
		public string Quit
		{
			get { return (string)this["quit"]; }
			set { this["quit"] = value; OnPropertyChanged("Quit"); }
		}

		[ConfigurationProperty("topic", DefaultValue = "Yellow")]
		public string Topic
		{
			get { return (string)this["topic"]; }
			set { this["topic"] = value; OnPropertyChanged("Topic"); }
		}

		[ConfigurationProperty("error", DefaultValue = "Red")]
		public string Error
		{
			get { return (string)this["error"]; }
			set { this["error"] = value; OnPropertyChanged("Error"); }
		}

		public ChatPalette Palette
		{
			get
			{
				var palette = new ChatPalette(Brushes.White);
				var converter = new BrushConverter();

				palette.Add("Default", converter.ConvertFromString(this.Default) as SolidColorBrush);
				palette.Add("Action", converter.ConvertFromString(this.Action) as SolidColorBrush);
				palette.Add("Ctcp", converter.ConvertFromString(this.Ctcp) as SolidColorBrush);
				palette.Add("Info", converter.ConvertFromString(this.Info) as SolidColorBrush);
				palette.Add("Invite", converter.ConvertFromString(this.Invite) as SolidColorBrush);
				palette.Add("Join", converter.ConvertFromString(this.Join) as SolidColorBrush);
				palette.Add("Kick", converter.ConvertFromString(this.Kick) as SolidColorBrush);
				palette.Add("Mode", converter.ConvertFromString(this.Mode) as SolidColorBrush);
				palette.Add("Nick", converter.ConvertFromString(this.Nick) as SolidColorBrush);
				palette.Add("Notice", converter.ConvertFromString(this.Notice) as SolidColorBrush);
				palette.Add("Own", converter.ConvertFromString(this.Own) as SolidColorBrush);
				palette.Add("Part", converter.ConvertFromString(this.Part) as SolidColorBrush);
				palette.Add("Quit", converter.ConvertFromString(this.Quit) as SolidColorBrush);
				palette.Add("Topic", converter.ConvertFromString(this.Topic) as SolidColorBrush);
				palette.Add("Error", converter.ConvertFromString(this.Error) as SolidColorBrush);

				return palette;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			var handler = this.PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
				handler(this, new PropertyChangedEventArgs("Palette"));
			}
		}
	}
}
