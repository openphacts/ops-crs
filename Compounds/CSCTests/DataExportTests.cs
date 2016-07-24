using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSC.Compounds.DataExport;
using VDS.RDF;

namespace RSC.Compounds.Tests
{
	//[TestClass]
	public class DataExportTests
	{
		/// <summary>
		/// The files we are exporting and the expected record counts for each.
		/// </summary>
		private readonly Dictionary<string, int> _dataExportFileRecordCounts
			= new Dictionary<string, int>
            {
                //{2} = YYYY {1} = MM {0} = DD
                {"LINKSET_CLOSE_PARENT_CHILD_TAUTOMER_INSENSITIVE_PARENT_THOMSON_PHARMA{2}{1}{0}.ttl", 1520},
                {"LINKSET_CLOSE_PARENT_CHILD_STEREO_INSENSITIVE_PARENT_THOMSON_PHARMA{2}{1}{0}.ttl", 864},
                {"LINKSET_EXACT_OPS_CHEMSPIDER_THOMSON_PHARMA{2}{1}{0}.ttl", 1413},
                {"SYNONYMS_THOMSON_PHARMA{2}{1}{0}.ttl", 24063},
                {"LINKSET_CLOSE_PARENT_CHILD_CHARGE_INSENSITIVE_PARENT_THOMSON_PHARMA{2}{1}{0}.ttl", 66},
                {"PROPERTIES_THOMSON_PHARMA{2}{1}{0}.ttl", 176551},
                {"LINKSET_CLOSE_PARENT_CHILD_SUPER_INSENSITIVE_PARENT_THOMSON_PHARMA{2}{1}{0}.ttl", 111},
                {"ISSUES_THOMSON_PHARMA{2}{1}{0}.ttl", 1156},
                {"LINKSET_CLOSE_PARENT_CHILD_ISOTOPE_INSENSITIVE_PARENT_THOMSON_PHARMA{2}{1}{0}.ttl", 9},
                {"LINKSET_RELATED_PARENT_CHILD_FRAGMENT_THOMSON_PHARMA{2}{1}{0}.ttl", 520},
                {"LINKSET_EXACT_THOMSON_PHARMA{2}{1}{0}.ttl", 1901}
            };

		/// <summary>
		/// Details of the generated void file.
		/// </summary>
		private readonly KeyValuePair<string, int> _dataExportVoidFileRecordCount = new KeyValuePair<string, int>("void_{2}-{1}-{0}.ttl", 0);

		/// <summary>
		/// Details of the generated SDF file.
		/// </summary>
		private readonly KeyValuePair<string, int> _dataExportSdfFileRecordCount = new KeyValuePair<string, int>("OPS_CHEMSPIDER_{2}-{1}-{0}.sdf", 3449);

		[TestMethod]
		public void TestThomsonPharmaDataExport()
		{
			//This test operates with the Thomson Pharma dataset which contains 1901 records.
			var dataSourceIds = new List<Guid>() { Guid.Parse("77DDC358-14F5-4800-A316-30D204AC0345") };

			//Get the Year, Month, Day as strings.
			var dateTime = DateTime.Now;
			var sYear = dateTime.ToString("yyyy");
			var sMonth = dateTime.ToString("MM");
			var sDay = dateTime.ToString("dd");

			//Perform the data export.
			var opsDataExport = new OpsDataExport();
			opsDataExport.Export(new OpsDataExportOptions() {
				DataSourceIds = dataSourceIds
			}
			);

			//We need access to the Data Export store to retrieve the logs.
			var dataExportStore = new EFDataExportStore();

			//Check the results for each Data Source.
			foreach ( var dataSourceId in dataSourceIds ) {
				/* State is private
				 * foreach (var dataExportId in opsDataExport.State.DataExportIds.Where(d => d.Key == dataSourceId))
				{
					//Retrieve the log.
					var dataExportLog = dataExportStore.GetDataExportLog(dataExportId.Value);

					//Data export did not succeed - fail the test.
					Assert.IsFalse(dataExportLog.HasFailed,
						string.Format("Data Export failed. Data Source Id:{0} Error message:{1}"
						, dataExportLog.DataSourceId
						, dataExportLog.ErrorMessage));

					//Check record count for each of the expected files.
					foreach (var expectedFile in _dataExportFileRecordCounts)
					{
						var exportedFile = dataExportLog.Files.FirstOrDefault(f => f.FileName == string.Format(expectedFile.Key, sDay, sMonth, sYear));

						//Missing file - fail the test.
						Assert.IsFalse(exportedFile == null, string.Format("Expected file {0} was not exported for Data Source Id:{1}.", expectedFile.Key, dataExportLog.DataSourceId));

						//Record count does not match the expected count - fail the test.
						Assert.IsTrue(expectedFile.Value == exportedFile.RecordCount, string.Format("Expected record count incorrect for file:{0} Expected count:{1}, Actual Count:{2}", exportedFile.FileName, expectedFile.Value, exportedFile.RecordCount));
					}
				}*/
			}

			//Check the Sdf file generation.

			//Retrieve the log.
			DataExportLog sdfDataExportLog = null;

			// This has to be re-done using private methods accessors
			// DataExportLog sdfDataExportLog = dataExportStore.GetDataExportLog(opsDataExport.SdfDataExportId);

			//If there is no log - fail the test.
			Assert.IsFalse(sdfDataExportLog == null, string.Format("Expected sdf file was not exported."));

			//If the data export failed in the log - fail the test.
			Assert.IsFalse(sdfDataExportLog.Status == DataExportStatus.ExportFailed, string.Format("Expected sdf file failed to export. Error Message:{0}", sdfDataExportLog.ErrorMessage));

			//Check for the expected file.
			var expectedSdfFile = string.Format(_dataExportSdfFileRecordCount.Key, sDay, sMonth, sYear);
			var exportedSdfFile = sdfDataExportLog.Files.FirstOrDefault(f => f.FileName == expectedSdfFile);

			//Missing file - fail the test.
			Assert.IsFalse(exportedSdfFile == null, string.Format("Expected file {0} was not exported.", expectedSdfFile));

			//Get the physical file path for the Sdf export.
			// TODO: compose full path using exports relative dir
			var sdfFilePath = exportedSdfFile.FileName;

			//Load the exported void file into a string.
			string sdfFile;
			using ( var sr = new StreamReader(sdfFilePath + ".gz") ) {
				sdfFile = sr.ReadToEnd();
			}

			//If the file is empty or does not exist - fail the test.
			Assert.IsFalse(string.IsNullOrEmpty(sdfFile), string.Format("Generated sdf file {0} is empty or does not exist.", sdfFilePath));

			//Check the void file generation.

			//Retrieve the log.
			DataExportLog voidDataExportLog = null;

			// This has to be re-done using private methods accessors
			// voidDataExportLog = dataExportStore.GetDataExportLog(opsDataExport.VoidDataExportId);

			//If there is no log - fail the test.
			Assert.IsFalse(voidDataExportLog == null, string.Format("Expected void file was not exported."));

			//If the data export failed in the log - fail the test.
			Assert.IsFalse(voidDataExportLog.Status == DataExportStatus.ExportFailed, string.Format("Expected void file failed to export. Error Message:{0}", voidDataExportLog.ErrorMessage));

			//Get the physical file path for the void export.
			// TODO: compose full path using exports relative dir
			var voidFilePath = voidDataExportLog.Files.Single().FileName;

			//Load the exported void file into a string.
			string voidFile;
			using ( var sr = new StreamReader(voidFilePath) ) {
				voidFile = sr.ReadToEnd();
			}

			//If the file is empty or does not exist - fail the test.
			Assert.IsFalse(string.IsNullOrEmpty(voidFile), string.Format("Generated void file {0} is empty or does not exist.", voidFilePath));

			//Load the file into Turtle parser - it's highly unlikely to fail as it was generated by the same component, but why not.
			var voidGraph = new Graph();
			try {
				voidGraph.LoadFromString(voidFile);
			}
			catch ( Exception ex ) {
				Assert.Fail(string.Format("Failed to load generated void into a graph. Error:{0}", ex.Message));
			}

			//TODO: It would be nice to validate against this OpenPhacts Validator. However there seems to be no method of submitting to this validator (other than via the web page). See link: http://openphacts.cs.man.ac.uk/validata/

			//TODO: Check various parts of the void file.
			//voidGraph.Triples.Where(t => t.)
		}
	}
}
