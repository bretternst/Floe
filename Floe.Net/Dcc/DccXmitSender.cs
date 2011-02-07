using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Floe.Net
{
	/// <summary>
	/// This class is responsible for sending files via the DCC XMIT protocol.
	/// </summary>
	public sealed class DccXmitSender : DccOperation
	{
		private const int SendChunkSize = 2048;

		private bool _isTransferring = false;
		private FileInfo _fileInfo;
		private FileStream _fileStream;
		private byte[] _resumeBytes = new Byte[4];
		private int _handshakeBytesReceived;

		/// <summary>
		/// Construct a new DccXmitSender.
		/// </summary>
		/// <param name="fileInfo">A reference to the file to send.</param>
		/// <param name="callback">An optional callback used to route events to another thread.</param>
		public DccXmitSender(FileInfo fileInfo, Action<Action> callback = null)
			: base(callback)
		{
			_fileInfo = fileInfo;
		}

		protected override void OnConnected()
		{
			base.OnConnected();

			var timeStampBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)(_fileInfo.LastWriteTimeUtc - new DateTime(1970, 1, 1)).TotalSeconds));
			this.Write(timeStampBytes, 0, 4);
		}

		protected override void OnReceived(byte[] buffer, int count)
		{
			base.OnReceived(buffer, count);

			if (!_isTransferring)
			{
				Array.Copy(buffer, 0, _resumeBytes, _handshakeBytesReceived, Math.Min(4, count));
				_handshakeBytesReceived += count;

				if (_handshakeBytesReceived >= 4)
				{
					int resume = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(_resumeBytes, 0));
					_fileStream = new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read);
					if (resume > 0)
					{
						_fileStream.Seek(resume, SeekOrigin.Begin);
						this.BytesTransferred = resume;
					}
					_isTransferring = true;

					buffer = new byte[SendChunkSize];
					try
					{
						while (_fileStream.Position < _fileStream.Length)
						{
							count = _fileStream.Read(buffer, 0, SendChunkSize);
							this.BytesTransferred += count;
							if (!this.Write(buffer, 0, count))
							{
								return;
							}
						}
					}
					catch (IOException ex)
					{
						this.OnError(ex);
					}
					finally
					{
						this.Close();
					}
				}
			}
		}

		protected override void OnDisconnected()
		{
			base.OnDisconnected();

			if (_fileStream != null)
			{
				_fileStream.Dispose();
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
