using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ChemSpider.Utilities;

namespace RSC.Compounds.DataExport
{
	public class CopyFileUploader : IFileUploader
	{
		public bool IsBusy
		{
			get { return false; }
		}

		public bool IsDisposed
		{
			get { return false; }
		}

		public int JobsRemaining
		{
			get { return 0; }
		}

		public bool SkipIfExists { get; set; }

		public bool CancelAsyncUpload(string remotePath)
		{
			return true;
		}

		public void DeleteDirectory(string targetPath, bool requireExists = false)
		{
			if ( Directory.Exists(targetPath) )
				Directory.Delete(targetPath);
		}

		public bool DeleteDirectoryAsync(string targetPath)
		{
			if ( Directory.Exists(targetPath) )
				Directory.Delete(targetPath);

			return true;
		}

		public bool TestConnection(object userState, out string error)
		{
			error = null;
			return true;
		}

		private static string UriToLocal(string uri_path)
		{
			if ( uri_path.StartsWith("file:", StringComparison.CurrentCultureIgnoreCase) ) {
				var uri = new Uri(uri_path);
				uri_path = uri.LocalPath;
			}
			return uri_path;
		}

		public bool UploadAsync(string sourcePath, string remotePath)
		{
			// TODO: Sync for now - change to async
			sourcePath = UriToLocal(sourcePath);
			remotePath = UriToLocal(remotePath);

			if ( File.Exists(remotePath) )
				File.Delete(remotePath);

			File.Copy(sourcePath, remotePath);

			return true;
		}

		public void UploadAsync(IEnumerable<KeyValuePair<string, string>> files)
		{
			files.ForAll(kv => UploadAsync(kv.Key, kv.Value));
		}

		public void Dispose()
		{
			
		}

		public void CreateDirectoryIfNotExists(string dir)
		{
			dir = UriToLocal(dir);
			if ( !Directory.Exists(dir) )
				Directory.CreateDirectory(dir);
		}
	}
}
