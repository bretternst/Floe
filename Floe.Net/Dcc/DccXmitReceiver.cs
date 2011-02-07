using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Floe.Net
{
	/// <summary>
	/// A class responsible for receiving files from another source using the DCC XMIT protocol.
	/// </summary>
	public sealed class DccXmitReceiver : DccOperation
	{
		private bool _isTransferring = false;
		private FileInfo _fileInfo;
		private FileStream _fileStream;
		private byte[] _timeStampBytes = new Byte[4];
		private int _timeStamp;
		private int _handshakeBytesReceived;

		/// <summary>
		/// Gets or sets a value indicating whether the specified file will be overwritten if it already exists and a resume is not possible. If this is set to false,
		/// the file will be renamed.
		/// </summary>
		public bool ForceOverwrite { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether a resume will be forced even if the transferred file does not appear to be the same as the existing file.
		/// If there is no existing file to resume, this property does nothing.
		/// </summary>
		public bool ForceResume { get; set; }

		/// <summary>
		/// Gets the path to the file that was created. This may differ from the initial file path if it has been automatically renamed to avoid overwriting a file.
		/// </summary>
		public string FileSavedAs { get; private set; }

		/// <summary>
		/// Construct a new DccXmitReceiver.
		/// </summary>
		/// <param name="fileInfo">A reference to the file to save. If the file exists, a resume will be attempted. If the resume fails, the file will be renamed.</param>
		/// <param name="callback">An optional callback used to route events to another thread.</param>
		public DccXmitReceiver(FileInfo fileInfo, Action<Action> callback = null)
			: base(callback)
		{
			_fileInfo = fileInfo;
			this.FileSavedAs = fileInfo.FullName;
		}

		protected override void OnReceived(byte[] buffer, int count)
		{
			base.OnReceived(buffer, count);

			if (!_isTransferring)
			{
				Array.Copy(buffer, 0, _timeStampBytes, _handshakeBytesReceived, Math.Min(4, count));
				_handshakeBytesReceived += count;

				byte[] resumeBytes = BitConverter.GetBytes((int)0);
				if (_handshakeBytesReceived >= 4)
				{
					_timeStamp = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(_timeStampBytes, 0));
					if (_fileInfo.Exists && _fileInfo.Length < int.MaxValue &&
						(this.ForceResume || (_timeStamp > 0 && _timeStamp == (int)(_fileInfo.LastWriteTimeUtc - new DateTime(1970, 1, 1)).TotalSeconds)))
					{
						_fileStream = new FileStream(_fileInfo.FullName, FileMode.Append, FileAccess.Write, FileShare.Read);
						resumeBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)_fileInfo.Length));
						this.BytesTransferred = _fileInfo.Length;
					}
					else
					{
						int i = 1;
						string fileName = Path.GetFileNameWithoutExtension(_fileInfo.Name);
						while (_fileInfo.Exists && !this.ForceOverwrite)
						{
							_fileInfo = new FileInfo(
								Path.Combine(_fileInfo.DirectoryName,
								string.Format("{0} ({1}).{2}", fileName, i++, _fileInfo.Extension)));
						}
						this.FileSavedAs = _fileInfo.FullName;
						_fileStream = new FileStream(_fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.Read);
					}
					this.Write(resumeBytes, 0, 4);
					_isTransferring = true;
				}
			}
			else
			{
				_fileStream.Write(buffer, 0, count);
				this.BytesTransferred += count;
			}
		}

		private void CloseFile()
		{
			if (_fileStream != null)
			{
				_fileStream.Dispose();
				if (_timeStamp > 0 && File.Exists(_fileInfo.FullName))
				{
					File.SetLastWriteTimeUtc(_fileInfo.FullName, new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(_timeStamp));
				}
			}
		}

		protected override void OnDisconnected()
		{
			base.OnDisconnected();
			this.CloseFile();
		}

		protected override void OnError(Exception ex)
		{
			base.OnError(ex);
			this.CloseFile();
		}
	}
}
