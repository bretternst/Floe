using System;
using System.ComponentModel;
using System.Configuration;

using Floe.Audio;
using Floe.Interop;

namespace Floe.Configuration
{
	public class VoiceElement : ConfigurationElement, INotifyPropertyChanged
	{
		[ConfigurationProperty("quality", DefaultValue=VoiceQuality.High)]
		public VoiceQuality Quality
		{
			get { return (VoiceQuality)this["quality"]; }
			set { this["quality"] = value; }
		}

		[ConfigurationProperty("pushToTalk", DefaultValue=true)]
		public bool PushToTalk
		{
			get { return (bool)this["pushToTalk"]; }
			set { this["pushToTalk"] = value; }
		}

		[ConfigurationProperty("talkKey", DefaultValue=InputButton.MouseButton4)]
		public InputButton TalkKey
		{
			get { return (InputButton)this["talkKey"]; }
			set { this["talkKey"] = value; }
		}

		[ConfigurationProperty("useStun", DefaultValue = true)]
		public bool UseStun
		{
			get { return (bool)this["useStun"]; }
			set { this["useStun"] = value; }
		}

		[ConfigurationProperty("stunServer", DefaultValue = "stun.failurefiles.com")]
		public string StunServer
		{
			get { return (string)this["stunServer"]; }
			set { this["stunServer"] = value; }
		}

		[ConfigurationProperty("altStunServer", DefaultValue = "")]
		public string AltStunServer
		{
			get { return (string)this["altStunServer"]; }
			set { this["altStunServer"] = value; }
		}

		[ConfigurationProperty("playbackVolume", DefaultValue = 1f)]
		public float PlaybackVolume
		{
			get { return (float)this["playbackVolume"]; }
			set { this["playbackVolume"] = value; OnPropertyChanged("PlaybackVolume"); }
		}

		[ConfigurationProperty("captureVolume", DefaultValue = 1f)]
		public float CaptureVolume
		{
			get { return (float)this["captureVolume"]; }
			set { this["captureVolume"] = value; OnPropertyChanged("CaptureVolume"); }
		}

		[ConfigurationProperty("talkLevel", DefaultValue = 0.2f)]
		public float TalkLevel
		{
			get { return (float)this["talkLevel"]; }
			set { this["talkLevel"] = value; }
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
