using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace RSC.CVSP
{
	public class FileStorage : IFileStorage
	{
		public string Root { get; private set; }

		public FileStorage()
		{
			Root = (ConfigurationManager.AppSettings["files_storage_root"] ?? "").TrimEnd('\\');

			if (string.IsNullOrEmpty(Root))
				throw new ArgumentNullException("File storage root is not specified");
		}

		public string UploadFile(Guid guid, string filename, Stream stream)
		{
			var dir = Path.Combine(Root, guid.ToString(), "Input");
			if(!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			string path = Path.Combine(dir, filename);

			using (var fileStream = File.Create(path))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(fileStream);
				fileStream.Close();
			}

			return path;
		}

		public IEnumerable<string> GetFiles(Guid guid)
		{
			DirectoryInfo di = new DirectoryInfo(Path.Combine(Root, guid.ToString(), "Input"));
			FileSystemInfo[] files = di.GetFileSystemInfos();

			return files.Select(f => f.FullName);
		}

		public Stream GetFile(Guid guid, string filename)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Clean all files uploaded to the deposition
		/// </summary>
		/// <param name="guid">Deposition's guid</param>
		public void CleanFiles(Guid guid)
		{
			var path = Path.Combine(Root, guid.ToString());
			if(Directory.Exists(path))
				Directory.Delete(path, true);
		}
	}
}
