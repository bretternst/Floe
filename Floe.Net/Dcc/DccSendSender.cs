using System;
using System.IO;
using System.Net;
using System.Linq;

namespace Floe.Net
{
	/// <summary>
	/// This class is responsible for sending files via the DCC SEND protocol. Unlike most implementations,
	/// this class does not care about receiving acknowledgements for each chunk of file, because that is
	/// worthless. Resume is not supported.
	/// </summary>
	public sealed class DccSendSender : DccOperation
	{
		private const int SendChunkSize = 4096;

		private FileInfo _fileInfo;
		private FileStream _fileStream;
		private byte[] _buffer = new byte[SendChunkSize];

		/// <summary>
		/// Construct a new DccSendSender.
		/// </summary>
		/// <param name="fileInfo">A reference to the file to send.</param>
		public DccSendSender(FileInfo fileInfo)
		{
			_fileInfo = fileInfo;
		}

		protected override void OnConnected()
		{
			base.OnConnected();

			_fileStream = new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read);
			this.WriteDataBlock();
		}

		protected override void OnSent(byte[] buffer, int offset, int count)
		{
			this.BytesTransferred += count;
			this.WriteDataBlock();
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

		private void CloseFile()
		{
			if (_fileStream != null)
			{
				_fileStream.Dispose();
			}
		}

		private void WriteDataBlock()
		{
			if (_fileStream.Position < _fileStream.Length)
			{
				int count = _fileStream.Read(_buffer, 0, SendChunkSize);
				this.QueueWrite(_buffer, 0, count);
			}
			else
			{
				this.Close();
			}
		}
	}
}
