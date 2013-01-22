using System;
using System.Windows;

namespace Floe.UI
{
	public partial class FileControl : ChatPage
	{
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
	}
}
