using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MoleculeObjects;
using com.ggasoftware.indigo;
using RSC.CVSP.Compounds;

namespace CVSPTests
{
    /// <summary>
    /// 2015-08-27.
    /// All of these tests are now present in RSCCheminfToolkit.
    /// </summary>
    [TestClass]
    public class CheminformaticsCdx
    {
        [TestMethod]
        public void Cdx_DoubleBondedPhenylTest()
        {
            Cdx cdx = new Cdx(Resource1.doublebondPh);
            string ct = cdx.ToMolFile();
            Console.WriteLine(ct);
            GenericMolecule gm = MoleculeFactory.FromMolV2000(ct);
            Assert.IsTrue(gm.HasNonInChIfiableAtoms(), "double bond Ph does not contain non-InChIfiable atoms");
        }

        /// <summary>
        /// C6F13 should be left as a label and not expanded by ChemDraw.
        /// </summary>
        [TestMethod]
        public void Cdx_PerfluoroalkylTest()
        {
            Cdx perfluoroalkylcompound = new Cdx(Resource1.perfluoroalkyl);
            var result = perfluoroalkylcompound.ToMolFile();
            Assert.IsTrue(result.Contains("C6F = C6F13"), "problem with expansion, see molfile " + result);
        }

        [TestMethod]
        public void Cdx_OrientationTest()
        {
            Cdx nitrile = new Cdx(Resource1.nitrile);
            var result = MoleculeFactory.Molecule(nitrile.ToMolFile());
            Assert.IsFalse(result.Match("[C-]#[N+]",Toolkit.OpenEye), result.ToSMILES());
        }

        [TestMethod]
        public void Cdx_AbsurdlyLargeMoleculeTest()
        {
            Cdx neg = new Cdx(Resource1.benzeneAsCdx);
            string negoot = neg.ToMolFile();
            Cdx c = new Cdx(Resource1.absurdlyLargeMolecule);
            try
            {
                string oot = c.ToMolFile();
                Assert.Fail("mol file generation should throw an error");
            }
            catch (MoleculeException me)
            {
                Assert.IsTrue(me.ToString().Contains("too many atoms"));
            }
        }

        [TestMethod]
        public void Cdx_LabelsInBoxTest()
        {
            PartitionedCdx p = new PartitionedCdx(Resource1.reactionWithAbbreviations);
            Assert.AreEqual(1, p.LabelsInBox(p.reagentBox).Count());
        }

        [TestMethod]
        public void Cdx_CommonReagentsInBoxTest()
        {
            PartitionedCdx p = new PartitionedCdx(Resource1.reactionWithAbbreviations);
            Assert.AreEqual(4, p.CommonReagentsInBox(p.reagentBox).Count());
            Assert.AreEqual(3, p.CommonReagentsInBox(p.reagentBox).Distinct().Count());
        }
        
        /// <summary>
        /// ChemDraw 6.0.2 seemed to create a lot of these.
        /// </summary>
        [TestMethod]
        public void Cdx_PhantomInternalStructureTest()
        {
            // negative test - should exclude phantom structure
            Cdx c = new Cdx(Resource1.phantominternalstructure);
            Molecule m = MoleculeFactory.Molecule(c.ToMolFile(CdxEnumerateOptions.EnumerateMarkush));
            Assert.IsTrue(m.ToSMILES(true).Contains("CC1C=C(C2C=CC3C(=NC(C)=CC=3C3C=CC=CC=3)C=2N=1)C1C=CC=CC=1"),
                m.ToSMILES(true));
            // positive test - should ensure structure inside goes through
            Cdx c2 = new Cdx(Resource1.germole);
            Molecule m2 = MoleculeFactory.Molecule(c2.ToMolFile());
        }

        [TestMethod]
        public void Cdx_ResinousBlobsTest()
        {
            Cdx a = new Cdx(Resource1.artwork);
            Assert.AreEqual(3, a.ResinousBlobs.Count, "wrong number of resinous blobs");
            //subtract two because there is a fourth star under Generics in the props.
            Assert.AreEqual(3, a.ToMolFile().Split(new char[] { '*' }).Count() - 2, "wrong number of stars: ");
        }

        [TestMethod]
        public void Cdx_NotACdxFileTest()
        {
            try
            {
                RawCdx r = new RawCdx(Resource1.ch4chm);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.ToString().Contains("3.x"), "file not identified as 3.x");
            }
            Cdx c = new Cdx(Resource1.benzeneAsCdx); // and should just be created and not fall over in order for test to pass
        }

        /// <summary>
        /// We rarely find a Text object which contains no text! This cdx file contains one.
        /// </summary>
        [TestMethod]
        public void Cdx_IllFormedCdxTextTest()
        {
            Cdx cdx = new Cdx(Resource1.illFormedCdxText);
        }
        
        // [TestMethod] broken
        public void Cdx_IdentifyWagglyFragmentsTest()
        {
            var combinedMol = MoleculeFactory.Molecule(Resource1.fragmentsMol);
            List<Molecule> untangledMols = combinedMol.EnumerateDistinctMolecules().ToList();
            Sdf sdf = new Sdf(String.Concat(from m in untangledMols select m.ToString()));
            Console.WriteLine("molecule count = " + sdf.molecules.Count);
            Console.WriteLine("generics count = " + sdf.genericMolecules.Count);
            sdf.ProcessEthanesToFragments();
            File.WriteAllText("wagglyout.sdf", sdf.ToString());
            Assert.IsTrue(sdf.genericMolecules.Count == 5,
                 String.Format("should be 5 generics (actually {0})", sdf.genericMolecules.Count));
            Assert.IsTrue(sdf.molecules.Count == 0,
                String.Format("should be no bona fide molecules (actually {0})", sdf.molecules.Count));
        }

        [TestMethod]
        public void Cdx_NoFragmentsTest()
        {
            CdxCountingTest(Resource1.nofragments, "should be no phantom fragment", 0);
        }
        
        [TestMethod]
        public void Cdx_InvisibleMoleculeTest()
        {
            CdxCountingTest(Resource1.invisiblemolecule, "invisible molecule!", 1);
        }

        [TestMethod]
        public void Cdx_NamedGroupWithTwoBondsTest()
        {
            Cdx cdx = new Cdx(Resource1.germole);
            var mols = cdx.ToMolFiles(CdxEnumerateOptions.EnumerateMarkush);
            Assert.AreEqual(1, mols.ToList().Count);
            Console.WriteLine(mols.First());
            Indigo i = new Indigo();
            string SMILES = (i.loadMolecule(mols.First())).canonicalSmiles().Split(new char[] { ' ' }).First();
            Assert.AreEqual("C[Ge]1(C)C2C=CC=CC=2C(CCC)=C1CCC", SMILES);
        }

        [TestMethod]
        public void Cdx_EnumerationsTest()
        {
            Cdx cdx = new Cdx(Resource1.bracketsN);
            var amf = cdx.ToMolFiles(CdxEnumerateOptions.EnumerateMarkush);
            Assert.AreEqual(2, amf.ToList().Count, "wrong number of molfiles returned" + String.Concat(amf));

            Console.WriteLine(amf.First());

            GenericMolecule first = MoleculeFactory.FromMolV2000(amf.First());
            GenericMolecule second = MoleculeFactory.FromMolV2000(amf.ElementAt(1));
            Assert.AreEqual("R = H", first.FirstProperty("Enumeration"));
            Assert.AreEqual("R = CO2H", first.Property("Enumeration")[1]);
            Assert.AreEqual("R", first.FirstProperty("Generics"));
            Assert.AreEqual(2, first.Property("labels").Count);
            Assert.AreEqual("R = H", second.FirstProperty("Enumeration"));
            Assert.AreEqual("R", second.FirstProperty("Generics"));
            Assert.AreEqual(1, second.Property("labels").Count);
        }

        [TestMethod]
        public void Cdx_AbbreviationsOnlyTest()
        {
            CdxCountingTest(Resource1.justAbbreviations, "wrong number of abbreviated molecules", 10);
        }

        [TestMethod]
        public void Cdx_SingleAtomsTest()
        {
            CdxCountingTest(Resource1.atoms, "wrong number of atoms", 8);
        }

        [TestMethod]
        public void Cdx_WrongWayBondsTest()
        {
            Cdx cdx = new Cdx(Resource1.wrongwaybonds);
            string ct = cdx.ToMolFiles().First();
            Assert.IsTrue(ct.Contains("2  6  1  1"), "one up bond in pentacycle missing");
            Assert.IsTrue(ct.Contains("7  6  1  1"), "other up bond in pentacycle missing");
        }

        /// <summary>
        /// To make sure that Ar gets interpreted as a Markush expression rather than argon.
        /// </summary>
        [TestMethod]
        public void Cdx_AromaticMarkushTest()
        {
            Cdx cdx = new Cdx(Resource1.aromaticMarkush);
            var result = cdx.ToMolFiles();
            Assert.IsTrue(result.First().Contains("Generics"), "problem with interpretation of Ar: " + String.Concat(result));
        }

        //[TestMethod] Broken
        public void NonsensicalStereoTest()
        {
            CdxCountingTest(Resource1._1_calix, "nonsensical stereo interpretation failure");
        }

        /// <summary>
        ///A test for cdxToSdf
        ///</summary>
        [TestMethod]
        public void Cdx_ToSdfTest()
        {
            string cdxfile = "benzene.cdx";
            File.WriteAllBytes(cdxfile, Resource1.benzeneAsCdx);
            Sdf sdf = new Sdf(String.Concat(new Cdx(Resource1.benzeneAsCdx).ToMolFiles()));
            Assert.IsTrue(sdf.molecules.Count == 1, "There should only be one molecule in this sdf");
            Assert.AreEqual("C1C=CC=CC=1", MoleculeFactory.Molecule(sdf.molecules.First().ToString()).ToSMILES(true));
        }

        [TestMethod]
        public void Cdx_MultipleMoleculeToSdfTest()
        {
            Sdf sdf = new Sdf(String.Concat(new Cdx(Resource1.multiplemolecules).ToMolFiles()));
            Console.WriteLine(sdf.ToString());
            Assert.IsTrue(sdf.molecules.Count > 1, "only one (or maybe zero) molecules in this sdf");
        }

        [TestMethod]
        public void Cdx_MultipleMoleculeSdfFileRoundTripTest()
        {
            string cdxfile = "multiplemolecules.cdx";
            File.WriteAllBytes(cdxfile, Resource1.multiplemolecules);
            Sdf sdf = new Sdf(String.Concat(new Cdx(Resource1.benzeneAsCdx).ToMolFiles()));
                
            string sdffile = "multiplemolecules.sdf";
            File.WriteAllText(sdffile, sdf.ToString());
            Sdf sdf2 = new Sdf(File.ReadAllText(sdffile));
        }

        [TestMethod]
        public void CdxPath_DoubleSlashSingleSlashTest()
        {
            Cdx cdx = new Cdx(Resource1.twomarkush);
            List<CdxObject> matches = cdx.CdxPathSelectObjects("//Node[@Node_Type=0]/Text");
            matches.ForEach(m => Console.WriteLine(String.Format("ID = {0}", m.ID)));

            Assert.AreEqual(1, matches.Count);
        }

        [TestMethod]
        public void CdxPath_FragmentNoPredicateTest()
        {
            Cdx cdx = new Cdx(Resource1.PhH);
            List<CdxObject> matches = cdx.CdxPathSelectObjects("//Fragment");
            Assert.IsTrue(matches.Count == 2,
                String.Format("wrong number ({0}) of Fragments found", matches.Count));
            Assert.IsTrue(matches[1].ID == 3,
                "first Fragment has wrong ID, should be 3, is " + matches[1]);
            Assert.IsTrue(matches[0].ID == 10,
                "second Fragment has wrong ID, should be 10, is " + matches[0]);
            Assert.IsTrue(matches[0].Objects.Count == 14,
                "second fragment has wrong number of children; should be 14, is " +
                matches[0].Objects.Count);
        }

        [TestMethod]
        public void CdxPath_FragmentParentPredicateTest()
        {
            Cdx cdx = new Cdx(Resource1.PhH);
            Console.Write(cdx.Root.ToString(0));
            List<CdxObject> matches = cdx.CdxPathSelectObjects("//Fragment[not(parent::Node)]");
            Assert.IsTrue(matches.Count == 1,
                String.Format("wrong number ({0}) of Fragments found", matches.Count));

        }

        private void TestBoldFaceDictionary(Cdx cdx, string key, string matchingValue)
        {
            List<CdxText> m_labels = cdx.Labels;
            Console.WriteLine();
            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            foreach (CdxText label in m_labels)
            {
                Console.WriteLine("LABEL " + label.Value);
                if (label.BoldFaceDictionary.Count > 0)
                {
                    foreach (KeyValuePair<string, List<string>> pair in label.BoldFaceDictionary)
                    {
                        Console.WriteLine("KEY = " + pair.Key + " VALUE = " + String.Join("; ", pair.Value));
                        dict.Add(pair.Key, pair.Value);
                    }
                }
            }
            Assert.IsTrue(dict.Count > 0, "count = " + dict.Count);
            Assert.IsTrue(dict.ContainsKey(key), String.Join("; ", from k in dict.Keys select k));
            Assert.IsTrue(dict[key].First() == matchingValue,
                String.Format("first dictionary coda should be {0}, actually {1}", matchingValue, dict[key].First()));
        }

        [TestMethod]
        public void Cdx_ExtractBoldFaceFromLabels()
        {
            TestBoldFaceDictionary(new Cdx(Resource1.selenacalixtriazines), "1a", "R = n-Bu");
            TestBoldFaceDictionary(new Cdx(Resource1.RnOH), "2", "n=2, R = OH");
        }

        [TestMethod]
        public void Cdx_GenericNicknamePhenylAcetateTest()
        {
            Cdx cdx = new Cdx(Resource1.phenylacetate);
            Sdf sdf = new Sdf(String.Concat(cdx.ToMolFiles()));
            //Console.WriteLine("sdf contains {0} molecules and {1} generics", sdf.molecules.Count, sdf.genericMolecules.Count);
            Assert.IsTrue(sdf.molecules.Count > 0, "no real molecules in sdf:" + sdf.ToString());
            Assert.IsFalse(sdf.molecules.First().HasProperty("Generics"), "first molecule has Generics property. wrongly: " + sdf.ToString());
        }

        [TestMethod]
        public void Cdx_UninterpretableFirstTest()
        {
            CdxCountingTest(Resource1.UninterpretableFirst, "S-tolyl compound not found");
        }

        [TestMethod]
        public void Cdx_UnrecognizedAbbreviationTest()
        {
            CdxCountingTest(Resource1.Cpcyclohexane, "cyclopentadienylcyclohexane not found");
        }

        [TestMethod]
        public void Cdx_GenericNicknameObjectTest()
        {
            CdxCountingTest(Resource1.Rcyclohexane, "R-cyclohexane not found");
        }

        [TestMethod]
        public void Cdx_MultipageCdxTest()
        {
            CdxCountingTest(Resource1.task128, "wrong number", 55);
        }

        public void CdxCountingTest(byte[] resource, string message)
        {
            CdxCountingTest(resource, message, 1);
        }

        public void CdxCountingTest(byte[] resource, string message, int count)
        {
            Cdx cdx = new Cdx(resource);
            var cts = cdx.ToMolFiles();
            Assert.AreEqual(count, cts.ToList().Count, message);
        }

        /// <summary>
        /// Ensures ToMolFile returns the correct number of molecules.
        /// </summary>
        [TestMethod]
        public void Cdx_ToMolFileCountTest()
        {
            Cdx cdx = new Cdx(Resource1.node_type1);
            string smiles = MoleculeFactory.Molecule(cdx.ToMolFile()).ToSMILES(true);
            Assert.AreEqual(2, smiles.Split(new char[] { '.' }).Count(), smiles);
        }

        /// <summary>
        /// Cdx NodeType 1 is a simple Element.
        /// http://www.cambridgesoft.com/services/documentation/sdk/chemdraw/cdx/properties/Node_Type.htm
        /// ChemSketch implements these slightly differently.
        /// </summary>
        [TestMethod]
        public void Cdx_NodeType1Tests()
        {
            Cdx cdx = new Cdx(Resource1.node_type1);
            Cdx cs = new Cdx(Resource1.NodeType1ChemSketch);
        }

        /// <summary>
        /// Cdx NodeType 5 is a Fragment; see
        /// http://www.cambridgesoft.com/services/documentation/sdk/chemdraw/cdx/properties/Node_Type.htm
        /// </summary>
        [TestMethod]
        public void Cdx_NodeType5Test()
        {
            CdxCountingTest(Resource1.cyclohexyl_acetate, "cyclohexyl acetate not found");
        }

        /// <summary>
        /// Cdx NodeType 4 is a Nickname; see
        /// http://www.cambridgesoft.com/services/documentation/sdk/chemdraw/cdx/properties/Node_Type.htm
        /// 
        /// Note that the data structures are in fact subtly different from 5.
        /// 
        /// hexaTHPcyclohexane should have 42 heavy atoms and 48 bonds (second assertion).
        /// </summary>
        [TestMethod]
        public void Cdx_NodeType4Test()
        {
            Cdx cdx = new Cdx(Resource1.hexaTHPcyclohexane);
            var cts = cdx.ToMolFiles();
            Assert.AreEqual(1, cts.ToList().Count, "hexaTHPcyclohexane not found" + cts.First());
            Assert.IsTrue(cts.First().Contains(" 42 48"), "wrong atom and bond count, should be 42 atoms and 48 bonds" + cts.First());

            Cdx cdx2 = new Cdx(Resource1.tetrahydropyranyl);
            var cts2 = cdx2.ToMolFiles();
            Assert.AreEqual(1, cts2.ToList().Count, "THP-cyclohexane not found" + cts.First());
        }

        /// <summary>
        /// NodeType 10 is for the likes of ferrocene.
        /// We have no V2000 output for this, so just map to a dummy node.
        /// For now, simply check that it parses and doesn't go down in flames.
        /// </summary>
        [TestMethod]
        public void Cdx_NodeType10Test()
        {
            Cdx cdx = new Cdx(Resource1.NodeType10);
        }

        /// <summary>
        /// To avoid nodes saying "DMF" being parsed as deuterium connected to an invalid atom connected
        /// to fluorine.
        /// </summary>
        [TestMethod]
        public void Cdx_NodeContainsFragmentContainsNodeType7Test()
        {
            Cdx cdx = new Cdx(Resource1.NodeContainsFragmentContainsNodeType7);
            Molecule mol = MoleculeFactory.Molecule(cdx.ToMolFile());
        }

        [TestMethod]
        public void Cdx_AbbreviationsTest()
        {
            Cdx cdx = new Cdx(Resource1.the32);
            var cts = cdx.ToMolFiles();
            Assert.AreEqual(32, cts.ToList().Count, String.Concat(cts));
        }

        [TestMethod]
        public void Cdx_UnusualChargesTest()
        {
            Cdx q6 = new Cdx(Resource1.charge6);
            string result6 = q6.ToMolFiles().First();
            Assert.IsTrue(result6.Contains("M  CHG  7   1   6"), "problem with +6: " + result6);
            Cdx q8 = new Cdx(Resource1.charge8);
            string result8 = q8.ToMolFiles().First();
            Assert.IsTrue(result8.Contains("M  CHG  5   1   8"), "problem with +8: " + result8);
        }

        [TestMethod]
        public void Cdx_DashyStereoTest()
        {
            // test special option actually works
            Cdx dashy = new Cdx(Resource1.dashystereo, CdxDashedBondOptions.Down);
            Molecule m = MoleculeFactory.Molecule(dashy.ToMolFiles().First());
            Assert.AreEqual("NCCC(=O)N[C@@H](CC1=CN=CN1)C(O)=O", m.ToSMILES(true));

            // now test default
            Cdx undashy = new Cdx(Resource1.dashystereo);
            Molecule um = MoleculeFactory.Molecule(undashy.ToMolFiles().First());
            Assert.AreNotEqual("N1=CNC(C[C@@H](C(O)=O)NC(=O)CCN)=C1", um.ToSMILES(true));
        }

        /// <summary>
        /// Sometimes people draw a plus sign insufficiently close to the atom for ChemDraw to recognize it as such.
        /// </summary>
        [TestMethod]
        public void Cdx_FloatingPlusTest()
        {
            Cdx negative = new Cdx(Resource1.OTfstrayplus);
            var molecule = MoleculeFactory.FromMolV2000(negative.ToMolFile(CdxEnumerateOptions.EnumerateMarkush));
            Assert.AreEqual(0, molecule.TotalCharge(), "molecule should not be charged: " + molecule.ct());

            Cdx floatingplus = new Cdx(Resource1.floatingplus);
            var molfiles = floatingplus.ToMolFiles(CdxEnumerateOptions.EnumerateMarkush);
            var first = MoleculeFactory.Molecule(molfiles.First());
            var second = MoleculeFactory.Molecule(molfiles.ElementAt(1));
            Assert.AreEqual(-1, first.TotalCharge(), "problem with first molecule's charge");
            Assert.AreEqual(1, second.TotalCharge(), "problem with second molecule's charge");
            File.WriteAllText("argh.mol", second.ToString());
            Assert.AreEqual("CC(C)C1C=CC=C(C(C)C)C=1[N+]1=C(CP(C2CCCCC2)C2CCCCC2)N(C2C(=CC=CC=2C(C)C)C(C)C)C(C)=C1C", second.ToSMILES(true));
        }
    }

    [TestClass]
    public class CdxBracketsTests
    {
        [TestMethod]
        public void CdxBrackets_nEquals0Test()
        {
            Cdx cdx = new Cdx(Resource1.nEquals0);
            var cts = cdx.ToMolFiles();
            File.WriteAllText("nEquals0.txt", String.Concat(cts));

            Assert.AreEqual(1, cts.ToList().Count, "Wrong number of connection tables pulled out.");
            Indigo i = new Indigo();
            string smiles = i.loadMolecule(cts.First()).canonicalSmiles();
            Assert.AreEqual("CC", smiles, "ethane not found");
        }

        [TestMethod]
        public void CdxBrackets_VisualBracketLabelTest()
        {
            Cdx cdx = new Cdx(Resource1.bracketsN);
            Assert.AreEqual(1, cdx.Labels.Count, "Wrong number of labels identified.");
        }

        [TestMethod]
        public void CdxBrackets_BracketedBondToAbbreviationTest()
        {
            Cdx cdx = new Cdx(Resource1.bracketedBondToAbbreviation);
            File.WriteAllBytes("bba.cdx", Resource1.bracketedBondToAbbreviation);
            Indigo i = new Indigo();
            string smiles = i.loadMolecule(cdx.ToMolFiles().First()).canonicalSmiles();
            File.WriteAllText("bracketedbondtoabbreviation.mol", cdx.ToMolFiles().First());
            Assert.IsTrue(smiles == "OCCCCC1C=CC=CC=1", smiles + " should be " + "OCCCCC1C=CC=CC=1");
        }

        [TestMethod]
        public void CdxBrackets_XYTest()
        {
            Cdx cdx = new Cdx(Resource1.bracketsxbracketsy);
            List<string> cts = cdx.ToMolFiles().ToList();
            Assert.AreEqual(6, cts.ToList().Count, String.Concat(cts));
            File.WriteAllText("bracketsxbracketsy.sdf", String.Join("", cts));

            Indigo i = new Indigo();
            IEnumerable<string> smiles = from ct in cts select (i.loadMolecule(ct)).canonicalSmiles();
            foreach (string smile in smiles)
            {
                Console.WriteLine(smile);
            }
            Assert.IsTrue(smiles.Contains("CC(=O)C(N)CO"), "x = 0, y = 1 case not found");
            Assert.IsTrue(smiles.Contains("CC(=O)CC(N)CO"), "x = 1, y = 1 case not found");
            Assert.IsTrue(smiles.Contains("CC(=O)CCC(N)CO"), "x = 2, y = 1 case not found");
            Assert.IsTrue(smiles.Contains("CC(=O)C(N)CCO"), "x = 0, y = 2 case not found");
            Assert.IsTrue(smiles.Contains("CC(=O)CC(N)CCO"), "x = 1, y = 2 case not found");
            Assert.IsTrue(smiles.Contains("CC(=O)CCC(N)CCO"), "x = 2, y = 2 case not found");
        }

        [TestMethod]
        public void CdxBrackets_ExpansionTest()
        {
            Cdx cdx = new Cdx(Resource1.OO_OCO_OCCO);
            List<string> cts = cdx.ToMolFiles().ToList();

            Console.WriteLine(String.Concat(cts));

            Indigo i = new Indigo();
            Assert.IsTrue((i.loadMolecule(cts.First())).canonicalSmiles() == "OO", "OO not found");
            Assert.IsTrue((i.loadMolecule(cts[1])).canonicalSmiles() == "OCO", "OCO not found");
            Assert.IsTrue((i.loadMolecule(cts[2])).canonicalSmiles() == "OCCO", "OCCO not found");
        }

        [TestMethod]
        public void CdxBrackets_LongRepeatsTest()
        {
            string desiredSMILES = "CC(O)CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCOCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC(C)O";

            Cdx cdx = new Cdx(Resource1.longrepeats);
            List<string> cts = cdx.ToMolFiles().ToList();
            File.WriteAllText("longrepeats.mol", cts.First());
            Indigo i = new Indigo();
            string actualSMILES = i.loadMolecule(cts.First()).canonicalSmiles();
            Assert.AreEqual(desiredSMILES, actualSMILES,
                "problem with long repeats, expected " + desiredSMILES + ", got " + actualSMILES);
        }

        [TestMethod]
        public void CdxBrackets_ExplicitExternalConnectionPointsTest()
        {
            Cdx cdx = new Cdx(Resource1.cavitand);
            Sdf sdf = new Sdf(String.Concat(cdx.ToMolFiles(CdxEnumerateOptions.EnumerateMarkush)));
            File.WriteAllText("cavitand_out.sdf", sdf.ToString());
            Indigo i = new Indigo();
            IndigoObject cavitand = i.loadMolecule(sdf.molecules.First().ct());
            cavitand.dearomatize();
            Console.WriteLine(cavitand.canonicalSmiles());
            IndigoObject ringQuery = i.loadSmarts("c1@c@c@C@c@c@c@C@c@c@c@C@c@c@c@C1");
            IndigoObject matchRing = i.substructureMatcher(cavitand).match(ringQuery);
            Assert.IsTrue(matchRing != null);
            // also test for fully expanded whatnot
        }

        /// <summary>
        /// The difficulty with ()n notation is that the expansion has to be done at the
        /// level of the cdx file rather than at the level of the sdf.
        /// </summary>
        //[TestMethod] Colin will fix after fall ACS of 2013
        public void CdxBrackets_RnOHTest()
        {
            Cdx cdx = new Cdx(Resource1.RnOH);
            List<string> cts = cdx.ToMolFiles().ToList();
            Assert.IsTrue(cts.Count == 2,
                String.Format("Wrong number {0} of connection tables pulled out.", cts.Count));
            CompoundEnumerator ce = new CompoundEnumerator();
            List<Molecule> result = new List<Molecule>();
            foreach (string ct in cts)
            {
                GenericMolecule gm = MoleculeFactory.FromMolV2000(ct);
                foreach (var prop in gm.Properties)
                {
                    Console.WriteLine(prop.Key + ": " + String.Join(";", prop.Value));
                }
                if (gm.HasProperty("Enumeration"))
                {
                    result.AddRange(ce.SubstituteMarkush(gm, gm.Property("Enumeration")));
                }
                else { Console.WriteLine("Enumeration property missing"); }
                Console.WriteLine();
            }
            File.WriteAllText("rnoh.sdf", String.Concat(from m in result.Distinct() select m.ToString()));

            Console.WriteLine("{0} molecules in result", result.Count);
            Indigo i = new Indigo();
            IEnumerable<string> smiles = from m in result select (i.loadMolecule(m.ct())).canonicalSmiles();
            Assert.IsTrue(smiles.Contains("O"), "water not found " + String.Join("; ", smiles));
            Assert.IsTrue(smiles.Contains("OCCO"), "ethylene glycol not found" + String.Join("; ", smiles));
        }

        [TestMethod]
        public void CdxBrackets_UnlabelledBracketsTest()
        {
            Cdx cdx = new Cdx(Resource1.unlabelledbrackets);
            List<string> mols = cdx.ToMolFiles().ToList();
            Assert.IsTrue(mols.Count == 1, "problem with molfile" + String.Concat(mols));
        }

        [TestMethod]
        public void CdxBrackets_OneEndedBracketTest()
        {
            Cdx cdx = new Cdx(Resource1.PPh2and3);
            List<string> mols = cdx.ToMolFiles().ToList();
            Assert.IsTrue(mols.Count == 2,
                String.Format("wrong number ({0}, not 2) of mol files: {1}",
                    mols.Count, string.Concat(mols)));
            Indigo i = new Indigo();
            IEnumerable<string> SMILES = from m in mols select (i.loadMolecule(m)).canonicalSmiles();
            Console.WriteLine(String.Join("; ", SMILES));
            Assert.IsFalse(SMILES.Contains("PC1C=CC=CC=1"), String.Join("; ", SMILES));
            Assert.IsTrue(SMILES.Contains("C1=CC=CC=C1P(C1C=CC=CC=1)C1C=CC=CC=1"), String.Join("; ", SMILES));
        }

        [TestMethod]
        public void CdxBrackets_DashedBondAcrossBracketTest()
        {
            // should not crash.
            Cdx cdx = new Cdx(Resource1.dashedbondacrossbracket);
            string oot = cdx.ToMolFile();
        }

        [TestMethod]
        public void CdxBrackets_ExpansionDiagnosticMessageTests()
        {
            Console.WriteLine("NO-ENDER");
            Cdx noender = new Cdx(Resource1.cavitand);
            String noenderSdf = String.Concat(noender.ToMolFiles().ToList());
            Assert.IsTrue(noenderSdf.Contains("CDXDiagnostics"), noenderSdf);
            Assert.IsTrue(noenderSdf.Contains("macrocycle"), "did not identify no-ended bracket (macrocycle): " + noenderSdf);

            Console.WriteLine("ONE-ENDER");
            Cdx oneender = new Cdx(Resource1.PPh2and3);
            String oneenderSdf = String.Concat(oneender.ToMolFiles().ToList());
            Assert.IsTrue(oneenderSdf.Contains("CDXDiagnostics"), oneenderSdf);
            Assert.IsTrue(oneenderSdf.Contains("one-ended bracket"), "did not identify one-ended bracket: " + oneenderSdf);

            Console.WriteLine("TWO-ENDER");
            Cdx twoender = new Cdx(Resource1.RnOH);
            String twoenderSdf = String.Concat(twoender.ToMolFiles().ToList());
            Assert.IsTrue(twoenderSdf.Contains("CDXDiagnostics"), twoenderSdf);
            Assert.IsTrue(twoenderSdf.Contains("two-ended bracket"), "did not identify two-ended bracket: " + twoenderSdf);
        }
    }
}
