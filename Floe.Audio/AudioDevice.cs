using System;
using Floe.Audio.Interop;

namespace Floe.Audio
{
	public class AudioDevice
	{
		private IMMDevice _device;

		public string DeviceId { get; private set; }
		public string Name { get; private set; }

		public bool IsActive
		{
			get
			{
				DeviceState state;
				_device.GetState(out state);
				return state == DeviceState.Active;
			}
		}

		internal AudioDevice(IMMDevice device)
		{
			_device = device;
			string id;
			_device.GetId(out id);
			this.DeviceId = id;
			IPropertyStore ips;
			_device.OpenPropertyStore(StorageAccessMode.Read, out ips);
			var pk = PropertyKeys.DeviceFriendlyName;
			PropertyVariant val;
			ips.GetValue(ref pk, out val);
			this.Name = val.Value.ToString();
		}

		public AudioOutputClient GetOutputClient(WaveFormat format)
		{
			return new AudioOutputClient(this.CreateClient(), format);
		}

		public AudioInputClient GetInputClient(WaveFormat format)
		{
			return new AudioInputClient(this.CreateClient(), format);
		}

		private IAudioClient CreateClient()
		{
			var iid = Interfaces.IAudioclient;
			object obj;
			_device.Activate(ref iid, ClsCtx.All, IntPtr.Zero, out obj);
			return obj as IAudioClient;
		}

		static AudioDevice()
		{
			MMDeviceEnumerator.Current.DefaultDeviceChanged += Current_DefaultDeviceChanged;
			DefaultOutputDevice = GetDefaultDevice(DataFlow.Reader);
			DefaultInputDevice = GetDefaultDevice(DataFlow.Capture);
		}

		public static AudioDevice DefaultOutputDevice { get; private set; }
		public static AudioDevice DefaultInputDevice { get; private set; }

		public static event EventHandler<DeviceChangedEventArgs> DefaultOutputDeviceChanged;
		public static event EventHandler<DeviceChangedEventArgs> DefaultInputDeviceChanged;

		private static void Current_DefaultDeviceChanged(object sender, MMNotificationEventArgs e)
		{
			if (e.Role == Role.Multimedia)
			{
				switch (e.DataFlow)
				{
					case DataFlow.Reader:
						DefaultOutputDevice = GetDefaultDevice(DataFlow.Reader);
						var handler = DefaultOutputDeviceChanged;
						if(handler != null)
						{
							handler(null, new DeviceChangedEventArgs(DefaultOutputDevice));
						}
						break;
					case DataFlow.Capture:
						DefaultOutputDevice = GetDefaultDevice(DataFlow.Capture);
						handler = DefaultInputDeviceChanged;
						if(handler != null)
						{
							handler(null, new DeviceChangedEventArgs(DefaultInputDevice));
						}
						break;
					default:
						return;
				}
			}
		}

		private static AudioDevice GetDefaultDevice(DataFlow dataFlow)
		{
			var imde = MMDeviceEnumerator.Current;
			IMMDevice immd;
			imde.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia, out immd);
			return new AudioDevice(immd);
		}
	}
}
