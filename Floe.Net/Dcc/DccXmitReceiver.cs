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
		/// Construct a new DccXmitReceiver.
		/// </summary>
		/// <param name="fileInfo">A reference to the file to save. If the file exists, a resume will be attempted. If the resume fails, the file will be renamed.</param>
		/// <param name="callback">An optional callback used to route events to another thread.</param>
		public DccXmitReceiver(FileInfo fileInfo, Action<Action> callback = null)
			: base(callback)
		{
			_fileInfo = fileInfo;
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
					if (_timeStamp > 0 && _fileInfo.Exists && _fileInfo.Length < int.MaxValue && _timeStamp == (_fileInfo.LastWriteTimeUtc - new DateTime(1970, 1, 1)).TotalSeconds)
					{
						_fileStream = new FileStream(_fileInfo.FullName, FileMode.Append, FileAccess.Write, FileShare.Read);
						resumeBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)_fileInfo.Length));
					}
					else
					{
						int i = 1;
						string fileName = Path.GetFileNameWithoutExtension(_fileInfo.Name);
						while (_fileInfo.Exists)
						{
							_fileInfo = new FileInfo(
								Path.Combine(_fileInfo.DirectoryName,
								string.Format("{0} ({1}).{2}", fileName, i++, _fileInfo.Extension)));
						}
						_fileStream = new FileStream(_fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.Read);
					}
					this.Stream.Write(resumeBytes, 0, 4);
					_isTransferring = true;
				}
			}
			else
			{
				_fileStream.Write(buffer, 0, count);
				this.BytesTransferred += count;
			}
		}

		protected override void OnDisconnected()
		{
			base.OnDisconnected();

			if (_fileStream != null)
			{
				_fileStream.Dispose();
				if (_timeStamp > 0)
				{
					File.SetLastWriteTimeUtc(_fileInfo.FullName, new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(_timeStamp));
				}
			}
		}

		protected override void OnError(Exception ex)
		{
			base.OnError(ex);

			if (_fileStream != null)
			{
				_fileStream.Dispose();
			}
		}
	}
}
