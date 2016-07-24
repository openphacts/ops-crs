using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoleculeObjects;
using RSC.CVSP.Compounds;
using com.ggasoftware.indigo;
using InChINet;

namespace CVSPTests
{
    [TestClass]
    public class StandardizationMetals : CVSPTestBase
    {
        static Indigo i;
        static IndigoInchi i_inchi;

        [ClassInitialize]
        public static void Initialize(TestContext tc)
        {
            i = new Indigo();
            i_inchi = new IndigoInchi(i);
            i.setOption("inchi-options", "/SUU /SLUUD /FixedH /SUCF");
        }

        /// <summary>
        /// Asserts that there should be no bond in molfile connecting element1 to element2.
        /// </summary>
        /// <param name="molfile"></param>
        /// <param name="element1">Element symbol.</param>
        /// <param name="element2">Element symbol.</param>
        public void TestBondAbsent(string molfile, string element1, string element2)
        {
            GenericMolecule gm = MoleculeFactory.FromMolV2000(molfile);
            Assert.IsFalse(gm.IndexedBonds.Values.Any(b => b.HasElements(element1, element2)),
                String.Format("should be no {0}-{1} bonds", element1, element2));
        }
        
        /// <summary>
        /// Asserts that there should be at least one bond in molfile connecting element1 to element2.
        /// </summary>
        /// <param name="molfile"></param>
        /// <param name="element1">Element symbol.</param>
        /// <param name="element2">Element symbol.</param>
        public void TestBondPresent(string molfile, string element1, string element2)
        {
            GenericMolecule gm = MoleculeFactory.FromMolV2000(molfile);
            Assert.IsTrue(gm.IndexedBonds.Values.Any(b => b.HasElements(element1, element2)),
                String.Format("should be at least one {0}-{1} bond", element1, element2));
        }

        [TestMethod]
        public void StandardizationMetals_NeutralizeFreeMetals()
        {
            string input = i.loadMolecule("[Na+].[Cr+3].[K+].[I-]").molfile();
            string result = StandardizationMetalsModule.NeutralizeFreeMetals(input);
            string resultSMILES = i.loadMolecule(result).canonicalSmiles();
            Assert.AreEqual("[NaH].[KH].[Cr].[I-]", resultSMILES, "wrong SMILES result");
        }

        [TestMethod]
        public void StandardizationMetals_disconnectMetalFromCarboxylate()
        {
            string new_mol = StandardizationMetalsModule.DisconnectMetalsInCarboxylates(Resource1.PtCarboxylateDisconnection, true);
            TestBondAbsent(new_mol, "Pt", "O");
        }

        [TestMethod]
        public void StandardizationMetals_disconnectMetalsFrom_N_O_F()
        {
			string new_mol = StandardizationMetalsModule.DisconnectMetalsFromNOF(Resource1.PtCarboxylateDisconnection, true);
            TestBondAbsent(new_mol, "Pt", "N");
        }

        /// <summary>
        /// disconnects metals (excluding Hg, Ga, Ge, In, Sn, As, Tl, Pb, Bi, Po) from non metals (exluding N,O, and F)
        /// </summary>
        [TestMethod]
        public void StandardizationMetals_disconnectMetalsFromNonMetals_1_Exc_NOF()
        {
			string new_mol = StandardizationMetalsModule.DisconnectMetalsFromNonMetals(Encoding.UTF8.GetString(Resource1.metalNonMetalBond), true);
            TestBondAbsent(new_mol, "Al", "S");
        }

        /// <summary>
        /// disconnects metals (excluding Hg, Ga, Ge, In, Sn, As, Tl, Pb, Bi, Po) from non metals (excluding N,O, and F)
        /// Not sure what this is testing - that no non-metals get disconnected?
        /// </summary>
        [TestMethod]
        public void StandardizationMetals_disconnectMetalsFromNonMetals_2()
        {
            string mol = Encoding.UTF8.GetString(Resource1.azabicycloheptene);
			string new_mol = StandardizationMetalsModule.DisconnectMetalsFromNonMetals(mol, true);
            string inchi = InChIUtils.mol2InChI(new_mol,InChIFlags.Standard);
            Assert.IsTrue(inchi.Equals("InChI=1S/C17H24N2O7S/c1-6-10-9(7(2)21)15(22)19(10)11(17(24)25)13(6)27-14-8(5-20)26-12(14)16(23)18(3)4/h6-10,12,14,20-21H,5H2,1-4H3,(H,24,25)/p-1/t6-,7-,8-,9-,10?,12+,14-/m1/s1"));
        }

        /// <summary>
        /// validate that Pt-O-H is not getting disconnected to Pt and OH
        /// </summary>
        [TestMethod]
        public void StandardizationMetals_disconnectMetalsFromNonMetals_2_Exc_NOF()
        {
            string new_mol = StandardizationMetalsModule.DisconnectMetalsFromNOF(Resource1.octahedralPtComplex, true);
            TestBondPresent(new_mol, "Pt", "O");
        }

        /// <summary>
        /// Validate correct behaviour around gold atom.
        /// Note (2015-10-02): the code actually does the wrong thing with Au=P bonds.
        /// </summary>
        [TestMethod]
        public void StandardizationMetals_disconnectMetalsFromNonMetals_3_Exc_NOF()
        {
            string input = Encoding.UTF8.GetString(Resource1.triplybondedgold);
			string new_mol = StandardizationMetalsModule.DisconnectMetalsFromNonMetals(input, true);
            IndigoObject obj = i.loadMolecule(new_mol);
            //bool res = false;
            foreach (IndigoObject atom in obj.iterateAtoms())
            {
                Assert.IsFalse(atom.symbol() == "Au" && atom.charge() != 3, "Charge on Au should be +3");
                Assert.IsFalse(atom.symbol() == "P" && atom.charge() != -2, "Charge on P should be -2");
                Assert.IsFalse(atom.symbol() == "S" && atom.charge() != -1, "Charge on S should be -1");
            }
            Assert.AreEqual("[Au+3].CC(=O)O[C@H]1[C@H]([S-])O[C@H](COC(C)=O)[C@@H](OC(C)=O)[C@@H]1OC(C)=O.CC[P-2](CC)CC",
                obj.canonicalSmiles());
        }
        
        /// <summary>
        /// Do not ionize metal (increment charge) with carboxylic acid (decrement charge) when >1 metal or carboxylic group
        /// </summary>
        [TestMethod]
        public void StandardizationMetals_ionize_freeMetal_With_TWoCarboxylicAcid()
        {
			string new_mol = StandardizationMetalsModule.IonizeFreeMetalWithCarboxylicAcid(Resource1.freeMetal_And_CarboxylicAcid, true);
            IndigoObject obj = i.loadMolecule(new_mol);
            Assert.AreEqual(obj.canonicalSmiles(), "[NaH].OC(=O)C1C=C(C=CC=1)C(O)=O");
        }

        /// <summary>
        /// ionized metal (increment charge) with carboxylic acid (decrement charge) when 1 metal and 1 carboxylic group
        /// </summary>
        [TestMethod]
        public void StandardizationMetals_ionize_1_freeMetal_With_1_CarboxylicAcid()
        {
            string mol = @"
  Ketcher 09261313052D 1   1.00000     0.00000     0

 13 12  0     0  0            999 V2000
    3.2763   -4.7416    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    4.6290   -4.7412    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    3.9539   -4.3506    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    4.6290   -5.5238    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    3.2763   -5.5273    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    3.9556   -5.9142    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.6713   -3.9740    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.3815   -2.5459    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    6.8511   -3.9740    0.0000 Cl  0  0  0  0  0  0  0  0  0  0  0  0
    3.8912   -6.8097    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    4.6469   -7.0121    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    2.6701   -7.9895    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    9.4591   -6.0853    0.0000 Na  0  0  0  0  0  0  0  0  0  0  0  0
  6  4  1  0     0  0
  5  6  2  0     0  0
  2  3  1  0     0  0
  1  5  1  0     0  0
  4  2  2  0     0  0
  3  1  2  0     0  0
  2  7  1  0     0  0
  7  8  2  0     0  0
  7  9  1  0     0  0
  6 10  1  0     0  0
 10 11  1  0     0  0
 10 12  2  0     0  0
M  END
";
            //MetalTreatment mt = new MetalTreatment();
			string new_mol = StandardizationMetalsModule.IonizeFreeMetalWithCarboxylicAcid(mol, true);

            Assert.AreEqual(InChIUtils.mol2InChI(new_mol,InChIFlags.Standard), "InChI=1S/C8H5ClO3.Na/c9-7(10)5-2-1-3-6(4-5)8(11)12;/h1-4H,(H,11,12);/q;+1/p-1");
        }

        [TestMethod]
        public void StandardizationMetals_platinumComplexTest()
        {
            string molfile = @"NSC 256927
  -INDIGO-02141313582D

 27 26  0  0  0  0  0  0  0  0999 V2000
    5.4640    0.5000    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    4.5980    0.0000    0.0000 Pt  0  0  0  0  0  0  0  0  0  0  0  0
    6.3300    0.0000    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    3.7320   -0.5000    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    4.5980   -1.0000    0.0000 Cl  0  0  0  0  0  0  0  0  0  0  0  0
    3.7320    0.5000    0.0000 Cl  0  0  0  0  0  0  0  0  0  0  0  0
    4.5980    1.0000    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    5.4640   -0.5000    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    7.1960    0.5000    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    6.3300   -1.0000    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    2.8660    0.0000    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    2.0000   -0.5000    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    2.8660    1.0000    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    6.3300    0.6200    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
    7.5060   -0.0370    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
    7.7330    0.8100    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
    6.8860    1.0370    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
    5.7100   -1.0000    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
    6.3300   -1.6200    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
    6.9500   -1.0000    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
    2.8660   -0.6200    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
    1.6900    0.0370    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
    1.4630   -0.8100    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
    2.3100   -1.0370    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
    3.4860    1.0000    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
    2.8660    1.6200    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
    2.2460    1.0000    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
  1  2  1  0  0  0  0
  1  3  1  0  0  0  0
  2  4  1  0  0  0  0
  2  5  1  0  0  0  0
  2  6  1  0  0  0  0
  2  7  1  0  0  0  0
  2  8  1  0  0  0  0
  3  9  1  0  0  0  0
  3 10  1  0  0  0  0
  3 14  1  0  0  0  0
  4 11  1  0  0  0  0
  9 15  1  0  0  0  0
  9 16  1  0  0  0  0
  9 17  1  0  0  0  0
 10 18  1  0  0  0  0
 10 19  1  0  0  0  0
 10 20  1  0  0  0  0
 11 12  1  0  0  0  0
 11 13  1  0  0  0  0
 11 21  1  0  0  0  0
 12 22  1  0  0  0  0
 12 23  1  0  0  0  0
 12 24  1  0  0  0  0
 13 25  1  0  0  0  0
 13 26  1  0  0  0  0
 13 27  1  0  0  0  0
M  END";
            Indigo i = new Indigo();
            IndigoInchi i_inchi = new IndigoInchi(i);
            i.setOption("inchi-options", "/SUU /SLUUD /FixedH /SUCF");
            IndigoObject input_obj = i.loadMolecule(molfile);
            string non_std_input_inchi = i_inchi.getInchi(input_obj);
            //MetalTreatment mt = new MetalTreatment();
			string output_molfile = StandardizationMetalsModule.DisconnectMetalsFromNOF(molfile, true);

            IndigoObject output_obj = i.loadMolecule(output_molfile);
            string non_std_output_inchi = i_inchi.getInchi(output_obj);
            Assert.AreEqual(non_std_output_inchi, "InChI=1/2C3H8N.2ClH.2H2O.Pt/c2*1-3(2)4;;;;;/h2*3-4H,1-2H3;2*1H;2*1H2;/q2*-1;;;;;+6/p-4/f2C3H8N.2Cl.2HO.Pt/h;;4*1h;/q2m;4*-1;m");

        }

        [TestMethod]
        public void StandardizationMetals_removeFreeMetalCations_Test()
        {
            string molfile = @"
  -INDIGO-02191318172D

 21 20  0  0  0  0  0  0  0  0999 V2000
   10.0727   -6.7891    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.6301   -7.6220    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    9.3514   -6.3726    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    8.6301   -6.7891    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.1875   -7.6220    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.9088   -6.3727    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.1875   -6.7891    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    6.4662   -5.5398    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    6.4662   -6.3727    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.7449   -6.7891    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.0236   -6.3727    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.0236   -5.5398    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.7449   -5.1233    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.7449   -3.9422    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    4.7220   -3.3517    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    4.7220   -2.1706    0.0000 S   0  0  0  0  0  0  0  0  0  0  0  0
    3.6992   -1.5800    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    5.7449   -1.5800    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    3.6992   -2.7611    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    4.0007   -6.9632    0.0000 S   0  0  0  0  0  0  0  0  0  0  0  0
    3.4063   -9.2188    0.0000 Na  0  0  0  0  0  0  0  0  0  0  0  0
  3  1  1  0  0  0  0
  4  2  2  0  0  0  0
  4  3  1  0  0  0  0
  6  4  1  0  0  0  0
  7  5  1  0  0  0  0
  7  6  1  0  0  0  0
  9  7  1  0  0  0  0
  9  8  1  0  0  0  0
 10  9  2  0  0  0  0
 11 10  1  0  0  0  0
 12 11  2  0  0  0  0
 13  8  2  0  0  0  0
 13 12  1  0  0  0  0
 13 14  1  0  0  0  0
 14 15  1  0  0  0  0
 15 16  1  0  0  0  0
 16 17  1  0  0  0  0
 16 18  2  0  0  0  0
 16 19  2  0  0  0  0
 11 20  1  0  0  0  0
M  CHG  2  20  -1  21   1
M  END";
            IndigoInchi i_inchi = new IndigoInchi(i);
            i.setOption("inchi-options", "/SUU /SLUUD /FixedH /SUCF");
            IndigoObject input_obj = i.loadMolecule(molfile);
            string non_std_input_inchi = i_inchi.getInchi(input_obj);
			string output_molfile = StandardizationMetalsModule.RemoveFreeMetalCations(molfile);

            IndigoObject output_obj = i.loadMolecule(output_molfile);
            string non_std_output_inchi = i_inchi.getInchi(output_obj);
            Assert.AreEqual(non_std_output_inchi, "InChI=1/C12H16O6S2/c1-8(3-12(13)17-2)10-4-9(5-11(19)6-10)7-18-20(14,15)16/h4-6,8,19H,3,7H2,1-2H3,(H,14,15,16)/p-1/t8?/fC12H15O6S2/h19h,14H/q-1");
        }

        [TestMethod]
        public void StandardizationMetals_removeFreeMetals_Test()
        {
            string molfile = @"
  -INDIGO-02191318172D

 21 20  0  0  0  0  0  0  0  0999 V2000
   10.0727   -6.7891    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.6301   -7.6220    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    9.3514   -6.3726    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    8.6301   -6.7891    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.1875   -7.6220    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.9088   -6.3727    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.1875   -6.7891    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    6.4662   -5.5398    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    6.4662   -6.3727    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.7449   -6.7891    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.0236   -6.3727    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.0236   -5.5398    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.7449   -5.1233    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.7449   -3.9422    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    4.7220   -3.3517    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    4.7220   -2.1706    0.0000 S   0  0  0  0  0  0  0  0  0  0  0  0
    3.6992   -1.5800    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    5.7449   -1.5800    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    3.6992   -2.7611    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    4.0007   -6.9632    0.0000 S   0  0  0  0  0  0  0  0  0  0  0  0
    3.4063   -9.2188    0.0000 Na  0  0  0  0  0  0  0  0  0  0  0  0
  3  1  1  0  0  0  0
  4  2  2  0  0  0  0
  4  3  1  0  0  0  0
  6  4  1  0  0  0  0
  7  5  1  0  0  0  0
  7  6  1  0  0  0  0
  9  7  1  0  0  0  0
  9  8  1  0  0  0  0
 10  9  2  0  0  0  0
 11 10  1  0  0  0  0
 12 11  2  0  0  0  0
 13  8  2  0  0  0  0
 13 12  1  0  0  0  0
 13 14  1  0  0  0  0
 14 15  1  0  0  0  0
 15 16  1  0  0  0  0
 16 17  1  0  0  0  0
 16 18  2  0  0  0  0
 16 19  2  0  0  0  0
 11 20  1  0  0  0  0
M  CHG  2  20  -1  21   1
M  END";
            IndigoObject input_obj = i.loadMolecule(molfile);
            string non_std_input_inchi = i_inchi.getInchi(input_obj);
			string output_molfile = StandardizationMetalsModule.RemoveFreeMetals(molfile);

            IndigoObject output_obj = i.loadMolecule(output_molfile);
            string non_std_output_inchi = i_inchi.getInchi(output_obj);
            Assert.AreEqual(non_std_output_inchi, "InChI=1/C12H16O6S2/c1-8(3-12(13)17-2)10-4-9(5-11(19)6-10)7-18-20(14,15)16/h4-6,8,19H,3,7H2,1-2H3,(H,14,15,16)/p-1/t8?/fC12H15O6S2/h19h,14H/q-1");

        }
    }
}
