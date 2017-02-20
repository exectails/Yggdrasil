// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;

namespace Yggdrasil.Logging.Targets
{
	/// <summary>
	/// Logger target logging to a text file.
	/// </summary>
	public class FileTarget : LoggerTarget
	{
		/// <summary>
		/// The folder the log file is in.
		/// </summary>
		public string FolderPath { get; private set; }

		/// <summary>
		/// The path to the log file.
		/// </summary>
		public string FilePath { get; private set; }

		/// <summary>
		/// Creates new instance, with the file going into the given folder.
		/// </summary>
		/// <param name="folderPath"></param>
		public FileTarget(string folderPath = "")
		{
			this.FolderPath = folderPath;
		}

		/// <summary>
		/// Writes clean message to the log file, prepending it with the
		/// time and date the message was written at.
		/// </summary>
		/// <param name="level"></param>
		/// <param name="message"></param>
		/// <param name="messageRaw"></param>
		/// <param name="messageClean"></param>
		public override void Write(LogLevel level, string message, string messageRaw, string messageClean)
		{
			if (this.FilePath == null)
			{
				this.FilePath = Path.Combine(this.FolderPath, this.Logger.Name + ".txt");

				if (File.Exists(this.FilePath))
					File.Delete(this.FilePath);

				if (!Directory.Exists(this.FolderPath))
					Directory.CreateDirectory(this.FolderPath);
			}

			messageClean = string.Format("{0:yyyy-MM-dd HH:mm} {1}", DateTime.Now, messageClean);

			File.AppendAllText(this.FilePath, messageClean);
		}

		/// <summary>
		/// Returns the format for the raw log message.
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		public override string GetFormat(LogLevel level)
		{
			return "[{0}] - {1}";
		}
	}
}
