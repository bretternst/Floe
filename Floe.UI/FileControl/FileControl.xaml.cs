using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using Floe.Net;
using Microsoft.Win32;

namespace Floe.UI
{
	public enum FileStatus
	{
		Asking,
		Working,
		Cancelled,
		Received,
		Sent
	}

	public enum DccMethod
	{
		Send,
		Xmit
	}

	public partial class FileControl : ChatPage
	{
		private const int PollTime = 250;
		private const int SpeedUpdateInterval = 4;
		private const int SpeedWindow = 5;

		private FileInfo _fileInfo;
		private DccOperation _dcc;
		private Timer _pollTimer;
		private IPAddress _address;
		private int _port;
		private bool _isPortForwarding;

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
		public long Speed
		{
			get { return (long)this.GetValue(SpeedProperty); }
			set { this.SetValue(SpeedProperty, value); }
		}

		public static readonly DependencyProperty EstimatedTimeProperty =
			DependencyProperty.Register("EstimatedTime", typeof(int), typeof(FileControl));
		public int EstimatedTime
		{
			get { return (int)this.GetValue(EstimatedTimeProperty); }
			set { this.SetValue(EstimatedTimeProperty, value); }
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

		public static readonly DependencyProperty IsDangerousProperty =
			DependencyProperty.Register("IsDangerous", typeof(bool), typeof(FileControl));
		public bool IsDangerous
		{
			get { return (bool)this.GetValue(IsDangerousProperty); }
			set { this.SetValue(IsDangerousProperty, value); }
		}

		public static readonly DependencyProperty DccMethodProperty =
			DependencyProperty.Register("DccMethod", typeof(DccMethod), typeof(FileControl));
		public DccMethod DccMethod
		{
			get { return (DccMethod)this.GetValue(DccMethodProperty); }
			set { this.SetValue(DccMethodProperty, value); }
		}

		public FileControl(IrcSession session, IrcTarget target, DccMethod method)
			: base(ChatPageType.DccFile, session, target, "DCC")
		{
			this.DccMethod = method;
			InitializeComponent();
			this.Id = "dcc-file";
			this.Header = this.Title = string.Format("{0} [DCC]", target.Name);
		}

		public void StartSend(FileInfo file, Action<int> readyCallback)
		{
			_fileInfo = file;
			this.Header = string.Format("[SEND] {0}", this.Target.Name);
			this.Title = string.Format("{0} - [DCC {1}] Sending file {2}", App.Product, this.Target.Name, _fileInfo.Name);
			this.Description = string.Format("Sending {0}...", file.Name);
			this.FileSize = file.Length;
			this.Status = FileStatus.Working;

			switch (this.DccMethod)
			{
				case DccMethod.Send:
					_dcc = new DccSendSender(_fileInfo);
					break;
				case DccMethod.Xmit:
					_dcc = new DccXmitSender(_fileInfo);
					break;
			}
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

			if (App.Settings.Current.Dcc.EnableUpnp && NatHelper.IsAvailable)
			{
				this.StatusText = "Forwarding port";
				NatHelper.BeginAddForwardingRule(_port, System.Net.Sockets.ProtocolType.Tcp, "Floe DCC", (o) =>
					{
						this.Dispatcher.BeginInvoke((Action)(() =>
							{
								this.StatusText = "Listening for connection";
								readyCallback(_port);
							}));
					});
				_isPortForwarding = true;
			}
			else
			{
				this.StatusText = "Listening for connection";
				readyCallback(_port);
			}
		}

		public void StartReceive(IPAddress address, int port, string name, long size)
		{
			_fileInfo = new FileInfo(Path.Combine(App.Settings.Current.Dcc.DownloadFolder, name));
			this.Header = string.Format("[RECV] {0}", this.Target.Name);
			this.Title = string.Format("{0} - [DCC {1}] Receiving file {2}", App.Product, this.Target.Name, _fileInfo.Name);
			_address = address;
			_port = port;
			this.Description = string.Format("Receiving {0}...", name);
			this.FileSize = size;
			this.StatusText = "Waiting for confirmation";
			this.Status = FileStatus.Asking;

			this.CheckFileExtension(_fileInfo.Extension.StartsWith(".", StringComparison.Ordinal) && _fileInfo.Extension.Length > 1 ? _fileInfo.Extension.Substring(1) : _fileInfo.Extension);

			if (App.Settings.Current.Dcc.AutoAccept)
			{
				this.Accept(false);
			}
		}

		public override bool CanClose()
		{
			if(this.Status == FileStatus.Working)
			{
				return App.Confirm(Window.GetWindow(this), "Are you sure you want to cancel this transfer in progress?", "Confirm Close");
			}
			else if (this.Status == FileStatus.Asking)
			{
				this.Decline();
			}
			return true;
		}

		public override void Dispose()
		{
			base.Dispose();

			if (_dcc != null)
			{
				_dcc.Dispose();
			}
		}

		private void CheckFileExtension(string ext)
		{
			string[] extensions = App.Settings.Current.Dcc.DangerExtensions.ToUpperInvariant().Split(' ');
			this.IsDangerous = extensions.Contains(ext.ToUpperInvariant());
		}

		private void Accept(bool forceOverwrite)
		{
			this.Status = FileStatus.Working;
			this.StatusText = "Connecting";
			switch (this.DccMethod)
			{
				case DccMethod.Send:
					_dcc = new DccSendReceiver(_fileInfo) { ForceOverwrite = forceOverwrite };
					break;
				case DccMethod.Xmit:
					_dcc = new DccXmitReceiver(_fileInfo) { ForceOverwrite = forceOverwrite, ForceResume = chkForceResume.IsChecked == true };
					break;
			}
			_dcc.Connect(_address, _port);
			_dcc.Connected += dcc_Connected;
			_dcc.Disconnected += dcc_Disconnected;
			_dcc.Error += dcc_Error;
		}

		private void Decline()
		{
			this.Status = FileStatus.Cancelled;
			this.StatusText = "Declined";
			this.Session.SendCtcp(this.Target, new CtcpCommand("ERRMSG", "DCC", "XMIT", "declined"), true);
		}

		private void DeletePortForwarding()
		{
			if (_isPortForwarding)
			{
				NatHelper.BeginDeleteForwardingRule(_port, System.Net.Sockets.ProtocolType.Tcp, (ar) => NatHelper.EndDeleteForwardingRule(ar));
				_isPortForwarding = false;
			}
		}

		private void dcc_Connected(object sender, EventArgs e)
		{
			this.StatusText = "Transferring";
			int iteration = SpeedUpdateInterval;
			var stats = new Queue<Tuple<long, long>>(SpeedWindow + 1);
			stats.Enqueue(new Tuple<long, long>(DateTime.UtcNow.Ticks, 0));
			_pollTimer = new Timer((o) =>
				{
					this.Dispatcher.BeginInvoke((Action)(() =>
						{
							this.BytesTransferred = _dcc.BytesTransferred;
							if (--iteration == 0)
							{
								iteration = SpeedUpdateInterval;
								var now = DateTime.UtcNow.Ticks;
								stats.Enqueue(new Tuple<long, long>(now, this.BytesTransferred));
								while (stats.Count > SpeedWindow)
								{
									stats.Dequeue();
								}
								if (stats.Count > 1)
								{
									var timeDiff = (now - stats.Peek().Item1) / 10000;
									var newBytes = this.BytesTransferred - stats.Peek().Item2;
									if (timeDiff / 1000 > 0)
									{
										this.Speed = newBytes / (timeDiff / 1000);
									}
									if (this.Speed > 0)
									{
										this.EstimatedTime = (int)(this.FileSize / this.Speed);
									}
									if (this.FileSize > 0)
									{
										this.Progress = (double)this.BytesTransferred / (double)this.FileSize;
									}
								}
							}
						}));
				}, null, PollTime, PollTime);
		}

		private void dcc_Disconnected(object sender, EventArgs e)
		{
			_pollTimer.Dispose();
			this.BytesTransferred = _dcc.BytesTransferred;
			this.Speed = 0;
			this.EstimatedTime = 0;
			this.Progress = 1f;

			if (this.BytesTransferred < this.FileSize)
			{
				if (this.Status != FileStatus.Cancelled)
				{
					this.Status = FileStatus.Cancelled;
					this.StatusText = "Connection lost";
				}
			}
			else
			{
				this.Status = (_dcc is DccXmitReceiver || _dcc is DccSendReceiver) ? FileStatus.Received : FileStatus.Sent;
				this.StatusText = "Finished";
			}
			this.DeletePortForwarding();
		}

		private void dcc_Error(object sender, Floe.Net.ErrorEventArgs e)
		{
			this.Status = FileStatus.Cancelled;
			this.StatusText = "Error: " + e.Exception.Message;
			if (_pollTimer != null)
			{
				_pollTimer.Dispose();
			}
			this.DeletePortForwarding();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Status = FileStatus.Cancelled;
			this.StatusText = "Cancelled";
			if (_dcc != null)
			{
				_dcc.Dispose();
			}
		}

		private void btnOpen_Click(object sender, RoutedEventArgs e)
		{
			string path = "";
			if (_dcc is DccXmitReceiver)
			{
				path = ((DccXmitReceiver)_dcc).FileSavedAs;
			}
			else
			{
				path = ((DccSendReceiver)_dcc).FileSavedAs;
			}
			if (File.Exists(path))
			{
				App.BrowseTo(path);
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
			this.Accept(false);
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
				this.Accept(true);
			}
		}

		private void btnDecline_Click(object sender, RoutedEventArgs e)
		{
			this.Decline();
		}
	}
}
