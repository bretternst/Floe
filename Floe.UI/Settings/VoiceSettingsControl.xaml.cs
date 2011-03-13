using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using Floe.Interop;

using Floe.Audio;

namespace Floe.UI.Settings
{
	public partial class VoiceSettingsControl : UserControl
	{
		private AudioCaptureClient _capture;
		private AudioMeter _meter;
		private Timer _timer;
		private VoiceLoopback _loopback;

		public VoiceSettingsControl()
		{
			InitializeComponent();
			this.Unloaded += new RoutedEventHandler(VoiceSettingsControl_Unloaded);
			_capture = new AudioCaptureClient(AudioDevice.DefaultCaptureDevice, 1000, 0, new WaveFormatPcm(44100, 16, 1));
			_meter = new AudioMeter(AudioDevice.DefaultCaptureDevice);
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property == Control.VisibilityProperty)
			{
				if ((Visibility)e.NewValue == Visibility.Visible)
				{
					_capture.Start();
					_timer = new Timer((o) =>
					{
						this.Dispatcher.BeginInvoke((Action)(() => prgMicLevel.Value = _meter.Peak));
					}, null, 25, 25);
				}
				else
				{
					if (_timer != null)
					{
						_timer.Dispose();
						_capture.Stop();
					}
				}
			}
		}

		private void btnTalkKey_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			btnTalkKey.Content = "Press a key...";
			RawInput.ButtonDown += RawInput_ButtonDown;
		}

		private void RawInput_ButtonDown(object sender, RawInputEventArgs e)
		{
			btnTalkKey.Content = App.Settings.Current.Voice.TalkKey = e.Button;
			RawInput.ButtonDown -= RawInput_ButtonDown;
			e.Handled = true;
		}

		private void btnLoopback_Checked(object sender, RoutedEventArgs e)
		{
			_loopback = new VoiceLoopback(VoiceCodec.Gsm610, App.Settings.Current.Voice.Quality);
			SetVolume();
			_loopback.Start();
		}

		private void btnLoopback_Unchecked(object sender, RoutedEventArgs e)
		{
			_loopback.Dispose();
			_loopback = null;
		}

		private void VoiceSettingsControl_Unloaded(object sender, RoutedEventArgs e)
		{
			_capture.Dispose();
			if (_timer != null)
			{
				_timer.Dispose();
			}
			if (_loopback != null)
			{
				_loopback.Dispose();
				_loopback = null;
			}
		}

		private void sldLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_loopback != null)
			{
				SetVolume();
			}
		}

		private void SetVolume()
		{
			_loopback.RenderVolume = (float)sldRenderLevel.Value;
			_loopback.CaptureVolume = (float)sldCaptureLevel.Value;
		}
	}
}
