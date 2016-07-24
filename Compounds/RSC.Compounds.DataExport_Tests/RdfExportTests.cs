using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSC.Compounds.DataExport;
using RSC.Compounds.EntityFramework;
using RSC.Logging.EntityFramework;
using RSC.Properties.EntityFramework;

namespace RSC.Compounds.DataExport_Tests
{
	[TestClass]
	public class RdfExportTests
	{
		private static string getFileUriFromRelPath(string relPath)
		{
			return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), relPath);
		}

		/// ///////////////////////////////////////////////////////////////////////////////
		
		[TestMethod]
		public void ExportDatasources()
		{
			var export = new OpsDataExport();
			var options = new OpsDataExportOptions {
				DataSourceIds = new List<Guid> { Guid.Parse("3384B886-67EA-4793-8240-5AF5D033D5FC") },
				UploadUrl = getFileUriFromRelPath("out"),
				DownloadUrl = "http://ops.rsc.org/download/",
			};

			export.ExportDatasources(options);
		}

		[TestMethod]
		public void ExportDatasources_ExportId()
		{
			var export = new OpsDataExport();
			var options = new OpsDataExportOptions {
				ExportIds = new List<int>() { 37 },
				UploadUrl = getFileUriFromRelPath("out"),
				DownloadUrl = "http://ops.rsc.org/download/",
			};

			export.ExportDatasourcesByExportIds(options);
		}

		[TestMethod]
		public void ExportOpsToCsidSdf()
		{
			var export = new OpsDataExport();
			var options = new OpsDataExportOptions {
				UploadUrl = getFileUriFromRelPath("out"),
			};

			export.ExportOpsToCsidSdf(options);
			Assert.Fail();
		}

		[TestMethod]
		public void ExportVoID()
		{
			var export = new OpsDataExport();
			var options = new OpsDataExportOptions {
				DataSourceIds = new List<Guid> { Guid.Parse("3384B886-67EA-4793-8240-5AF5D033D5FC") },
				ExportIds = new List<int>() { 23 },
				Override = true,
				UploadUrl = getFileUriFromRelPath("out"),
				DownloadUrl = "http://ops.rsc.org/download/",
			};

			export.ExportVoID(options);
			Assert.Fail();
		}

		[TestMethod]
		public void Export()
		{
			var export = new OpsDataExport();
			var options = new OpsDataExportOptions {
				DataSourceIds = new List<Guid> { Guid.Parse("3384B886-67EA-4793-8240-5AF5D033D5FC") },
				Override = true,
				UploadUrl = getFileUriFromRelPath("out"),
				DownloadUrl = "http://ops.rsc.org/download/",
			};

			export.Export(options);
			Assert.Fail();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		[TestMethod]
		public void RdfCRSDataSourceExport_ExactLinksetDataExportFile()
		{
			var export = new OpsDataSourceExport(Guid.Parse("3384B886-67EA-4793-8240-5AF5D033D5FC"), DateTime.Now) {
				DataExportStore = new EFDataExportStore(),
				PropertiesStore = new EFPropertyStore(),
				LogStore = new EFLogStore(null),
				UploadUrl = getFileUriFromRelPath("out"),
				DownloadUrl = "http://ops.rsc.org/download/",
				Encoding = new UTF8Encoding(false)
			};
			export.CompoundsStore = new EFCompoundStore(export.PropertiesStore);
			new ExactLinksetDataExportFile(export).Export(export, export.Encoding);
		}
	}
}
