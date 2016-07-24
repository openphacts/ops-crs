
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ChemSpider.Molecules;
using RSC.CVSP.Compounds;
using MoleculeObjects;
using com.ggasoftware.indigo;
using InChINet;
using RSC.CVSP;

namespace CVSPTests
{
    [TestClass]
    public class StandardizationTests : CVSPTestBase
    {
        static Reactor r;
        static Indigo i;

        [ClassInitialize]
        public static void InitializeStandardization(TestContext tc)
        {
            r = new Reactor();
            i = new Indigo();
            i.setOption("ignore-stereochemistry-errors", "true");
        }

        [TestMethod]
        public void StandardizationModule_FoldAllHydrogens()
        {
            string result = StandardizationModule.FoldAllHydrogens(Resource1.hairycyclohexane);
            GenericMolecule gm = MoleculeFactory.FromMolV2000(result);
            Assert.AreEqual(6, gm.IndexedAtoms.Count);
        }

        [TestMethod]
        public void StandardizationFailingTest()
        {
            Sdf sdf = new Sdf(Resource1.standardizationTestSdf);
            string glucose = sdf.molecules.First().ct();
            StandardizationResult sr = Standardization.Standardize(glucose, Resources.Vendor.Indigo);
            Assert.IsFalse(sr.Issues.Any(), "no issues should be returned");
        }

        public string Standardization_Test(string ct, string expectedSMILES)
        {
            var result = Standardization.Standardize(ct, Resources.Vendor.Indigo);

            foreach (var s in result.Issues) Console.WriteLine(s.Code +" " +s.Message);

            Console.WriteLine(MoleculeFactory.Molecule(result.Standardized).IndexedAtoms.Where(a => a.Value.Charge != 0).Count() + "charged atoms");
            Assert.AreEqual(expectedSMILES, i.loadMolecule(result.Standardized).canonicalSmiles(), "wrong smiles for " + result);
            return result.Standardized;
        }

        [TestMethod]
        public void Standardization_LayoutCongestedMoleculeTest()
        {
            Molecule congested = MoleculeFactory.Molecule(Resource1.congestedsdf);
            Molecule crossedbonds = MoleculeFactory.Molecule(Resource1.crossedbondsdf);
            Molecule uncongested = Standardization.LayoutStructure(congested, Resources.LayoutOptions.Throw);
            Molecule uncrossed = Standardization.LayoutStructure(crossedbonds, Resources.LayoutOptions.Throw);
            Assert.IsFalse(uncongested.IsCongested());
            Assert.IsFalse(uncrossed.IsCongested());
        }

        /// <summary>
        /// The aim here is to find a tidying mechanism that *actually*works*.
        /// On hold for now. 2012-09-07
        /// </summary>
        //[TestMethod]
        public void Standardization_Halichoblelide_B_Test()
        {
            Molecule halichoblelideB = MoleculeFactory.Molecule(Resource1.halichoblelideB);
            Molecule tidied = Standardization.LayoutStructure(halichoblelideB, Resources.LayoutOptions.Throw);
        }

        // retain at least some of the individual tests

        // SMIRKS-based normalizations

        //[TestMethod]
        public void StandardizeNitrosoTest()
        {
         //   SMIRKSStandardizationTest("[C;H2:1][N:2]=[O:3]>>[C:1]=[N:2][O]",
          //      Resource1._20e_badBenzaldoxime, "T3", "nitroso normalization failed");
        }

        //[TestMethod]
        public void Standardize13EnolTest()
        {
            //SMIRKSStandardizationTest("[C:1]=[C:2]-[O,S,Se,Te;H1:3]>>[C:1][C:2]=[O,S,Se,Te:3]",
              //  Resource1._20b_badAcetophenone, "T1", "1,3-enol normalization failed");
        }

        //[TestMethod]
        public void Standardize13AzaEnolTest()
        {
     //       SMIRKSStandardizationTest("[N:1]=[C:2]-[O,S,Se,Te;H1:3]>>[N:1][C:2]=[O,S,Se,Te:3]",
       //         Resource1._20c_badPhenobarbital, "T2", "1,3-azaenol normalization failed");
        }

        //[TestMethod]
        public void StandardizeNitroTest()
        {
         //   SMIRKSStandardizationTest("[*:1][N:2](=[O:3])=[O:4]>>[*:1][N+:2](=[O:3])[O-:4]",
           //     Resource1._5ai_pentavalentnitro, "5a", "nitro normalization failed");
        }

        // non-SMIRKS-based normalizations
        [TestMethod]
        public void MendThimerosalTest()
        {
            Molecule badthimerosal = MoleculeFactory.Molecule(Resource1._19bii_badthimerosal);
            Assert.AreEqual(badthimerosal.IndexedBonds.Values.Where(b => b.HasElements("C", "Hg")).Count(), 0, "initial molecule contains a mercury--carbon bond");
            Molecule possiblygoodthimerosal = Standardization.MendCarbonMetalSigmaBonds(badthimerosal);
            Assert.AreNotEqual(possiblygoodthimerosal.IndexedBonds.Values.Where(b => b.HasElements("C", "Hg")).Count(), 0, "result contains no mercury--carbon bonds");
        }

        [TestMethod]
        public void Standardization_MendTrimethyltinChloride()
        {
            Molecule badMe3SnCl = MoleculeFactory.Molecule(Resource1._19i_badtrimethyltinchloride);
            Assert.AreEqual(badMe3SnCl.IndexedBonds.Values.Where(b => b.HasElements("C", "Sn")).Count(), 0, "initial molecule contains a tin--carbon bond");
            Molecule fixedMe3SnCl = Standardization.MendCarbonMetalSigmaBonds(badMe3SnCl);
            Assert.AreNotEqual(fixedMe3SnCl.IndexedBonds.Values.Where(b => b.HasElements("C", "Sn")).Count(), 0, "result contains no tin--carbon bonds");
        }

        [TestMethod]
        public void Standardization_PyridineOxide()
        {
            IndigoObject inputObj = i.loadMolecule("O=N1C=CC=CC=1");

            string inputSMILES = inputObj.canonicalSmiles();
            List<string> Transformations = new List<string>();
            var result = Standardization.Standardize(inputObj.molfile(), Resources.Vendor.Indigo);
            Assert.IsNotNull(result, "Standardization result is null");

            IndigoObject outputObj = i.loadMolecule(result.Standardized);
            string outputSMILES = outputObj.canonicalSmiles();

            Assert.AreEqual(outputSMILES, "[O-][N+]1C=CC=CC=1");
        }

        [TestMethod]
        public void Standardization_RemoveInorganicResidues()
        {
            string molInput = Resource1.inorganicresidues;
            
            IndigoObject inputObj = i.loadMolecule(molInput);
            //inputObj.aromatize();
            string inputSMILES = inputObj.canonicalSmiles();

			string result = StandardizationFragmentsModule.removeNeutralInorganicAcidBaseResidues(molInput);
            result = StandardizationFragmentsModule.removeWater(result);
			result = StandardizationFragmentsModule.removeOrganicSolvents(result);
			result = StandardizationFragmentsModule.removeGasMolecules(result);
            IndigoObject outputObj = i.loadMolecule(result);
            //outputObj.aromatize();
            string outputSMILES = outputObj.canonicalSmiles();

            Assert.AreEqual(outputSMILES, "[OH-].[S-2].[SH-].[Cl-].[Br-].[I-].CC.[O-]P([O-])([O-])=O.[O-]S(O)(=O)=O.[O-]N(=O)=O.[O-]P(O)(O)=O");
        }

        [TestMethod]
        public void Standardization_FoldNonStereoHydrogens()
        {
            Func<string, string> fn = s => StandardizationModule.FoldNonStereoHydrogens(s);
            StandardizationTest(fn, Resource1._20ki_tetracycline, "C[C@]1(O)[C@H]2C[C@H]3[C@@H](C(O)=C(C(N)=O)C(=O)[C@@]3(O)C(=O)C2C(=O)C2=C1C=CC=C2O)N(C)C");
            StandardizationTest(fn, Resource1.nonstereohydrogens2, @"Cl.C/C(/O)=C1\C(=O)[C@]2(O)[C@@H](CC3=C(C2=O)C(O)=C2C(C=CC=C2O)=C3C)[C@@H](N)C\1=O");
            StandardizationTest(fn, Resource1.nonstereohydrogens3, @"CC(=O)N[C@@H](C=O)[C@@H](O)[C@H](O)[C@H](O)CO");
            StandardizationTest(fn, Resource1.nonstereohydrogens4, @"[H]/N=C/C1C(OC)O[C@H]([C@@H](O)CO)C=1C=C");
            StandardizationTest(fn, Resource1.nonstereohydrogens5, @"O[C@@H](CO)[C@H]1OC(=O)C(O)=C1O");
        }

        public void StandardizationTest(Func<string, string> fn, string ct, string expectedSMILES)
        {
            string result = fn(ct);
            Assert.AreEqual(expectedSMILES, i.loadMolecule(result).canonicalSmiles(), "wrong smiles for " + result);
        }

        [TestMethod]
        public void Standardization_IronPorphyrin()
        {
            string molInput = Resource1.ironporphyrin;
            IndigoObject inputObj = i.loadMolecule(molInput);
            string inputSMILES = inputObj.canonicalSmiles();

			var result = Standardization.Standardize(molInput, Resources.Vendor.Indigo);
            Assert.IsNotNull(result, "Standardization result is null");

            IndigoObject outputObj = i.loadMolecule(result.Standardized);
            string outputSMILES = outputObj.canonicalSmiles();

            Assert.AreEqual("[Fe+4].CC1C2=CC3[N-]C(=CC4[N-]C(=CC5[N-]C(C=C([N-]2)C=1CCC(O)=O)=C(CCC(O)=O)C=5C)[C@H](C=C)C=4C)/C(=C/O)/C=3C |c:6,10,15,t:2|",
                outputSMILES);
        }

        [TestMethod]
        public void Standardization_CannotCalculateExplicitHydrogen()
        {
            var result = Standardization.Standardize(Resource1.fusedrings, Resources.Vendor.Indigo);
            IndigoObject Obj = i.loadMolecule(result.Standardized);
            Assert.IsTrue(Obj.canonicalSmiles().Equals("ClC1=NC(Br)=NC2N=CNC=21"), "molecule should have smiles ClC1=NC(Br)=NC2N=CNC=21");
        }

        [TestMethod]
        public void Standardization_WrongStereoInversion()
        {
            var codes = Standardization.Standardize(Resource1.stereoinversion, Resources.Vendor.Indigo).Issues;
            foreach (var c in codes) Console.WriteLine(c.Code);
            Standardization_Test(Resource1.stereoinversion, "CC(C)C=C/C=C(/C)\\Cl");
        }

        /// <summary>
        /// "either" bonds should not manifest as a specific regiochemistry in the standardized structure.
        /// </summary>
        [TestMethod]
        public void Standardization_EitherBond()
        {
            Standardization_Test(Resource1.eitherbond_molecule, "NC(=N)NN=NC1C=CN=C2C=CC=CC=12");
        }

        //convert group 1 and 2 metals with carboxylic acid into carboxylate
        // 2015-10-02: commented out until we sort out metal disconnection properly.
        //[TestMethod]
        public void Standardize_CarboxylicAcid2Carboxylate()
        {
            var codes = Standardization.Standardize(Resource1.sodiumcarboxylicacid, Resources.Vendor.Indigo).Issues;
            foreach (var c in codes) Console.WriteLine(c.Code);

            Standardization_Test(Resource1.sodiumcarboxylicacid, "[Na+].CC([O-])=O");
        }

        /// <summary>
        /// Tests that the negative charge is shifted from the benzene ring to the beta position next to the carbonyl.
        /// </summary>
        [TestMethod]
        public void Standardization_AcidBase()
        {
            Standardization_Test(Resource1.acidbase, "[Na+].C[C@@H](C(=O)[CH-]C=C)C1C=CCC=1");
        }

        /// <summary>
        /// Converts double bond adjacent to wavy single bond to crossed double bond
        /// </summary>
        [TestMethod]
        public void Standardization_ConvertDoubleBondWithAttachedEitherSingleBondStereo2EitherDoubleBond()
        {
            Standardization_Test(Resource1.bondType4molecule, "CN(C)CCC=C1C2=CC(Cl)=CC=C2SC2C=CC=CC=21");
        }

        [TestMethod]
        public void Standardization_ConvertDoubleBondWithAttachedEitherSingleBondStereo2EitherDoubleBond_Imine()
        {
            var result = Standardization.Standardize(Resource1.doublebondwithattachedeitherbond, Resources.Vendor.Indigo);
            string inchi = InChINet.InChIUtils.mol2InChI(result.Standardized, InChINet.InChIFlags.Standard);
            Assert.AreEqual("InChI=1S/C23H22ClN3O2.ClH/c24-18-12-10-17(11-13-18)14-26-21-8-4-5-9-22(21)27(23(26)25)15-19(28)16-29-20-6-2-1-3-7-20;/h1-13,19,25,28H,14-16H2;1H", inchi);
        }

        [TestMethod]
        public void Standardize_NonuniqueDearomatization()
        {
            string result = Standardization_Test(Resource1.fusedrings, "ClC1=NC(Br)=NC2N=CNC=21");
            string inchi = InChINet.InChIUtils.mol2InChI(result, InChINet.InChIFlags.Standard);
            Assert.AreEqual(inchi, "InChI=1S/C5H2BrClN4/c6-5-10-3(7)2-4(11-5)9-1-8-2/h1H,(H,8,9,10,11)");
        }

        /// <summary>
        /// Replace Up or Down bonds adjacent to double bonds to non stereo single bonds
        /// </summary>
        [TestMethod]
        public void Standardization_StereoBondAdjacentToDouble()
        {
            string mol = Resource1.CHEMBL50821_stereobondadjacenttodoublebond;
            var result = Standardization.Standardize(mol, Resources.Vendor.Indigo);
            string inchiInput = InChIUtils.mol2InChI(mol, InChIFlags.Standard);
            string stdInChIOutput = InChIUtils.mol2InChI(result.Standardized, InChIFlags.Standard);

            string nonstdInChIInput = InChIUtils.mol2InChI(mol, InChIFlags.CRS);
            string nonstdInChIOutput = InChIUtils.mol2InChI(result.Standardized, InChIFlags.CRS);

            IndigoObject before = i.loadMolecule(mol);
            IndigoObject after = i.loadMolecule(result.Standardized);
            Assert.AreEqual(inchiInput, stdInChIOutput, "module should not affect std inchi");
            Assert.AreEqual(nonstdInChIInput, nonstdInChIOutput, "module should not affect non std inchi");
            Assert.AreEqual(before.canonicalSmiles(), after.canonicalSmiles(), "module should not alter canonical smiles");
        }

        [TestMethod]
        public void Standardization_Tritium()
        {
            string mol = Resource1.tritiatedmolecule;
            var result = Standardization.Standardize(mol, Resources.Vendor.Indigo);
            string nonstdInChIInput = InChIUtils.mol2InChI(mol, InChIFlags.CRS);
            string nonstdInChIOutput = InChIUtils.mol2InChI(result.Standardized, InChIFlags.CRS);
            Assert.AreEqual(nonstdInChIInput, nonstdInChIOutput);
        }

        /// <summary>
        /// Convert Up or Down bonds adjacent to double bonds to non stereo single bonds
        /// CHEMBL12760
        /// </summary>
        [TestMethod]
        public void Standardization_DoubleBondAdjacentToUpBond()
        {
            string mol = Resource1.CHEMBL12760_doublebondadjacenttoupbond;
            string smilesBefore = i.loadMolecule(mol).canonicalSmiles();
            var result = Standardization.Standardize(mol, Resources.Vendor.Indigo);
            string smilesAfter = i.loadMolecule(result.Standardized).canonicalSmiles();
            Assert.AreEqual(smilesBefore, smilesAfter);
        }

        /// <summary>
        /// Not clear what this is testing
        /// TODO: find out
        /// </summary>
        [TestMethod]
        public void Standardization_Mystery2()
        {
            string mol = Resource1.mystery;
            var result = Standardization.Standardize(mol, Resources.Vendor.Indigo);
            string stdInChIOutput = InChINet.InChIUtils.mol2InChI(result.Standardized, InChINet.InChIFlags.Standard);
            Assert.AreEqual("InChI=1S/C99H110N12O13/c1-43-46(4)70-37-88-97(55(13)79(109-88)34-76-51(9)63(21-27-93(116)117)84(106-76)40-82-61(19-25-91(112)113)49(7)73(103-82)31-67(43)100-70)58(16)123-59(17)98-56(14)80-35-77-53(11)65(23-29-95(120)121)86(108-77)42-87-66(54(12)75(105-87)33-69-44(2)47(5)71(101-69)38-89(98)110-80)24-30-96(122)124-60(18)99-57(15)81-36-78-52(10)64(22-28-94(118)119)85(107-78)41-83-62(20-26-92(114)115)50(8)74(104-83)32-68-45(3)48(6)72(102-68)39-90(99)111-81/h31-36,40-42,58-60,70-72,103-105,109-111H,19-30,37-39H2,1-18H3,(H,112,113)(H,114,115)(H,116,117)(H,118,119)(H,120,121)/b73-31-,74-32-,75-33-,76-34-,77-35-,78-36-,82-40-,83-41-,87-42-",
                stdInChIOutput);
        }

        [TestMethod]
        public void Standardization_Ammonia()
        {
            Indigo i = new Indigo();
            Standardization_Test(i.loadMolecule("N.Cl").molfile(), "[NH4+].[Cl-]");
            Standardization_Test(i.loadMolecule("N.C(=O)O").molfile(), "[NH4+].[O-]C=O");
            Standardization_Test(i.loadMolecule("C[NH+](C)C.[Cl-]").molfile(), "Cl.CN(C)C");
            Standardization_Test(i.loadMolecule("[NH+]=C.OS(=O)(=O)[O-]").molfile(), "C=N.OS(O)(=O)=O");
        }

        [TestMethod]
        public void StandardizationModules_Ammonia()
        {
            Func<string, string> fn = s => StandardizationModule.TreatAmmonia(s);
            StandardizationTest(fn, i.loadMolecule("N.Cl").molfile(), "[NH4+].[Cl-]");
            StandardizationTest(fn, i.loadMolecule("N.C(=O)O").molfile(), "[NH4+].[O-]C=O");
            StandardizationTest(fn, i.loadMolecule("C[NH+](C)C.[Cl-]").molfile(), "Cl.CN(C)C");
            StandardizationTest(fn, i.loadMolecule("[NH+]=C.OS(=O)(=O)[O-]").molfile(), "C=N.OS(O)(=O)=O");
        }

        /// <summary>
        /// Old comment reads "Crashes application".
        /// </summary>
        [TestMethod]
        public void Standardization_InorganicResiduesInChIPreserved()
        {
            string mol = Resource1.inorganicresidues2;
			var result = Standardization.Standardize(mol, Resources.Vendor.Indigo);

            string nonstdInChIInput = InChIUtils.mol2InChI(mol, InChIFlags.CRS);
            string nonstdInChIOutput = InChIUtils.mol2InChI(result.Standardized, InChIFlags.CRS);

            Assert.AreEqual("InChI=1/C2H6.CH4.2BrH.2ClH.2HI.HNO3.NO3.H3N.4H3O4P.3H2O4S.2H2O.2H2S.S/c1-2;;;;;;;;2*2-1(3)4;;7*1-5(2,3)4;;;;;/h1-2H3;1H4;6*1H;(H,2,3,4);;1H3;4*(H3,1,2,3,4);3*(H2,1,2,3,4);4*1H2;/q;;;;;;;;;-1;;;;;;;;;;;;;-2/p-14/fC2H6.CH4.BrH.Br.2Cl.HI.I.HNO3.NO3.H4N.H3O4P.H2O4P.HO4P.O4P.H2O4S.HO4S.O4S.H2O.HO.H2S.HS.S/h;;;3*1h;;1h;2H;;1H;1-3H;1-2H;1H;;1-2H;1H;;;1h;;1h;/q;;;3*-1;;-1;;m;+1;;-1;-2;-3;;-1;-2;;-1;;-1;m",
                nonstdInChIOutput, "module should not affect non-std inchi");
        }

        [TestMethod]
        public void Standardization_Kekulize()
        {
            Standardization_Test(i.loadMolecule("c1ccccc1").molfile(), "C1C=CC=CC=1");
        }

        [TestMethod]
        public void StandardizationModule_Kekulize()
        {
            string result = StandardizationModule.Kekulize(i.loadMolecule("c1ccccc1").molfile());
            string smilesResult = i.loadMolecule(result).canonicalSmiles();
            Assert.AreEqual("C1C=CC=CC=1", smilesResult);
        }

        [TestMethod]
        public void StandardizationModule_Aromatize()
        {
            string result = StandardizationModule.Aromatize(i.loadMolecule("C1C=CC=CC=1").molfile());
            string smilesResult = i.loadMolecule(result).canonicalSmiles();
            Assert.AreEqual("c1ccccc1", smilesResult);
        }

        [TestMethod]
        public void StandardizationModule_ShouldRunTautomerCanonicalizer()
        {
            string benzene = i.loadMolecule("c1ccccc1").molfile();
            Assert.IsFalse(StandardizationModule.ShouldRunTautomerCanonicalizer(benzene, 5));
            Assert.IsTrue(StandardizationModule.ShouldRunTautomerCanonicalizer(benzene, 100));
        }

        [TestMethod]
        public void StandardizationModule_ReplaceIsotopes()
        {
            var benzene = i.loadMolecule("c1ccccc1");
            var flags = InChIFlags.CRS;
            string inchi, inchiKey;
            Assert.AreEqual(benzene.canonicalSmiles(),
                i.loadMolecule(StandardizationModule.ReplaceIsotopes(benzene.molfile(), out inchi, out inchiKey, flags))
                    .canonicalSmiles());
            var deuterobenzene = i.loadMolecule("[2H]c1ccccc1");
            Assert.AreEqual(benzene.canonicalSmiles(),
                i.loadMolecule(StandardizationModule.ReplaceIsotopes(deuterobenzene.molfile(), out inchi, out inchiKey, flags))
                    .canonicalSmiles());
            var thirteenbenzene = i.loadMolecule("c1ccc[13c]c1");
            Assert.AreEqual(benzene.canonicalSmiles(),
                i.loadMolecule(StandardizationModule.ReplaceIsotopes(thirteenbenzene.molfile(), out inchi, out inchiKey, flags))
                    .canonicalSmiles());
        }

        /// <summary>
        /// basically checks that it doesn't go mad or fall over. Seems a bit mysterious to be honest.
        /// </summary>
        [TestMethod]
        public void StandardizationModule_StandardizeByInChIRules()
        {
            string result = StandardizationModule.StandardizeByInChIRules(i.loadMolecule("c1ccccc1C(=O)O").molfile());
            string smilesResult = i.loadMolecule(result).canonicalSmiles();
            Assert.AreEqual("OC(=O)C1C=CC=CC=1", smilesResult);
        }

        [TestMethod]
        public void StandardizationModule_StandardizeByInChIRulesFoldAllHydrogens()
        {
            string input = i.loadMolecule("C[C@@H](O)N").molfile();
            string result = StandardizationModule.StandardizeByInChIRules(input);
            string smilesResult = i.loadMolecule(result).canonicalSmiles();
            Assert.AreEqual("CC(N)O", smilesResult);
        }

        [TestMethod]
        public void StandardizationModule_ApplyReactions()
        {
            string input = i.loadMolecule("CCO").molfile();
            List<string> rxns = new List<string>() { "[*:1][O:2]>>[*:1][Cl:2]",
                "[*:1][Cl:2]>>[*:1][Br:2]" };
            Assert.AreEqual("CCBr", i.loadMolecule(StandardizationModule.ApplyReactions(input, rxns)).canonicalSmiles());
        }
    }
}

