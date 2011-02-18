using System;
using Floe.Audio.Interop;

namespace Floe.Audio
{
	public class AudioDevice
	{
		private IMMDevice _device;
		private IAudioClient _client;

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
			var iid = Interfaces.IAudioclient;
			object obj;
			_device.Activate(ref iid, ClsCtx.All, IntPtr.Zero, out obj);
			_client = obj as IAudioClient;
		}

		public void Initialize(AudioChannels channels, long sampleRate, BitsPerSample bitsPerSample, bool exclusive = false)
		{
			var fmt = new WaveFormat((ushort)channels, (uint)sampleRate, (ushort)bitsPerSample);
			var sessionId = Guid.Empty;
			_client.Initialize(exclusive ? AudioShareMode.Exclusive : AudioShareMode.Shared,
				AudioStreamFlags.None, 10000000, 0, ref fmt, ref sessionId);
		}

		public OutputStream GetOutputStream()
		{
			return new OutputStream(_client);
		}

		public InputStream GetInputStream()
		{
			return new InputStream(_client);
		}

		public static AudioDevice DefaultOutputDevice { get; private set; }
		public static AudioDevice DefaultInputDevice { get; private set; }

		public static event EventHandler<DeviceChangedEventArgs> DefaultOutputDeviceChanged;
		public static event EventHandler<DeviceChangedEventArgs> DefaultInputDeviceChanged;

		static AudioDevice()
		{
			MMDeviceEnumerator.Current.DefaultDeviceChanged += Current_DefaultDeviceChanged;
			DefaultOutputDevice = GetDefaultDevice(DataFlow.Reader);
			DefaultInputDevice = GetDefaultDevice(DataFlow.Capture);
		}

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
