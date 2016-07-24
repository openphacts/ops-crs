using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

using RSC.CVSP.Compounds;
using InChINet;
using com.ggasoftware.indigo;
using RSC.CVSP;
using RSC;
using RSC.Logging;

namespace CVSPTests
{
	[TestClass]
	public class StandardizationStereo : CVSPTestBase
	{
        static Indigo i;
        [ClassInitialize]
        public static void Initialize(TestContext tc)
        {
            i = new Indigo();
            i.setOption("ignore-stereochemistry-errors", "true");
        }

        [TestMethod]
        public void StandardizationStereo_ClearChiralFlag()
        {
            string fakeChiral = StandardizationStereoModule.AddChiralFlag(Resource1.flatsugar);
            Assert.AreEqual(Resource1.flatsugar, StandardizationStereoModule.ClearChiralFlagOnFlatStructure(fakeChiral));
            string reallyChiral = Resource1.ringstereo_present;
            Assert.AreEqual(Resource1.ringstereo_present, StandardizationStereoModule.ClearChiralFlagOnFlatStructure(reallyChiral));
        }

        [TestMethod]
        public void StandardizationStereo_RemoveChiralFlag()
        {
            string fakeChiral = StandardizationStereoModule.AddChiralFlag(Resource1.flatsugar);
            Assert.AreEqual(Resource1.flatsugar, StandardizationStereoModule.RemoveChiralFlag(fakeChiral));
        }

		[TestMethod]
		public void StandardizationStereo_ConvertV2000RelativeStereoToAbsolute()
		{
            string sdf = Resource1.v2000relativestereo;
			string inchi = InChIUtils.mol2InChI(sdf, InChIFlags.CRS);
			string result = StandardizationStereoModule.AddChiralFlag(sdf);
			string inchiOut = InChIUtils.mol2InChI(result, InChIFlags.CRS);
			Assert.AreNotEqual(inchi, inchiOut, "inchis should be different");
            Assert.AreEqual("InChI=1/C17H19FN4O4/c1-8(23)15(19)16-21-12(17(26)22(16)7-13(24)25)5-9-6-20-11-4-2-3-10(18)14(9)11/h2-4,6,8,15,20,23,26H,5,7,19H2,1H3,(H,24,25)/t8-,15-/m1/s1/f/h24H",
                inchiOut);
		}

        [TestMethod]
        public void StandardizationStereo_RemoveSP3Stereo()
        {
            string result = StandardizationStereoModule.RemoveSP3Stereo(i.loadMolecule("F[C@@H](Br)Cl").molfile());
            Assert.AreEqual("FC(Cl)Br", i.loadMolecule(result).canonicalSmiles());
        }        

        [TestMethod]
        public void StandardizationStereo_ResetSymmetricStereoCenters()
        {
            string result = StandardizationStereoModule.ResetSymmetricStereoCenters(i.loadMolecule("CC[C@@H](CN)CC").molfile());
            Assert.AreEqual("CCC(CN)CC", i.loadMolecule(result).canonicalSmiles());
        }

        [TestMethod]
        public void StandardizationStereo_ResetSymmetricCisTrans()
        {
            string input = i.loadMolecule("C/C(C)=C(N)/N").molfile();
            string result = StandardizationStereoModule.ResetSymmetricCisTrans(input);
            Assert.AreEqual("CC(C)=C(N)N", i.loadMolecule(result).canonicalSmiles());
        }

		[TestMethod]
        public void StandardizationStereo_AmbiguousStereoOnH()
		{
            string mol = Resource1.ambiguousSp3Stereo;
			string result = StandardizationModule.RemoveAmbiguousSp3Stereo(mol);
			string smiles = i.loadMolecule(result).smiles();
            Assert.AreEqual("OC(C)(C)[C@@H](C)/C=C/[C@@H](C)[C@@]1([H])CC[C@@]2([H])/C(=C/C=C3/C[C@H](C[C@@H](C/3)O)O)/C([H])CC[C@]12C", smiles);
		}

		[TestMethod]
		public void StandardizationStereo_FlatSugar()
		{
			var result = Standardization.Standardize(Resource1.flatsugar, Resources.Vendor.Indigo);
			string std_smiles = i.loadMolecule(result.Standardized).smiles();
			Assert.AreEqual("[C@@H]1(O)[C@@H](CO)O[C@@H](O)[C@@H](O)[C@@H]1O", std_smiles);
		}

        [TestMethod]
        public void StandardizationStereo_RemoveAlleneStereo()
        {
            // the input molfile here has two up and two down bonds
            string result = StandardizationStereoModule.RemoveAlleneStereo(Resource1.badallene);
            Assert.AreEqual("CC(I)=C=C(O)Br", i.loadMolecule(result).canonicalSmiles());
        }

        [TestMethod]
        public void StandardizationStereo_ConvertDoubleBondsToEither()
        {
            string result = StandardizationStereoModule.ConvertDoubleBondsToEither(Resource1.stereobondatdoublebond);
            // Am doing this with the InChI because the current method we are testing explicitly involves
            // munging the SMILES.
            string inchiOut = InChIUtils.mol2InChI(result, InChIFlags.CRS);
            Assert.IsFalse(inchiOut.Contains("/b12-10+,14-11-"), "resulting InChI should have no stereo in double bond layer " + inchiOut);
        }

        [TestMethod]
        public void StandardizationStereo_ConvertEitherBondsToDefined()
        {
            string result = StandardizationStereoModule.ConvertEitherBondsToDefined(Resource1.cistranseither);
            Assert.IsFalse(result.Contains("  1  2  2  3"), "result should not contain an 'either' double bond");
            Assert.IsTrue(result.Contains("  1  2  2  0"), "result should contain a 'defined' double bond");
        }


        [TestMethod]
        public void StandardizationStereo_FixStereoBondNextToDouble()
        {
            string input = i.loadMolecule("C[C@@H](F)=[C@@H](F)Cl").molfile();
            string result = StandardizationStereoModule.ConvertDoubleBondWithAttachedEitherSingleBondStereoToEitherDoubleBond(input);
            Assert.AreEqual("C[CH](F)=[CH](F)Cl", i.loadMolecule(result).canonicalSmiles());
        }

        [TestMethod]
        public void StandardizationStereo_FixStereoBondNextToDoubleNextToStereo()
        {
            string input = i.loadMolecule("C[C@@H](F)=[C@@H](F)Cl").molfile();
            string result = StandardizationStereoModule.ConvertUpOrDownBondsAdjacentToDoubleBondToNoStereoSingleBondsWithCrossedDoubleBond(input);
            Console.WriteLine(result);
            Assert.AreEqual("C[CH](F)=[CH](F)Cl", i.loadMolecule(result).canonicalSmiles());
        }

        /// <summary>
        /// the notion in this one is that the SMILES should be preserved.
        /// But genuinely I'm a bit baffled as to what's going on with this.
        /// </summary>
        [TestMethod]
        public void StandardizationStereo_StrippableH()
        {
            var inputObj = i.loadMolecule(Resource1.multicomponentStrippableH);
            string result = Standardization.Standardize(Resource1.multicomponentStrippableH, Resources.Vendor.Indigo).Standardized;
            string smilesIn = i.loadMolecule(Resource1.multicomponentStrippableH).canonicalSmiles();
            var resultObj = i.loadMolecule(result);
            string smilesOut = resultObj.canonicalSmiles();
            Assert.AreEqual(smilesIn, smilesOut);
        }
	}
}
