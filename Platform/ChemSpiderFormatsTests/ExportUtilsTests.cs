using System;
using System.IO;
using ChemSpider.Formats;
using ChemSpider.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChemSpiderFormatsTests
{
	[TestClass]
	public class ExportUtilsTests
	{
		[TestMethod]
		public void DumpSdfFileTest()
		{
			using ( TempFile tmp = new TempFile() ) {
				bool some_records_exported = ExportUtils.DumpSdfFile(tmp.FullPath, 1, 100, true);
				Assert.IsTrue(some_records_exported);
				Assert.IsTrue(new FileInfo(tmp.FullPath).Length > 1000);
			}
		}
	}
}
