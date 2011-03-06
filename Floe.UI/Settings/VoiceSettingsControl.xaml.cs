using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using Floe.Interop;

namespace Floe.UI.Settings
{
	public partial class VoiceSettingsControl : UserControl
	{
		private AudioCaptureClient _capture;
		private AudioMeter _meter;
		private Timer _timer;

		public VoiceSettingsControl()
		{
			InitializeComponent();
			this.Loaded += new System.Windows.RoutedEventHandler(VoiceSettingsControl_Loaded);
			this.Unloaded += new System.Windows.RoutedEventHandler(VoiceSettingsControl_Unloaded);
			_capture = new AudioCaptureClient(AudioDevice.DefaultCaptureDevice, 1000, 0, new WaveFormatPcm(44100, 16, 1));
			_meter = new AudioMeter(AudioDevice.DefaultCaptureDevice);
		}

		private void VoiceSettingsControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			_capture.Start();
			_timer = new Timer((o) =>
				{
					this.Dispatcher.BeginInvoke((Action)(() => prgMicLevel.Value = _meter.Peak));
				}, null, 50, 50);
		}

		private void VoiceSettingsControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
		{
			_timer.Dispose();
			_capture.Stop();
		}

		private void btnTalkKey_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			btnTalkKey.Content = "Press a key...";
			RawInput.ButtonDown += RawInput_ButtonDown;
		}

		private void RawInput_ButtonDown(object sender, RawInputEventArgs e)
		{
			btnTalkKey.Content = App.Settings.Current.Voice.TalkKey = e.Button.ToString();
			RawInput.ButtonDown -= RawInput_ButtonDown;
			e.Handled = true;
		}
	}
}
