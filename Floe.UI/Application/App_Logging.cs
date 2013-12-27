using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace Floe.UI
{
	public partial class App : Application
	{
		private const string LogsFolder = "Logs";

		public static string LoggingPathBase
		{
			get
			{
				return Path.Combine(App.Settings.BasePath, LogsFolder);
			}
		}

		public static LogFileHandle OpenLogFile(string name)
		{
			return new LogFileHandle(LoggingPathBase, string.Format("{0}.log", name),
				App.Settings.Current.Buffer.BufferLines);
		}

		public static void LogUnhandledException(object exceptionObject)
		{
			string path = Path.Combine(LoggingPathBase, "exception.txt");
			using (var sw = new StreamWriter(path))
			{
				sw.WriteLine(string.Format("-{2}{0}{2}{1}{2}", DateTime.Now.ToString(), exceptionObject.ToString(), Environment.NewLine));
				sw.Flush();
			}
		}
	}

	public class LogFileHandle : IDisposable
	{
		private FileStream _logFile;

		public Queue<ChatLine> Buffer { get; private set; }

		public LogFileHandle(string folderPath, string fileName, int linesToRead)
		{
			this.Buffer = new Queue<ChatLine>();

			if (!App.Settings.Current.Buffer.IsLoggingEnabled)
			{
				_logFile = null;
				return;
			}

			try
			{
				if (!Directory.Exists(folderPath))
				{
					Directory.CreateDirectory(folderPath);
				}
				string filePath = Path.Combine(folderPath, fileName);
				if (File.Exists(filePath))
				{
					this.FillBuffer(filePath, linesToRead);
				}
				_logFile = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Error opening log file: " + ex.Message);
				_logFile = null;
			}
		}

		public void FillBuffer(string filePath, int linesToRead)
		{
			using (var reader = new StreamReader(filePath))
			{
				reader.BaseStream.Seek(Math.Max(0, reader.BaseStream.Length - (512 * (linesToRead + 1))), SeekOrigin.Begin);
				reader.DiscardBufferedData();

				var rawLines = new List<string>();
				while (!reader.EndOfStream)
				{
					rawLines.Add(reader.ReadLine());
				}
				for (int i = Math.Max(0, rawLines.Count - linesToRead); i < rawLines.Count; i++)
				{
					var cl = this.Parse(rawLines[i]);
					if (cl != null)
					{
						this.Buffer.Enqueue(cl);
					}
				}
			}
		}

		public void WriteLine(ChatLine line)
		{
			if (_logFile != null)
			{
				var s = string.Format("{0}\t{1}\t{2}\t{3}\t{4}{5}",
					line.ColorKey, line.Time.ToBinary(), line.NickHashCode, line.Nick ?? "*", line.RawText, Environment.NewLine);
				if (s.Length > 512)
				{
					s = s.Substring(0, 512);
				}
				byte[] buf = Encoding.UTF8.GetBytes(s);
				try
				{
					_logFile.Write(buf, 0, buf.Length);
					_logFile.Flush();
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine("Error writing to log file: " + ex.Message);
					_logFile = null;
				}
			}
		}

		public void Dispose()
		{
			if (_logFile != null)
			{
				_logFile.Dispose();
			}
		}

		private ChatLine Parse(string s)
		{
			string[] parts = s.Split('\t');
			if (parts.Length != 5)
			{
				return null;
			}

			long dt;
			if (!long.TryParse(parts[1], out dt))
			{
				return null;
			}

			int hashCode;
			if (!int.TryParse(parts[2], out hashCode))
			{
				return null;
			}

			var time = DateTime.FromBinary(dt);
			return new ChatLine(parts[0], time, hashCode, parts[3] == "*" ? null : parts[3], parts[4], ChatMarker.None);
		}
	}
}
