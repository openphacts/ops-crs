using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSC.CVSP.Compounds;
using com.ggasoftware.indigo;
using InChINet;

namespace CVSPTests
{
    [TestClass]
    public class StandardizationParents : CVSPTestBase
    {
        [TestMethod]
        public void StandardizationParents_chargeUnsensParent_NMePlus()
        {
            Indigo i = new Indigo();
            //remove methyl
            IndigoObject obj1 = i.loadMolecule("C[N+]1(C)CCC2=CC(OC)=C(OC)C3OC4C=CC(CC5C6=CC(OC7=CC(CC1C2=3)=CC=C7OC)=C(C=C6CC[N+]5(C)C)OC)=CC=4");
            obj1.layout();
            obj1.markEitherCisTrans();
			string new_inchi = String.Empty, new_inchi_key = String.Empty;
            string output_mol = StandardizationChargesModule.NeutralizeCharges(obj1.molfile(), out new_inchi, out new_inchi_key,InChIFlags.CRS);
            IndigoObject output = i.loadMolecule(output_mol);
            Assert.AreEqual("CN1CCC2=CC(OC)=C(OC)C3OC4C=CC(CC5C6=CC(OC7=CC(CC1C2=3)=CC=C7OC)=C(C=C6CCN5C)OC)=CC=4", output.canonicalSmiles(), "Problem with removing methyl from quaternary nitrogen");
        }

        [TestMethod]
        public void StandardizationParents_chargeUnsensParent_NEtPlus()
        {
            Indigo i = new Indigo();
            
            //remove ethyl
            IndigoObject obj2 = i.loadMolecule("CC[N+](CC)(CC)CCC(O)(C1CCCCC1)C1=CC=CC=C1");
            obj2.layout();
            obj2.markEitherCisTrans();
			string new_inchi = String.Empty, new_inchi_key = String.Empty;
			string output_mol = StandardizationChargesModule.NeutralizeCharges(obj2.molfile(), out new_inchi, out new_inchi_key, InChIFlags.CRS);
            IndigoObject output = i.loadMolecule(output_mol);
            Assert.AreEqual("CCN(CCC(O)(C1CCCCC1)C1C=CC=CC=1)CC", output.canonicalSmiles(), "Problem with removing ethyl from quaternary nitrogen");
        }

        [TestMethod]
        public void StandardizationParents_chargeUnsensParent_NHMinus()
        {
            Indigo i = new Indigo();
            IndigoObject obj = i.loadMolecule("[NH-][C@@H]1CCCC[C@H]1[NH-]");
            obj.layout();
            obj.markEitherCisTrans();
			string new_inchi = String.Empty, new_inchi_key = String.Empty;
			string output_mol = StandardizationChargesModule.NeutralizeCharges(obj.molfile(), out new_inchi, out new_inchi_key, InChIFlags.CRS);
            IndigoObject output = i.loadMolecule(output_mol);
            Assert.AreEqual("N[C@@H]1CCCC[C@H]1N", output.canonicalSmiles(), "Problem with neutralizing NH-");
        }

        [TestMethod]
        public void StandardizationParents_StereoRemovalOnDoubleBonds()
        {
            Indigo i = new Indigo();
            i.setOption("ignore-stereochemistry-errors", "true");
            IndigoInchi i_inchi = new IndigoInchi(i);
            i.setOption("inchi-options", "/SUU /SLUUD /FixedH /SUCF");
            IndigoObject obj1 = i.loadMolecule(@"C[C@H](CCCC(C)(C)c1ccccc1)[C@H]2CC[C@@H]\3[C@@]2(CCC/C3=C\C=C/4\C[C@H](C[C@@H](C4=C)O)O)C");
            obj1.layout();
            string inchi_input = i_inchi.getInchi(obj1);
			string inchi, inchi_key;
			string output_mol = StandardizationModule.RemoveStereo(obj1.molfile(), out inchi, out inchi_key, InChIFlags.CRS);
            IndigoObject output = i.loadMolecule(output_mol);
            string inchi_output = i_inchi.getInchi(output);
            Assert.AreEqual(inchi_output, "InChI=1/C33H48O2/c1-23(11-9-19-32(3,4)27-13-7-6-8-14-27)29-17-18-30-25(12-10-20-33(29,30)5)15-16-26-21-28(34)22-31(35)24(26)2/h6-8,13-16,23,28-31,34-35H,2,9-12,17-22H2,1,3-5H3/b25-15u,26-16u/t23?,28?,29?,30?,31?,33?");
        }

        [TestMethod]
        public void StandardizationParents_StereoRemoval()
        {
 			string inchi, inchi_key;
			string outmol = StandardizationModule.RemoveStereo(Resource1.stereoIsotopeChargeFragmentMolecule, out inchi, out inchi_key, InChIFlags.CRS);
            string non_std_inchi = InChIUtils.mol2InChI(outmol, InChIFlags.CRS);
            Assert.AreEqual(non_std_inchi, "InChI=1/C9H12O4S.Na/c1-5(9(11)12)8-4-6(10)7(13-8)2-3-14;/h2-5,7-8,10,14H,1H3,(H,11,12);/q;+1/p-1/b3-2u;/t5?,7?,8?;/i13+2;/fC9H11O4S.Na/q-1;m");
        }
    }
}
