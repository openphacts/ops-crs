using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSC.CVSP;
using RSC.CVSP.Compounds;
using RSC.CVSP.Compounds.Operations;
using RSC.Compounds;
using RSC.Properties;

namespace CVSPTests
{
    [TestClass]
    public class Operations : CVSPTestBase
    {
        /// <summary>
        /// for web services
        /// </summary>
        private static string token;
        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            Operations.token = ConfigurationManager.AppSettings["security_token"];
        }

        [TestMethod]
        public void CompoundRecord_AddChemSpiderProperty()
        {
            var cr = new CompoundRecord
            {
                Standardized = Resource1.benzeneAsMol
            };
            cr.AddChemSpiderProperty(PropertyName.CSID, "236");
            Assert.AreEqual("236", cr.Properties.First(p => p.Name == PropertyName.CSID).Value);
            cr.AddChemSpiderProperty(PropertyName.LOG_P, 2.13, 0.1);
            cr.AddChemSpiderProperty(PropertyName.LOG_D, 2.35, new Condition { Name = PropertyName.PH, Value = 5.5 });
            Assert.AreEqual(2.13, cr.Properties.First(p => p.Name == PropertyName.LOG_P).Value);
            Assert.AreEqual(0.1, cr.Properties.First(p => p.Name == PropertyName.LOG_P).Error);
            Assert.AreEqual(2.35, cr.Properties.First(p => p.Name == PropertyName.LOG_D).Value);
            Assert.IsTrue(cr.Properties.First(p => p.Name == PropertyName.LOG_D).Conditions.Any(),
                "log D should have condition");
        }

        [TestMethod]
        public void CompoundRecord_AddChemSpiderSynonym()
        {
            var cr = new CompoundRecord
            {
                Standardized = Resource1.benzeneAsMol
            };
            cr.AddChemSpiderSynonym(new Synonym { Name = "benzene" });
            var result = cr.GetChemSpiderSynonyms();
            Assert.IsTrue(result.Any(), "record should have synonyms");
            Assert.AreEqual("benzene", result.First().Name);
        }

        [TestMethod]
        public void CompoundRecord_AddChemSpiderSynonyms()
        {
            var cr = new CompoundRecord
            {
                Standardized = Resource1.benzeneAsMol
            };
            cr.AddChemSpiderSynonyms(new[] {
                new Synonym { Name = "benzene" },
                new Synonym { Name = "Benzol", LanguageId = "de" } });
            var result = cr.GetChemSpiderSynonyms();
            Assert.IsTrue(result.Any(), "record should have synonyms");
            Assert.AreEqual("benzene", result.First().Name);
            Assert.AreEqual("Benzol", result.ElementAt(1).Name);
        }

        [TestMethod]
        public void CompoundRecord_CalculateSMILES()
        {
            this.CheckSmilesGeneration(Resource1.benzeneAsMol);
            this.CheckSmilesGeneration(Resource1.OPS1136548);
        }

        public void CheckSmilesGeneration(string mol)
        {
            var benzene = new CompoundRecord
            {
                Standardized = mol,
                Original = mol
            };
            var cs = new CalculateSMILES();
            cs.ProcessRecord(benzene);
            Assert.IsTrue(benzene.HasProperty("SMILES"), "SMILES missing");
            Assert.IsNotNull(benzene.GetProperty("SMILES"));
            Console.WriteLine(benzene.GetProperty("SMILES"));
            Assert.IsTrue(benzene.HasProperty("StdSMILES"), "Standard SMILES missing");
            Assert.IsNotNull(benzene.GetProperty("StdSMILES"));
            Console.WriteLine(benzene.GetProperty("StdSMILES"));
        }

        [TestMethod]
        public void CompoundRecord_CalculateStdInChI()
        {
            var r = new CompoundRecord { Standardized = Resource1.benzeneAsMol };
            var csi = new CalculateStdInChI();
            csi.ProcessRecord(r);
            foreach (var p in r.Properties) Assert.IsNotNull(p.Name);
            Assert.IsTrue(r.HasProperty(PropertyName.STD_INCHI), "no std inchi");
            Assert.IsTrue(r.HasProperty(PropertyName.STD_INCHI_KEY), "no std inchi key");
        }

        [TestMethod]
        public void CompoundRecord_CalculateStdInChIs()
        {
            var molfiles = new List<string> { Resource1.benzeneAsMol, Resource1.deuteratedmolecule, Resource1.erythromycin };
            var records = molfiles.Select(m => new CompoundRecord { Standardized = m }).ToList();
            var csi = new CalculateStdInChI();
            csi.Process(records);
            foreach (var r in records)
            {
                Assert.IsTrue(r.HasProperty(PropertyName.STD_INCHI), "no std inchi");
                Assert.IsTrue(r.HasProperty(PropertyName.STD_INCHI_KEY), "no std inchi key");
            }
        }

        [TestMethod]
        public void Operations_DoEverythingMultipleRecords()
        {
            var molfileStrings = new[] {
                Encoding.UTF8.GetString(Resource1.failingNAD),
                Encoding.UTF8.GetString(Resource1.nitricoxide) };
            this.DoEverything(molfileStrings);
        }

        [TestMethod]
        public void Operations_DoEverythingSingleRecord()
        {
            this.DoEverything(new [] { Encoding.UTF8.GetString(Resource1.failingNAD) });
            this.DoEverything(new [] { Encoding.UTF8.GetString(Resource1.nitricoxide) });
            this.DoEverything(new[] { Encoding.UTF8.GetString(Resource1.heme) });
        }

        [TestMethod]
        public void OPS_Verification()
        {
            var cp = new CalculateParents(this.StandardizationModule, this.StandardizationChargesModule);
            var parents = cp.GenerateParents(Resource1.OPS1136544);
            foreach (var p in parents) Console.WriteLine(@"{0} {1}", p.Relationship,
                p.MolFile != null ? p.MolFile.Length : 0);
            Assert.AreEqual(4, parents.Count, "wrong number of parents for molecule");
            var standardized = parents.Select(p => this.Standardization.Standardize(p.MolFile, Resources.Vendor.Indigo)).ToList();
            var compounds = standardized.Zip(parents, Tuple.Create)
                .Select(
                    t => new CompoundRecord
                    {
                        Original = t.Item2.MolFile,
                        Standardized = t.Item1.Standardized,
                        Issues = t.Item1.Issues,
                        Properties = new List<Property>()
                    });
            new CalculateSMILES().Process(compounds);
            foreach (var c in compounds)
            {
                Console.WriteLine(c.Issues.Count());
                Console.WriteLine(c.Original);
                foreach (var p in c.Properties)
                    Console.WriteLine(@"{0} {1}", p.Name, p.Value);
                Assert.IsTrue(c.HasProperty(PropertyName.ORIGINAL_SMILES));
                Assert.IsTrue(c.HasProperty(PropertyName.STANDARDIZED_SMILES));
                Assert.IsFalse(string.IsNullOrEmpty(c.GetProperty("SMILES").ToString()));
                Console.WriteLine(@"SMILES = " + c.GetProperty("SMILES"));
                Console.WriteLine(@"Standard SMILES = " + c.GetProperty("StdSMILES"));
            }
        }

        public void DoEverything(IEnumerable<string> molfiles)
        {
            var standardized = molfiles.Select(m => this.Standardization.Standardize(m, Resources.Vendor.Indigo));
            var records = standardized.Zip(molfiles, Tuple.Create)
                .Select(t => new CompoundRecord
                {
                    Original = t.Item2,
                    Standardized = t.Item1.Standardized,
                    Issues = t.Item1.Issues
                }).ToList();
            new CalculateMolecularFormula().Process(records);
            new CalculateMolecularWeight().Process(records);
            new CalculateMonoisotopicMass().Process(records);
            new CalculateMostAbundantMass().Process(records);
            new CalculateNonStdInChI().Process(records);
            new CalculateParents(this.StandardizationModule, this.StandardizationChargesModule).Process(records);
            new CalculateSMILES().Process(records);
            new CalculateStdInChI().Process(records);
            new ImportChemSpiderProperties().Process(records);
            new ImportChemSpiderSynonyms().Process(records);
            foreach (var r in records)
                Assert.IsFalse(r.Issues.Select(c => c.Code).Contains("500.1"), "should not return 'processing failed'");
        }

        [TestMethod]
        public void PropertiesImport_Constructor()
        {
            var ws = new RSC.CVSP.Compounds.com.chemspider.www.PropertiesImport.Properties
            {
                Timeout = 30000
            };
            Assert.IsNotNull(ws);
        }

        [TestMethod]
        public void PropertiesImport_GetOperations()
        {
            var i = new ImportChemSpiderProperties();
            var ops = i.GetOperations().ToList();
            Assert.AreEqual(1, ops.Count, "wrong number of ops");
            Assert.AreEqual("Import ChemSpider Properties", ops.First().Name, "wrong name");
        }

        [TestMethod]
        public void PropertiesImport_Process()
        {
            var csi = new CalculateStdInChI();
            var i = new ImportChemSpiderProperties();
            var molfiles = new List<string> { Resource1.benzeneAsMol, Resource1.deuteratedmolecule, Resource1.erythromycin };
            var records = molfiles.Select(m => new CompoundRecord { Standardized = m }).ToList();
            csi.Process(records);
            foreach (var r in records)
            {
                Assert.IsNotNull(r.Properties, "properties should not be null");
                Assert.IsTrue(r.Properties.Any(), "there should be some properties");
                Assert.IsTrue(r.HasProperty(PropertyName.STD_INCHI), "no std inchi");
                Assert.IsTrue(r.HasProperty(PropertyName.STD_INCHI_KEY), "no std inchi key");
            }
            i.Process(records);
            foreach (var r in records)
            {
                Assert.IsTrue(r.HasProperty("KOC"), "no KOC for " + r.StandardizedStdInChI.Inchi);
            }
        }

        [TestMethod]
        public void PropertiesImport_WebService()
        {
            var ws = new RSC.CVSP.Compounds.com.chemspider.www.PropertiesImport.Properties
            {
                Timeout = 30000
            };
            // try empty list
            var list = new string[] { };

            var result = ws.RetrieveByInChIKeyList(list, Operations.token).ToList();
            Assert.AreEqual(0, result.Count, "shouldn't return anything");
            var actuallist = new[] { "UHOVQNZJYSORNB-UHFFFAOYSA-N", "NXHAKHHKDBVHPV-UHFFFAOYSA-N",
                "KAESVJOAVNADME-UHFFFAOYSA-N", "RCINICONZNJXQF-MZXODVADSA-N" };
            var result2 = ws.RetrieveByInChIKeyList(actuallist, Operations.token).ToList();
            Assert.AreEqual(4, result2.Count, "wrong number of results");
            // test some CSIDs
            Assert.AreEqual(236, result2.First().CSID, "wrong CSID");
            Assert.AreEqual(7736, result2.ElementAt(1).CSID, "wrong CSID");
            Assert.AreEqual(10368587, result2.ElementAt(2).CSID, "wrong CSID");
            Assert.AreEqual(16787685, result2.ElementAt(3).CSID, "wrong CSID");
            Assert.AreEqual(207.276, result2.First().Parachor, "wrong parachor for benzene");
            Assert.AreEqual(4, result2.First().ParachorError, "wrong parachor error for benzene");
        }

        [TestMethod]
        public void SynonymsImport_Constructor()
        {
            var ws = new RSC.CVSP.Compounds.com.chemspider.www.Synonyms.Synonyms
            {
                Timeout = 30000
            };
            Assert.IsNotNull(ws);
        }

        [TestMethod]
        public void SynonymsImport_GetOperations()
        {
            var i = new ImportChemSpiderSynonyms();
            var ops = i.GetOperations().ToList();
            Assert.AreEqual(1, ops.Count, "wrong number of ops");
            Assert.AreEqual("Import ChemSpider Synonyms", ops.First().Name, "wrong name");
        }

        /// <summary>
        /// This depends on the CSID!
        /// </summary>
        [TestMethod]
        public void SynonymsImport_Process()
        {
            var csi = new CalculateStdInChI();
            var ip = new ImportChemSpiderProperties();
            var molfiles = new List<string> { Resource1.benzeneAsMol, Resource1.deuteratedmolecule, Resource1.erythromycin };
            var records = molfiles.Select(m => new CompoundRecord { Standardized = m }).ToList();
            foreach (var r in records) csi.ProcessRecord(r);
            ip.Process(records);
            var i = new ImportChemSpiderSynonyms();
            i.Process(records);
            this.TestChemSpiderSynonymPresent(records.First(), "benzene");
            this.TestChemSpiderSynonymPresent(records.ElementAt(1), "acetaminophen-d3");
            this.TestChemSpiderSynonymPresent(records.ElementAt(2), "erythromycin");
        }

        [TestMethod]
        public void SynonymsImport_WebService()
        {
            var ws = new RSC.CVSP.Compounds.com.chemspider.www.Synonyms.Synonyms
            {
                Timeout = 30000
            };
            var csids = Enumerable.Range(236, 10);
            var synonyms = ws.RetrieveByCSIDList(csids.ToArray(), Operations.token).ToList();
            Assert.IsTrue(synonyms.Count > 100, "not enough synonyms {0}, should be at least 100", synonyms.Count);
            Assert.IsTrue(synonyms.Any(s => s.Synonym.ToLower() == "benzene"), "at least one of them should be benzene: " + string.Join("; ", synonyms.Select(s => s.Synonym)));
        }

        public void TestChemSpiderSynonymPresent(CompoundRecord cr, string synonym)
        {
            Assert.IsTrue(cr.GetChemSpiderSynonyms().Any(s => s.Name.ToLower() == synonym), "'" + synonym + "' not found: "
                + string.Join("; ", cr.GetChemSpiderSynonyms().Select(s => s.Name)));
        }
    }
}