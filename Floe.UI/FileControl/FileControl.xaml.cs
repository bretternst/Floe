using System;
using System.IO;
using System.Net;
using System.Windows;
using Floe.Net;

namespace Floe.UI
{
	public enum FileStatus
	{
		Working,
		Cancelled,
		Finished
	}

	public partial class FileControl : ChatPage
	{
		private FileInfo _fileInfo;

		public static readonly DependencyProperty DescriptionProperty =
			DependencyProperty.Register("Description", typeof(long), typeof(FileControl));
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
			DependencyProperty.Register("Status", typeof(long), typeof(FileControl));
		public FileStatus Status
		{
			get { return (FileStatus)this.GetValue(StatusProperty); }
			set { this.SetValue(StatusProperty, value); }
		}

		public static readonly DependencyProperty StatusTextProperty =
			DependencyProperty.Register("StatusText", typeof(long), typeof(FileControl));
		public string StatusText
		{
			get { return (string)this.GetValue(StatusTextProperty); }
			set { this.SetValue(StatusTextProperty, value); }
		}

		public FileControl(IrcSession session, IrcTarget target)
			: base(ChatPageType.DccFile, session, null, "DCC")
		{
			InitializeComponent();
			this.Status = FileStatus.Working;
			this.Id = "dcc-file";
			this.Header = this.Title = string.Format("DCC [{0}]", target.Name);
		}

		public void StartSend(IPAddress address, FileInfo file)
		{
			_fileInfo = file;
			this.Description = string.Format("Sending {0}...", file.Name);
			this.FileSize = file.Length;
			this.StatusText = "Listening for connection";
		}

		public void StartReceive(IPAddress address, int port, string name, long size)
		{
			_fileInfo = new FileInfo(Path.Combine(App.Settings.Current.Dcc.DownloadFolder, name));
			int i = 1;
			while (_fileInfo.Exists)
			{
				_fileInfo = new FileInfo(Path.Combine(App.Settings.Current.Dcc.DownloadFolder, string.Format("{0} ({1}).{2}", Path.GetFileNameWithoutExtension(name), i++, _fileInfo.Extension)));
			}
			this.Description = string.Format("Receiving {0}...", name);
			this.FileSize = size;
			this.StatusText = "Connecting";
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Status = FileStatus.Cancelled;
			this.StatusText = "Cancelled";
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
	}
}
