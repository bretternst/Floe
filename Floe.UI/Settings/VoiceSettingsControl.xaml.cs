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
		private WaveInMeter _meter;
		private VoiceLoopback _loopback;

		public VoiceSettingsControl()
		{
			InitializeComponent();
			this.Unloaded += new RoutedEventHandler(VoiceSettingsControl_Unloaded);
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property == Control.VisibilityProperty)
			{
				if ((Visibility)e.NewValue == Visibility.Visible)
				{
					_meter = new WaveInMeter(2500);
					_meter.LevelUpdated += (sender, eLevel) =>
						{
							this.Dispatcher.BeginInvoke((Action)(() => prgMicLevel.Value = eLevel.Level));
						};
				}
				else
				{
					if (_meter != null)
					{
						_meter.Dispose();
						_meter = null;
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
			this.SetVolume();
			_loopback.Start();
		}

		private void btnLoopback_Unchecked(object sender, RoutedEventArgs e)
		{
			_loopback.Dispose();
			_loopback = null;
		}

		private void VoiceSettingsControl_Unloaded(object sender, RoutedEventArgs e)
		{
			if (_meter != null)
			{
				_meter.Dispose();
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
				this.SetVolume();
			}
		}

		private void SetVolume()
		{
			_loopback.RenderVolume = (float)sldRenderLevel.Value;
			_loopback.InputGain = (float)sldMicGain.Value;
			_loopback.OutputGain = (float)sldOutGain.Value;
		}
	}
}
