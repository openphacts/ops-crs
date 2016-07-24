using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSC.Compounds.DataExport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
namespace RSC.Compounds.DataExport.Tests
{
	[TestClass()]
	public class CopyFileUploaderTests
	{
		[TestMethod()]
		public void DeleteDirectoryTest()
		{
			string dir_name = "test_dir1";
			Directory.CreateDirectory(dir_name);

			CopyFileUploader a = new CopyFileUploader();
			a.DeleteDirectoryAsync(dir_name);
			Assert.IsTrue(!Directory.Exists(dir_name));
		}

		[TestMethod()]
		public void DeleteDirectoryAsyncTest()
		{
			string dir_name = "test_dir1";
			Directory.CreateDirectory(dir_name);

			CopyFileUploader a = new CopyFileUploader();
			a.DeleteDirectoryAsync(dir_name);
			Assert.IsTrue(!Directory.Exists(dir_name));
		}

		[TestMethod()]
		public void UploadAsyncTest()
		{
			string src_name = @"src\file.txt";
			Directory.CreateDirectory(Path.GetDirectoryName(src_name));
			File.WriteAllText(src_name, "Some thing");

			string dst_name = @"dst\file.txt";
			CopyFileUploader a = new CopyFileUploader();
			a.UploadAsync(src_name, dst_name);
			Assert.IsTrue(File.Exists(dst_name));
		}

		[TestMethod()]
		public void CreateDirectoryIfNotExistsTest()
		{
			string dir_name = "test_dir";
			CopyFileUploader a = new CopyFileUploader();
			a.CreateDirectoryIfNotExists(dir_name);
			Assert.IsTrue(Directory.Exists(dir_name));
		}
	}
}
