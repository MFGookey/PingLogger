using System;
using System.IO;

namespace PingLogger
{
	public class FileLogger
	{
		private readonly string _filePath;

		public FileLogger (string fileDirectoryPath, bool createFile)
		{
			Directory.CreateDirectory(fileDirectoryPath);
			var fileName = string.Format(@"{0}\{1}.txt", fileDirectoryPath, DateTime.Now.ToString ().Replace ('/', '-').Replace(':', '.'));
			if (createFile) {
				using (var stream = File.Create (fileName)) {};
			}

			_filePath = fileName;

			this.LogFormat ("Logging beginning {0}", DateTime.Now);

		}

		public void Log(string data){
			using (var writer = File.AppendText (_filePath)) {
				writer.WriteLine (data);
			}
		}

		public void LogFormat(string template, params object[] args){
			this.Log(string.Format(template, args));
		}
	}
}

