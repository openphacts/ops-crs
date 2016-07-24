using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using ChemSpider.Molecules;
using RSC.CVSP.Compounds;
using InChINet;
using com.ggasoftware.indigo;

using MoleculeObjects;

namespace CVSPTests
{
	[TestClass]
	public class StandardizationHexagons : CVSPTestBase
	{
        [TestMethod]
        public void Hexagon_ChairsAtAllAnglesTest()
        {
            Molecule m = MoleculeFactory.Molecule(Resource1.chairsAtAllAngles);
            Molecule fixedM = Standardization.StandardizeChairs(m);
            Assert.IsFalse(fixedM.HasSugarRing(), fixedM.ToString());
        }

        [TestMethod]
        public void Hexagon_LostAnomericStereoTests()
        {
            Molecule m = MoleculeFactory.Molecule(new Cdx(Resource1.lostAnomericStereo).ToMolFile());
            Molecule fixedM = Standardization.StandardizeChairs(m);
            Assert.AreEqual("OC1C=CC(=CC=1O[C@H]1OC[C@H](O)[C@H](O)[C@H]1O)C(O)=O", fixedM.ToSMILES(true));
            Molecule m2 = MoleculeFactory.Molecule(new Cdx(Resource1.lostAnomericStereo2).ToMolFile());
            Molecule fixedM2 = Standardization.StandardizeChairs(m2);
            Assert.AreEqual("C[C@@H]1O[C@@H](OC2=CC(=CC(O)=C2C(=O)C2C=C(O)C=C(O)C=2)OC)[C@H](O)[C@H](O)[C@H]1O", fixedM2.ToSMILES(true));
        }

        [TestMethod]
        public void Molecule_RectifyChairTest()
        {
            Cdx cdx = new Cdx(Resource1.nojirimycin);
            string ct = cdx.ToMolFiles().First();
            Molecule nojirimycin = MoleculeFactory.Molecule(ct);

            Molecule rectified = Standardization.StandardizeChairs(nojirimycin);
            Console.WriteLine(rectified.ToSMILES(true));
            string nojirimycinSMILES = "OC[C@H]1NC(O)[C@H](O)[C@@H](O)[C@@H]1O";
            Assert.AreEqual(nojirimycinSMILES,
                rectified.ToSMILES(true), "non-matching SMILES for left-handed chair");

            Cdx cdx2 = new Cdx(Resource1.righthandedchair);
            Molecule chairglucose = MoleculeFactory.Molecule(cdx2.ToMolFiles().First());
            Molecule rectifiedglucose = Standardization.StandardizeChairs(chairglucose);
            Console.WriteLine(rectifiedglucose.ToSMILES(true));
            string glucoseSMILES = "OC[C@H]1O[C@H](O)[C@H](O)[C@@H](O)[C@@H]1O";
            Assert.AreEqual(glucoseSMILES,
                rectifiedglucose.ToSMILES(true), "non-matching InChIs for right-handed chair");
        }

        /* ask Colin to fix
[TestMethod]
public void Hexagon_FixHorizontalHaworthsTest()
{
    Molecule horizontalhowarth = MoleculeFactory.Molecule(Resource1.horizontalhowarth);
    var Haworths = (from h in horizontalhowarth.Hexagons() where h.IsHaworth(horizontalhowarth) select h).ToList();
    Assert.AreEqual(5, Haworths.Count(), "wrong number " + Haworths.Count() + " of Haworth rings detected.");
    FileInfo StdRulesXMLFilePath = new FileInfo("StandardizationRules.xml");
    FileInfo AcidBaseRulesXMLFilePath = new FileInfo("acidgroups.xml");
    Standardization st = new Standardization(StdRulesXMLFilePath, new Acidity(AcidBaseRulesXMLFilePath));

    Molecule fix1 = st.StandardizeFirstHowarth(horizontalhowarth);
    Assert.AreEqual(4, (from h in fix1.Hexagons() where h.IsHaworth(fix1) select h).ToList().Count);
    Molecule fix2 = st.StandardizeFirstHowarth(fix1);
    Assert.AreEqual(3, (from h in fix2.Hexagons() where h.IsHaworth(fix2) select h).ToList().Count);

    CSMolecule fixt = RemoteMoleculeFactory.CSMolecule(st.StandardizeHowarths(horizontalhowarth));
    File.WriteAllText("fixedHorizontalHaworth.mol", fixt.ToString());
    Assert.IsFalse(fixt.HasHaworth());
}
 * 
 */
        [TestMethod]
        public void Hexagon_FixHaworthTest()
        {
            Molecule haworth = MoleculeFactory.Molecule(new Cdx(Resource1.haworth).ToMolFiles().First());
            Molecule fixt = Standardization.StandardizeHowarths(haworth);
            Assert.IsFalse(fixt.HasHexacycleWithBadStereo());
            Assert.IsFalse(fixt.HasHaworth());
            string std_inchi_output = InChINet.InChIUtils.mol2InChI(fixt.ct(), InChIFlags.Standard);
            Assert.AreEqual("InChI=1S/C6H12O6/c7-1-2-3(8)4(9)5(10)6(11)12-2/h2-11H,1H2/t2-,3-,4+,5-,6+/m1/s1", std_inchi_output);
        }

		//[TestMethod]
		public void Standardization_Hexagon()
		{
			string molfile = @"
   JSDraw212021408062D

 13 13  0  0  1  0              0 V2000
   18.7626  -15.1473    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
   17.2520  -15.5534    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   16.4724  -14.2053    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
   14.9618  -14.5950    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   13.4513  -14.2053    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   14.2310  -15.5534    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   15.7415  -15.1473    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   14.1822  -13.2469    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   12.6229  -13.2469    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
   14.9618  -11.8988    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
   11.9408  -14.6113    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
   14.2310  -17.1126    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
   15.7415  -13.5881    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
  1  2  1  0  0  0  0
  2  3  1  0  0  0  0
  2  7  1  0  0  0  0
  3  4  1  0  0  0  0
  4  5  1  0  0  0  0
  4  8  1  0  0  0  0
  5  6  1  0  0  0  0
  5 11  1  0  0  0  0
  6  7  1  0  0  0  0
  6 12  1  0  0  0  0
  7 13  1  0  0  0  0
  8  9  2  0  0  0  0
  8 10  1  0  0  0  0
M  END

$$$$
";
			string std_inchi_input = InChINet.InChIUtils.mol2InChI(molfile,InChIFlags.Standard);
			
			Indigo i = new Indigo();
			Molecule m = MoleculeFactory.FromMolFile(molfile, null, ChemicalFormat.V2000);
			Molecule m_new = Standardization.StandardizeHexagons(m, true);
			string std_inchi_output = InChINet.InChIUtils.mol2InChI(m_new.ct(), InChIFlags.Standard);
			Assert.IsTrue(!std_inchi_output.Contains("?"));
		}
	}
}
