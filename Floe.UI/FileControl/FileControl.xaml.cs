using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Threading;
using Microsoft.Win32;
using Floe.Net;

namespace Floe.UI
{
	public enum FileStatus
	{
		Asking,
		Working,
		Cancelled,
		Finished
	}

	public partial class FileControl : ChatPage
	{
		private FileInfo _fileInfo;
		private DccOperation _dcc;
		private Timer _pollTimer;
		private IPAddress _address;
		private int _port;

		public static readonly DependencyProperty DescriptionProperty =
			DependencyProperty.Register("Description", typeof(string), typeof(FileControl));
		public string Description
		{
			get { return (string)this.GetValue(DescriptionProperty); }
			set { this.SetValue(DescriptionProperty, value); }
		}

		public static readonly DependencyProperty FileSizeProperty =
			DependencyProperty.Register("FileSize", typeof(long), typeof(FileControl));
		public long FileSize
		{
			get { return (long)this.GetValue(FileSizeProperty); }
			set { this.SetValue(FileSizeProperty, value); }
		}

		public static readonly DependencyProperty BytesTransferredProperty =
			DependencyProperty.Register("BytesTransferred", typeof(long), typeof(FileControl));
		public long BytesTransferred
		{
			get { return (long)this.GetValue(BytesTransferredProperty); }
			set { this.SetValue(BytesTransferredProperty, value); }
		}

		public static readonly DependencyProperty SpeedProperty =
			DependencyProperty.Register("Speed", typeof(long), typeof(FileControl));
		public string Speed
		{
			get { return (string)this.GetValue(SpeedProperty); }
			set { this.SetValue(SpeedProperty, value); }
		}

		public static readonly DependencyProperty StatusProperty =
			DependencyProperty.Register("Status", typeof(FileStatus), typeof(FileControl));
		public FileStatus Status
		{
			get { return (FileStatus)this.GetValue(StatusProperty); }
			set { this.SetValue(StatusProperty, value); }
		}

		public static readonly DependencyProperty StatusTextProperty =
			DependencyProperty.Register("StatusText", typeof(string), typeof(FileControl));
		public string StatusText
		{
			get { return (string)this.GetValue(StatusTextProperty); }
			set { this.SetValue(StatusTextProperty, value); }
		}

		public FileControl(IrcSession session, IrcTarget target)
			: base(ChatPageType.DccFile, session, target, "DCC")
		{
			InitializeComponent();
			this.Id = "dcc-file";
			this.Header = this.Title = string.Format("DCC [{0}]", target.Name);
		}

		public int StartSend(FileInfo file)
		{
			_fileInfo = file;
			this.Description = string.Format("Sending {0}...", file.Name);
			this.FileSize = file.Length;
			this.StatusText = "Listening for connection";
			this.Status = FileStatus.Working;

			_dcc = new DccXmitSender(_fileInfo, (action) => this.Dispatcher.BeginInvoke(action));
			_dcc.Connected += dcc_Connected;
			_dcc.Disconnected += dcc_Disconnected;
			_dcc.Error += dcc_Error;
			try
			{
				_port = _dcc.Listen(App.Settings.Current.Dcc.LowPort, App.Settings.Current.Dcc.HighPort);
			}
			catch (InvalidOperationException)
			{
				this.Status = FileStatus.Cancelled;
				this.StatusText = "Error: No ports available";
				_port = 0;
			}
			return _port;
		}

		public void StartReceive(IPAddress address, int port, string name, long size)
		{
			_fileInfo = new FileInfo(Path.Combine(App.Settings.Current.Dcc.DownloadFolder, name));
			_address = address;
			_port = port;
			this.Description = string.Format("Receiving {0}...", name);
			this.FileSize = size;
			this.StatusText = "Waiting for confirmation";
			this.Status = FileStatus.Asking;

			if (App.Settings.Current.Dcc.AutoAccept)
			{
				this.Accept();
			}
		}

		private void Accept()
		{
			this.Status = FileStatus.Working;
			this.StatusText = "Connecting";
			_dcc = new DccXmitReceiver(_fileInfo, (action) => this.Dispatcher.BeginInvoke(action));
			_dcc.Connect(_address, _port);
			_dcc.Connected += dcc_Connected;
			_dcc.Disconnected += dcc_Disconnected;
			_dcc.Error += dcc_Error;
		}

		private void dcc_Connected(object sender, EventArgs e)
		{
			this.StatusText = "Transferring";
			_pollTimer = new Timer((o) =>
				{
					this.Dispatcher.BeginInvoke((Action)(() =>
						{
							this.BytesTransferred = _dcc.BytesTransferred;
						}));
				}, null, 250, 250);
		}

		private void UpdateProgress()
		{
			this.BytesTransferred = _dcc.BytesTransferred;
		}

		private void dcc_Disconnected(object sender, EventArgs e)
		{
			this.Status = FileStatus.Finished;
			this.StatusText = "Finished";
			_pollTimer.Dispose();
		}

		private void dcc_Error(object sender, Floe.Net.ErrorEventArgs e)
		{
			this.Status = FileStatus.Cancelled;
			this.StatusText = "Error: " + e.Exception.Message;
			_pollTimer.Dispose();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Status = FileStatus.Cancelled;
			this.StatusText = "Cancelled";
			if (_dcc != null)
			{
				_dcc.Close();
			}
		}

		private void btnOpen_Click(object sender, RoutedEventArgs e)
		{
			if (_fileInfo != null && _fileInfo.Exists)
			{
				App.BrowseTo(_fileInfo.FullName);
			}
		}

		private void btnOpenFolder_Click(object sender, RoutedEventArgs e)
		{
			if (_fileInfo != null)
			{
				App.BrowseTo(_fileInfo.DirectoryName);
			}
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			this.Accept();
		}

		private void btnSaveAs_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new SaveFileDialog();
			dialog.InitialDirectory = App.Settings.Current.Dcc.DownloadFolder;
			dialog.FileName = _fileInfo.Name;
			if (dialog.ShowDialog(Window.GetWindow(this)) == true)
			{
				_fileInfo = new FileInfo(dialog.FileName);
				this.Description = string.Format("Receiving {0}...", dialog.SafeFileName);
				this.Accept();
			}
		}

		private void btnDecline_Click(object sender, RoutedEventArgs e)
		{
			this.Status = FileStatus.Cancelled;
			this.StatusText = "Declined";
			this.Session.SendCtcp(this.Target, new CtcpCommand("ERRMSG", "DCC", "XMIT", "declined"), true);
		}

		public void Dispose()
		{
			if (_dcc != null)
			{
				_dcc.Close();
			}
		}
	}
}
