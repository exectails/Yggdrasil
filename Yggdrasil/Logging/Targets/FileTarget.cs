// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;

namespace Yggdrasil.Logging.Targets
{
	public class FileTarget : LoggerTarget
	{
		public string FolderPath { get; private set; }
		public string FilePath { get; private set; }

		public FileTarget(string folderPath = "")
		{
			this.FolderPath = folderPath;
		}

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

		public override string GetFormat(LogLevel level)
		{
			return "[{0}] - {1}";
		}
	}
}
