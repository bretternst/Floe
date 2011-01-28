using System;
using System.Windows;
using System.Reflection;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using Floe.Configuration;
using Floe.Net;

namespace Floe.UI
{
	public partial class App : Application
	{
		public static PersistentSettings Settings { get; private set; }
		public static string Product { get; private set; }
		public static string HelpText { get; private set; }
		
		private static Lazy<ImageSource> appImage = new Lazy<ImageSource>(() =>
		{
			using (var stream = typeof(App).Assembly.GetManifestResourceStream(
				string.Format("{0}.Resources.App.ico", typeof(App).Namespace)))
			{
				return BitmapFrame.Create(stream);
			}
		});
		public static ImageSource ApplicationImage
		{
			get
			{
				return appImage.Value;
			}
		}

		private static Lazy<Icon> appIcon = new Lazy<Icon>(() =>
		{
			using (var stream = typeof(App).Assembly.GetManifestResourceStream(
				string.Format("{0}.Resources.App.ico", typeof(App).Namespace)))
			{
				return new Icon(stream);
			}
		});
		public static Icon ApplicationIcon
		{
			get
			{
				return appIcon.Value;
			}
		}

		public static string Version
		{
			get
			{
				return typeof(App).Assembly.GetName().Version.ToString();
			}
		}

		static App()
		{
			App.Product = typeof(App).Assembly.GetCustomAttributes(
					typeof(AssemblyProductAttribute), false).OfType<AssemblyProductAttribute>().FirstOrDefault().Product;

			try
			{
				App.Settings = new PersistentSettings(App.Product);
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(string.Format("Unable to load user configuration. You may want to delete the configuration file and try again.",
					ex.Message));
				Environment.Exit(-1);
			}

			App.RefreshAttentionPatterns();
			App.LoadIgnoreMasks();

			using (var sr = new System.IO.StreamReader(typeof(App).Assembly.GetManifestResourceStream(
				string.Format("{0}.Resources.Help.txt", typeof(App).Namespace))))
			{
				App.HelpText = sr.ReadToEnd();
			}
		}
	}
}
