using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ChemSpider.Formats.Accelrys;
using ChemSpider.Molecules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChemSpiderFormatsTests
{
	[TestClass]
	public class SdfTests
	{
		[TestMethod]
		[DeploymentItem(@"..\..\Resources\1.sdf")]
		public void Sdf_Test1()
		{
			using (SdfReader reader = new SdfReader(new StreamReader("1.sdf")))
			{
				reader.FieldsMap = new Dictionary<string, string> { { "SYNONYMS2", "Synonyms" } };
				reader.Splitters = new Dictionary<string, Func<string, IEnumerable<string>>> { { "SYNONYMS", SdfReader.DefaultSplitter } };

				List<ChemSpider.Molecules.SdfRecord> records = reader.Records.ToList();
				
				Assert.AreEqual(3, records.Count, "Incorrect number of records");
				Assert.AreEqual(3, records[1]["SYNONYMS"].Count(), "Cannot parse multiline values");
				Assert.IsTrue(records[1]["SYNONYMS"].ToArray()[2] == "syn_3", "Cannot read multiline values");

				Assert.AreEqual(3, records[2]["Synonyms"].Count(), "Case sensitive");
			}
		}

		[TestMethod]
		[DeploymentItem(@"..\..\Resources\2.sdf")]
		public void Sdf_XmlField_Test()
		{
			using ( SdfReader reader = new SdfReader(new StreamReader("2.sdf")) )
			{
				List<SdfRecord> records = reader.Records.ToList();
				Assert.AreEqual(records.Count, 1);

				string desc = records[0].GetConcatFields("DESC_PART").Trim();
				Assert.IsTrue(!String.IsNullOrWhiteSpace(desc));
				Assert.IsTrue(desc.EndsWith(">"), "Malformed XML - no trailing '>'");

				XDocument xdoc = XDocument.Parse(desc);
			}
		}

		[TestMethod]
		[DeploymentItem(@"..\..\Resources\3.sdf")]
		public void Sdf_XmlField2_Test()
		{
			using ( SdfReader reader = new SdfReader(new StreamReader("3.sdf")) ) {
				List<SdfRecord> records = reader.Records.ToList();
				Assert.AreEqual(records.Count, 1);

				string desc = records[0].GetConcatFields("DESC_PART").Trim();
				Assert.IsTrue(!String.IsNullOrWhiteSpace(desc));
				Assert.IsTrue(desc.EndsWith(">"), "Malformed XML - no trailing '>'");

				XDocument xdoc = XDocument.Parse(desc);
				Assert.IsNotNull(xdoc);

				Assert.AreEqual("FDA_ID: F002678", records[0]["COMMENTS"].First());
			}
		}

		[TestMethod]
		[DeploymentItem(@"..\..\Resources\ccdc.sdf")]
		public void Sdf_CCDC_Test()
		{
			using ( SdfReader reader = new SdfReader(new StreamReader("ccdc.sdf")) ) {
				IEnumerable<SdfRecord> records = reader.Records.ToList();
				Assert.AreEqual(141, records.Count());
			}
		}

		[TestMethod]
		[DeploymentItem(@"..\..\Resources\drugbank.sdf")]
		public void Sdf_DrugBank_Test()
		{
			using ( SdfReader reader = new SdfReader(new StreamReader("drugbank.sdf")) ) {
				IEnumerable<SdfRecord> records = reader.Records.ToList();
				Assert.AreEqual(62, records.Count());
			}
		}

		[TestMethod]
		[DeploymentItem(@"..\..\Resources\hmdb.sdf")]
		public void Sdf_HMDB_Test()
		{
			using ( SdfReader reader = new SdfReader(new StreamReader("hmdb.sdf")) )
			{
				reader.Splitters = new Dictionary<string, Func<string, IEnumerable<string>>> { { "SYNONYMS", s => s.Split(';').Select(t => t.Trim()) } };

				IEnumerable<SdfRecord> records = reader.Records.ToList();
				Assert.AreEqual(records.Count(), 90);

				Assert.AreEqual(9, records.First()["SYNONYMS"].Count());
			}
		}

		[TestMethod]
		[DeploymentItem(@"..\..\Resources\ccdc+hmdb.sdf")]
		public void Sdf_MixedEOL_Test()
		{
			using ( SdfReader reader = new SdfReader(new StreamReader("ccdc+hmdb.sdf")) ) {
				IEnumerable<SdfRecord> records = reader.Records.ToList();
				Assert.AreEqual(231, records.Count());
			}
		}

		[TestMethod]
		[DeploymentItem(@"..\..\Resources\non-sdf.sdf")]
		public void Sdf_NonSdf_Test()
		{
			using ( SdfReader reader = new SdfReader(new StreamReader("non-sdf.sdf")) ) {
				IEnumerable<SdfRecord> records = reader.Records.ToList();
				Assert.AreEqual(0, records.Count());
			}
		}

		[TestMethod]
		[DeploymentItem(@"..\..\Resources\EmptySDField.sdf")]
		public void Sdf_EmptySDField_Test()
		{
			using ( SdfReader reader = new SdfReader(new StreamReader("EmptySDField.sdf")) ) {
				reader.Splitters = new Dictionary<string, Func<string, IEnumerable<string>>> { { "SYNONYMS", SdfReader.DefaultSplitter } };

				List<ChemSpider.Molecules.SdfRecord> records = reader.Records.ToList();

				Assert.AreEqual(1, records.Count, "Incorrect number of records");
				Assert.AreEqual(3, records[0]["SYNONYMS"].Count(), "Cannot parse multiline values");
				Assert.IsTrue(records[0]["SYNONYMS"].ToArray()[2] == "syn_3", "Cannot read multiline values");

				Assert.AreEqual("", records[0]["Name"].First(), "Case sensitive");
			}
		}

		[TestMethod]
		[DeploymentItem(@"..\..\Resources\No$$$$.sdf")]
		public void Sdf_NoEOR_Test()
		{
			using ( SdfReader reader = new SdfReader(new StreamReader("No$$$$.sdf")) ) {
				List<ChemSpider.Molecules.SdfRecord> records = reader.Records.ToList();

				Assert.AreEqual(1, records.Count, "Incorrect number of records");
				Assert.AreEqual("cs000000045209", records[0]["NAME"].First(), "Case sensitive");
			}
		}

		[TestMethod]
		[DeploymentItem(@"..\..\Resources\NoMEnd.sdf")]
		public void Sdf_NoMEND_Test()
		{
			using ( SdfReader reader = new SdfReader(new StreamReader("NoMEnd.sdf")) ) {
				List<ChemSpider.Molecules.SdfRecord> records = reader.Records.ToList();

				Assert.AreEqual(1, records.Count, "Incorrect number of records");

				Assert.IsNotNull(records.First().Molecule);
				Assert.AreEqual("cs000000045209", records[0]["NAME"].First(), "Case sensitive");
			}
		}
	}
}
