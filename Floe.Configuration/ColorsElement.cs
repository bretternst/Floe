using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows.Media;
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

		[ConfigurationProperty("newMarker", DefaultValue = "#FF002B00")]
		public string NewMarker
		{
			get { return (string)this["newMarker"]; }
			set { this["newMarker"] = value; OnPropertyChanged("NewMarker"); }
		}

		[ConfigurationProperty("oldMarker", DefaultValue = "#FF3C0000")]
		public string OldMarker
		{
			get { return (string)this["oldMarker"]; }
			set { this["oldMarker"] = value; OnPropertyChanged("OldMarker"); }
		}

		[ConfigurationProperty("attention", DefaultValue = "#404000")]
		public string Attention
		{
			get { return (string)this["attention"]; }
			set { this["attention"] = value; OnPropertyChanged("Attention"); }
		}

		[ConfigurationProperty("noiseActivity", DefaultValue = "#647491")]
		public string NoiseActivity
		{
			get { return (string)this["noiseActivity"]; }
			set { this["noiseActivity"] = value; OnPropertyChanged("NoiseActivity"); }
		}

		[ConfigurationProperty("chatActivity", DefaultValue = "#A58F5A")]
		public string ChatActivity
		{
			get { return (string)this["chatActivity"]; }
			set { this["chatActivity"] = value; OnPropertyChanged("ChatActivity"); }
		}

		[ConfigurationProperty("alertActivity", DefaultValue = "#FFFF00")]
		public string Alert
		{
			get { return (string)this["alertActivity"]; }
			set { this["alertActivity"] = value; OnPropertyChanged("Alert"); }
		}

		[ConfigurationProperty("windowBackground", DefaultValue = "#293955")]
		public string WindowBackground
		{
			get { return (string)this["windowBackground"]; }
			set { this["windowBackground"] = value; OnPropertyChanged("WindowBackground"); }
		}

		[ConfigurationProperty("windowForeground", DefaultValue = "White")]
		public string WindowForeground
		{
			get { return (string)this["windowForeground"]; }
			set { this["windowForeground"] = value; OnPropertyChanged("WindowForeground"); }
		}

		[ConfigurationProperty("highlight", DefaultValue = "#3399FF")]
		public string Highlight
		{
			get { return (string)this["highlight"]; }
			set { this["highlight"] = value; OnPropertyChanged("Highlight"); }
		}

		[ConfigurationProperty("color0", DefaultValue = "#FFFFFF")]
		public string Color0
		{
			get { return (string)this["color0"]; }
			set { this["color0"] = value; OnPropertyChanged("Palette"); }
		}

		[ConfigurationProperty("color1", DefaultValue = "#000000")]
		public string Color1
		{
			get { return (string)this["color1"]; }
			set { this["color1"] = value; OnPropertyChanged("Palette"); }
		}

		[ConfigurationProperty("color2", DefaultValue = "#00007F")]
		public string Color2
		{
			get { return (string)this["color2"]; }
			set { this["color2"] = value; OnPropertyChanged("Palette"); }
		}

		[ConfigurationProperty("color3", DefaultValue = "#009300")]
		public string Color3
		{
			get { return (string)this["color3"]; }
			set { this["color3"] = value; OnPropertyChanged("Palette"); }
		}

		[ConfigurationProperty("color4", DefaultValue = "#FF0000")]
		public string Color4
		{
			get { return (string)this["color4"]; }
			set { this["color4"] = value; OnPropertyChanged("Palette"); }
		}

		[ConfigurationProperty("color5", DefaultValue = "#7F0000")]
		public string Color5
		{
			get { return (string)this["color5"]; }
			set { this["color5"] = value; OnPropertyChanged("Palette"); }
		}

		[ConfigurationProperty("color6", DefaultValue = "#9C009C")]
		public string Color6
		{
			get { return (string)this["color6"]; }
			set { this["color6"] = value; OnPropertyChanged("Palette"); }
		}

		[ConfigurationProperty("color7", DefaultValue = "#FC7F00")]
		public string Color7
		{
			get { return (string)this["color7"]; }
			set { this["color7"] = value; OnPropertyChanged("Palette"); }
		}

		[ConfigurationProperty("color8", DefaultValue = "#FFFF00")]
		public string Color8
		{
			get { return (string)this["color8"]; }
			set { this["color8"] = value; OnPropertyChanged("Palette"); }
		}

		[ConfigurationProperty("color9", DefaultValue = "#00FC00")]
		public string Color9
		{
			get { return (string)this["color9"]; }
			set { this["color9"] = value; OnPropertyChanged("Palette"); }
		}

		[ConfigurationProperty("color10", DefaultValue = "#009393")]
		public string Color10
		{
			get { return (string)this["color10"]; }
			set { this["color10"] = value; OnPropertyChanged("Palette"); }
		}

		[ConfigurationProperty("color11", DefaultValue = "#00FFFF")]
		public string Color11
		{
			get { return (string)this["color11"]; }
			set { this["color11"] = value; OnPropertyChanged("Palette"); }
		}

		[ConfigurationProperty("color12", DefaultValue = "#0000FC")]
		public string Color12
		{
			get { return (string)this["color12"]; }
			set { this["color12"] = value; OnPropertyChanged("Palette"); }
		}

		[ConfigurationProperty("color13", DefaultValue = "#FF00FF")]
		public string Color13
		{
			get { return (string)this["color13"]; }
			set { this["color13"] = value; OnPropertyChanged("Palette"); }
		}

		[ConfigurationProperty("color14", DefaultValue = "#7F7F7F")]
		public string Color14
		{
			get { return (string)this["color14"]; }
			set { this["color14"] = value; OnPropertyChanged("Palette"); }
		}

		[ConfigurationProperty("color15", DefaultValue = "#D2D2D2")]
		public string Color15
		{
			get { return (string)this["color15"]; }
			set { this["color15"] = value; OnPropertyChanged("Palette"); }
		}

		[ConfigurationProperty("transmit", DefaultValue = "#00FF00")]
		public string Transmit
		{
			get { return (string)this["transmit"]; }
			set { this["transmit"] = value; OnPropertyChanged("Transmit"); }
		}
		
		public ChatPalette Palette
		{
			get
			{
				var converter = new BrushConverter();
				var palette = new ChatPalette(converter.ConvertFromString(this.Default) as SolidColorBrush);

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

				palette.Add("Color0", converter.ConvertFromString(this.Color0) as SolidColorBrush);
				palette.Add("Color1", converter.ConvertFromString(this.Color1) as SolidColorBrush);
				palette.Add("Color2", converter.ConvertFromString(this.Color2) as SolidColorBrush);
				palette.Add("Color3", converter.ConvertFromString(this.Color3) as SolidColorBrush);
				palette.Add("Color4", converter.ConvertFromString(this.Color4) as SolidColorBrush);
				palette.Add("Color5", converter.ConvertFromString(this.Color5) as SolidColorBrush);
				palette.Add("Color6", converter.ConvertFromString(this.Color6) as SolidColorBrush);
				palette.Add("Color7", converter.ConvertFromString(this.Color7) as SolidColorBrush);
				palette.Add("Color8", converter.ConvertFromString(this.Color8) as SolidColorBrush);
				palette.Add("Color9", converter.ConvertFromString(this.Color9) as SolidColorBrush);
				palette.Add("Color10", converter.ConvertFromString(this.Color10) as SolidColorBrush);
				palette.Add("Color11", converter.ConvertFromString(this.Color11) as SolidColorBrush);
				palette.Add("Color12", converter.ConvertFromString(this.Color12) as SolidColorBrush);
				palette.Add("Color13", converter.ConvertFromString(this.Color13) as SolidColorBrush);
				palette.Add("Color14", converter.ConvertFromString(this.Color14) as SolidColorBrush);
				palette.Add("Color15", converter.ConvertFromString(this.Color15) as SolidColorBrush);

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
