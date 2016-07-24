using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSC.Compounds.DataExport;
using RSC.Datasources;
using RSC.Properties;

namespace RSC.Compounds.DataExport_Tests
{
	[TestClass]
	public class SynonymDataExport : DataExportTestBase
	{
		public static Compound compoundWithSmiles, compoundWithoutSmiles;

		[ClassInitialize]
		public static void Initialize(TestContext tc)
		{
			compoundWithSmiles = new Compound()
			{
				Smiles = new Smiles() { IndigoSmiles = "c1ccccc1" },
				StandardInChI = new InChI() { 
					// change these to real ones when the internet comes back
					Inchi = "34859485948", InChIKey = "235903859845" },
				ExternalReferences = new List<ExternalReference>()
					{
						new ExternalReference() { Type = opsReferenceType, Value = "234771" },
						new ExternalReference() { Type = csReferenceType, Value = "771234" }
					}
			};
			compoundWithoutSmiles = new Compound()
			{
				StandardInChI = new InChI() { 
					// change these to real ones when the internet comes back
					Inchi = "34859485948", InChIKey = "235903859845" },
				ExternalReferences = new List<ExternalReference>()
					{
						new ExternalReference() { Type = opsReferenceType, Value = "234771" },
						new ExternalReference() { Type = csReferenceType, Value = "771234" }
					}
			};
		}

		[TestMethod]
		public void SynonymDataExport_DBIDTests()
		{
			var dbidflags = new List<SynonymFlag>() {new SynonymFlag() { Name = "DBID" } };
			DataSource ds = new DataSource();
			var export = new OpsDataSourceExport(dsGuid, DateTime.Now);
			SynonymsDataExportFile sdef = new SynonymsDataExportFile(export);

			// validated DBID
			Assert.AreEqual("cheminf:CHEMINF_000467 \"123456-78-9\"@en",
				new Synonym() { 
					Name = "123456-78-9", 
					State = CompoundSynonymState.eApproved,
					Flags = dbidflags,
					LanguageId = "en" }.ToPredicateObject(), "wrong identifier for validated database ID");

			// unvalidated DBID
			Assert.AreEqual("cheminf:CHEMINF_000464 \"123456-78-9\"@en",
				new Synonym() { 
					Name = "123456-78-9", 
					State = CompoundSynonymState.eDeleted,
					Flags = dbidflags,
					LanguageId = "en" }.ToPredicateObject(), "wrong identifier for non-validated database ID");
		}

		[TestMethod]
		public void SynonymDataExport_SynonymTests()
		{
			DataSource ds = new DataSource();
			var export = new OpsDataSourceExport(dsGuid, DateTime.Now);
			SynonymsDataExportFile sdef = new SynonymsDataExportFile(export);
			// validated DBID
			Assert.AreEqual("cheminf:CHEMINF_000465 \"pyridine\"@en",
				new Synonym()
				{
					Flags = new List<SynonymFlag>(),
					Name = "pyridine",
					State = CompoundSynonymState.eApproved,
					LanguageId = "en"
				}.ToPredicateObject(), "wrong identifier for validated synonym");
			// unvalidated DBID
			Assert.AreEqual("cheminf:CHEMINF_000466 \"pyrrhidine\"@en",
				new Synonym()
				{
					Flags = new List<SynonymFlag>(),
					Name = "pyrrhidine",
					State = CompoundSynonymState.eDeleted,
					LanguageId = "en"
				}.ToPredicateObject(), "wrong identifier for non-validated synonym");
		}

		[TestMethod]
		public void SynonymDataExport_TurtleLine()
		{
			var opsType = new ExternalReferenceType() { UriSpace = Constants.OPSUriSpace };
			var csType = new ExternalReferenceType() { UriSpace = Constants.CSUriSpace };
			Compound compound = new Compound()
			{
				Smiles = new Smiles("C1C=CC=CC=1"),
				StandardInChI = new InChI("InChI=1S/C6H6/c1-2-4-6-5-3-1/h1-6H", "UHOVQNZJYSORNB-UHFFFAOYSA-N"),
				ExternalReferences = new List<ExternalReference>() { 
					new ExternalReference() { Type = opsType, Value = "23232" },
					new ExternalReference() { Type = csType, Value = "236" }
				},
				Synonyms = new List<Synonym>() { 
					new Synonym() { Name = "[6]annulene",
						State = CompoundSynonymState.eApproved,
						Flags = new List<SynonymFlag>(),
						LanguageId = "en"
					},
					new Synonym() { Name = "benzene", IsTitle = true, State = CompoundSynonymState.eApproved,
						Flags = new List<SynonymFlag>(),
						LanguageId = "en"
					},
					new Synonym() { Name = "349340-33-1", 
						State = CompoundSynonymState.eApproved,
						Flags = new List<SynonymFlag>() {new SynonymFlag() { Name = "DBID" } },
						LanguageId = "en"
					}
				}
			};
			List<Property> props = new List<Property>() { new Property() { Name = PropertyName.MOLECULAR_FORMULA,
				Value = "C6H6"} };
			DataSource ds = new DataSource();
			var export = new OpsDataSourceExport(dsGuid, DateTime.Now);
			SynonymsDataExportFile sdef = new SynonymsDataExportFile(export);
			//string result = sdef.TurtleLine(compound, "C6H6");
			//Assert.IsTrue(result.EndsWith(" ."));
			//Assert.AreNotEqual("", result);
			//Assert.AreEqual(@"<http://ops.rsc.org/OPS23232> cheminf:CHEMINF_000405 236; cheminf:CHEMINF_000396 ""InChI=1S/C6H6/c1-2-4-6-5-3-1/h1-6H""; cheminf:CHEMINF_000399 ""UHOVQNZJYSORNB-UHFFFAOYSA-N""; cheminf:CHEMINF_000018 ""C1C=CC=CC=1""; cheminf:CHEMINF_000490 ""C6H6""; cheminf:CHEMINF_000465 ""[6]annulene""@en; cheminf:CHEMINF_000476 ""benzene""@en; cheminf:CHEMINF_000467 ""349340-33-1""@en .", result);
		}

		[TestMethod]
		public void SynonymDataExport_PrefixLines()
		{
			DataSource ds = new DataSource();
			var export = new OpsDataSourceExport(dsGuid, DateTime.Now);
			SynonymsDataExportFile sdef = new SynonymsDataExportFile(export);
			var result = sdef.PrefixLines("ftp://ftp.rsc.org/dummyproperties", TurtlePrefixSets.Properties).ToList();
			Assert.IsTrue(result.Any(), "no prefixes produced");
			foreach (string line in result)
			{
				Assert.IsTrue(line.StartsWith("@prefix"), "malformed line: " + line);
				Assert.IsTrue(line.Contains(": <"), "malformed line: " + line);
				Assert.IsTrue(line.EndsWith("> ."), "malformed line: " + line);
			}
		}

		[TestMethod]
		public void SynonymDataExport_AnnotationProperties()
		{
			DataSource ds = new DataSource();
			var export = new OpsDataSourceExport(dsGuid, DateTime.Now);
			SynonymsDataExportFile sdef = new SynonymsDataExportFile(export);
			var result = sdef.AnnotationPropertyLines().ToList();
			Assert.AreEqual(10, result.Count);
			foreach (string line in result)
			{
				Assert.IsTrue(line.Contains(" . # "), "no explanation of what is going on in: " + line);
				Assert.IsTrue(line.Contains(" a "), "no predicate in: " + line);
				Assert.IsTrue(line.Contains("owl:AnnotationProperty"), "no assertion that this is an annotation property: " + line);
			}
		}

		[TestMethod]
		public void SynonymDataExport_CompoundToPredicateObjects()
		{
			string resultWithSmiles = compoundWithSmiles.ToPredicateObjects();
			Assert.IsTrue(resultWithSmiles.Contains("c1ccccc1"), "should contain SMILES");
			string resultWithoutSmiles = compoundWithoutSmiles.ToPredicateObjects();
			Assert.IsFalse(resultWithoutSmiles.Contains("c1ccccc1"), "should not contain SMILES");
		}

		[TestMethod]
		public void SynonymDataExport_ToChemSpiderLinksetLine()
		{
			string result = compoundWithSmiles.ToChemSpiderLinksetLine();
			Assert.IsTrue(result.Contains("skos:exactMatch"), result);
		}
	}
}
