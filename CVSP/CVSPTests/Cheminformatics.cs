using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSC.CVSP.Compounds;
using RSC.CVSP;

using MoleculeObjects;

namespace CVSPTests
{
    /// <summary>
    /// 2015-08-27.
    /// All relevant tests now in RSCCheminfToolkit.
    /// </summary>
    [TestClass]
    public class Cheminformatics
    {
        [TestMethod]
        public void Molecule_IsOverallChargedTest()
        {
            Molecule heme = MoleculeFactory.Molecule(Resource1.hemeAsMol);
            Assert.IsFalse(heme.TotalCharge() != 0);
        }



        [TestMethod]
        public void MoleculeFactory_NotAValidCTFileTest()
        {
            try
            {
                GenericMolecule gm = MoleculeFactory.FromMolV2000("benzene.mol");
                Assert.Fail("should have thrown an exception by now");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.ToString().Contains("not a valid ctfile"), "did not throw the right exception " + e);
            }
        }

        [TestMethod]
        public void MoleculeFactory_ctFileFirstLineTest()
        {
            GenericMolecule gm = MoleculeFactory.FromMolV2000(Resource1.benzeneAsMol);
            Assert.AreEqual("benzene.cdx", gm.Headers.First(), String.Format("problem with first line, {0}", gm.Headers.First()));
        }

        [TestMethod]
        public void MoleculeFactory_ctFileSecondLineTest()
        {
            GenericMolecule gm = MoleculeFactory.FromMolV2000(Resource1.benzeneAsMol);
            Assert.AreEqual(" OpenBabel09081111092D", gm.Headers[1], String.Format("problem with second line, {0}", gm.Headers[1]));
        }

        [TestMethod]
        public void Molecule_HasUnevenLengthBondsTest()
        {
            Molecule stretchy = MoleculeFactory.Molecule((new Cdx(Resource1.stretchedbondcdx)).ToMolFiles().First());
            Assert.IsTrue(stretchy.HasUnevenLengthBonds(), stretchy.ToString());
            Molecule clashy = MoleculeFactory.Molecule(Resource1.clashymol);
            Assert.IsTrue(clashy.HasUnevenLengthBonds());
            Molecule benzene = MoleculeFactory.Molecule(Resource1.benzeneAsMol);
            Assert.IsFalse(benzene.HasUnevenLengthBonds());
        }

        [TestMethod]
        public void Molecule_ConstructorTest()
        {
            string molfile = Resource1.clashymol;

            Molecule target = MoleculeFactory.Molecule(molfile);
            Assert.AreNotEqual(0, target.IndexedAtoms.Count, "no atoms in molecule.");
        }

        [TestMethod]
        public void GenericMolecule_DedupeAtomsTest()
        {
            GenericMolecule clashy = MoleculeFactory.FromMolV2000(Resource1.clashymol);
            GenericMolecule result = clashy.DedupeAtoms();
            bool stillClashy = result.HasDuplicateAtoms();
            Assert.IsFalse(stillClashy, "Molecule still contains duplicates");
        }

        /// <summary>
        /// Resource1.clashymol was output by OBNET.convert and contains duplicate atoms.
        /// Resource1.benzene, of course, doesn't.
        ///</summary>
        [TestMethod]
        public void GenericMolecule_ContainsDuplicateAtomsTest()
        {
            string clashy = Resource1.clashymol;
            GenericMolecule target = MoleculeFactory.FromMolV2000(clashy);
            bool actual = target.HasDuplicateAtoms();
            Assert.AreEqual(true, actual, "Duplicate atoms not identified");
            string notClashy = Resource1.benzeneAsMol;
            GenericMolecule benzene = MoleculeFactory.FromMolV2000(notClashy);
            actual = benzene.HasDuplicateAtoms();
            Assert.AreEqual(false, actual, "Erroneous identification of duplicate atoms");
        }

        [TestMethod]
        public void GenericMolecule_ContainsNonInChIfiableAtomsTests()
        {
            GenericMolecule rbenzene = MoleculeFactory.FromMolV2000(Resource1.RBenzene);
            GenericMolecule benzene = MoleculeFactory.FromMolV2000(Resource1.benzeneAsMol);
            GenericMolecule methylphenylhydrazine = MoleculeFactory.FromMolV2000(Resource1._1_methylphenylhydrazine);
            GenericMolecule cyclohexylcompound = MoleculeFactory.FromMolV2000(Resource1.cyclohexylcompound);

            Assert.IsTrue(methylphenylhydrazine.HasNonInChIfiableAtoms());
            Assert.IsTrue(cyclohexylcompound.HasNonInChIfiableAtoms());

            string bigGenericString = new Cdx(Resource1.bigGeneric).ToMolFile();
            GenericMolecule bigGeneric = MoleculeFactory.FromMolV2000(bigGenericString);

            Assert.IsTrue(rbenzene.HasNonInChIfiableAtoms());
            Assert.IsTrue(bigGeneric.HasNonInChIfiableAtoms());
            Assert.IsFalse(benzene.HasNonInChIfiableAtoms(), benzene.HasNonInChIfiableAtoms().ToString());
        }

        [TestMethod]
        public void Molecule_AromaticRingCountTest()
        {
            Molecule benzene = MoleculeFactory.Molecule(Resource1.benzeneAsMol);
            Molecule cyclohexane = MoleculeFactory.Molecule(Resource1.regularHexagon);
            Assert.AreEqual(1, benzene.AromaticRingCount().Item1);
            Assert.AreEqual(0, cyclohexane.AromaticRingCount().Item1);
        }

        [TestMethod]
        public void GenericMolecule_QueryBondTest()
        {
            GenericMolecule gm = MoleculeFactory.FromMolV2000(Resource1.querybonds);
            Assert.IsTrue(gm.HasQueryBonds(), "query bond molecule's query bonds not detected");
            GenericMolecule noquery = MoleculeFactory.FromMolV2000(Resource1.benzeneAsMol);
            Assert.IsFalse(noquery.HasQueryBonds(), "query bonds erroneously identified");
        }

        [TestMethod]
        public void Molecule_ChargesInAtomBlockTest()
        {
            Molecule molecule = MoleculeFactory.Molecule(Resource1.chargesInAtomBlock);
            Assert.IsTrue(molecule.TotalCharge() == -1, molecule.ct());
        }

        [TestMethod]
        public void Atom_AtomTranslationTest()
        {
            Atom a = new Atom(0.0, 0.0, 0.0, "C");
            var translation = new Tuple<double, double, double>(1.0, 1.0, 1.0);
            Atom b = a.Translate(translation);
            Assert.AreEqual(b.xyz.Item1, -1.0, 1e-7, "X position wrong", b.x);
            Assert.AreEqual(b.xyz.Item2, -1.0, 1e-7, "Y position wrong", b.y);
            Assert.AreEqual(b.xyz.Item3, -1.0, 1e-7, "Z position wrong", b.z);
        }

        [TestMethod]
        public void Atom_AtomRotationTest()
        {
            Atom a = new Atom(1.0, 0.0, 0.0, "C");
            var angle = Math.PI / 2; // rotate to twelve o'clock
            Console.WriteLine("before rotation " + a.xyz);

            Atom b = a.Rotate2D(angle);
            Console.WriteLine("after rotation " + b.xyz);
            Assert.AreEqual(b.xyz.Item1, 0.0, 1e-7, String.Format("X position wrong, should be 0.0, is {0}", b.xyz.Item1));
            Assert.AreEqual(b.xyz.Item2, 1.0, 1e-7, String.Format("Y position wrong, should be 1.0, is {0}", b.xyz.Item2));
            Assert.AreEqual(b.xyz.Item3, 0.0, 1e-7, String.Format("Z position wrong, should be 0.0, is {0}", b.xyz.Item3));
        }

        public void CheckEquality(double expectedX, double expectedY, Tuple<double, double> result)
        {
            Assert.AreEqual(expectedX, result.Item1, 1e-10, "non-matching X");
            Assert.AreEqual(expectedY, result.Item2, 1e-10, "non-matching Y");
        }

        [TestMethod]
        public void Extensions_WalkTests()
        {
            var point = new Tuple<double, double>(20, 30);
            var newpoint = point.Walk(0, Math.PI);
            CheckEquality(20, 30, newpoint);
            var threeoclock = point.Walk(100, 0);
            CheckEquality(120, 30, threeoclock);
            var twelveoclock = point.Walk(100, Math.PI / 2);
            CheckEquality(20, 130, twelveoclock);
            var nineoclock = point.Walk(100, Math.PI);
            CheckEquality(-80, 30, nineoclock);
            var sixoclock = point.Walk(100, 3 * Math.PI / 2);
            CheckEquality(20, -70, sixoclock);
        }

        [TestMethod]
        public void Extensions_BoundingBoxContainsTest()
        {
            var bb = new Tuple<double, double, double, double>(0, 0, 100, 100);
            var inPoint = new Tuple<double, double>(50, 50);
            var outPoint1 = new Tuple<double, double>(150, 50);
            var outPoint2 = new Tuple<double, double>(50, 150);
            var outPoint3 = new Tuple<double, double>(150, 150);
            Assert.IsTrue(bb.Contains(inPoint));
            Assert.IsFalse(bb.Contains(outPoint1), String.Format("{0}", outPoint1));
            Assert.IsFalse(bb.Contains(outPoint2), String.Format("{0}", outPoint2));
            Assert.IsFalse(bb.Contains(outPoint3), String.Format("{0}", outPoint3));
        }

        [TestMethod]
        public void Molecule_NoUnnecessaryPropsTest()
        {
            string mol = MoleculeFactory.Molecule(Resource1.benzeneAsMol).ct();
            Assert.IsFalse(mol.Contains("M  CHG"), "spurious charge line");
            Assert.IsFalse(mol.Contains("M  ISO"), "spurious isotope line");
            Assert.IsFalse(mol.Contains("M  RAD"), "spurious radicals line");
        }

        [TestMethod]
        public void Molecule_MoleculeOfDeathTest()
        {
            Molecule m2 = MoleculeFactory.Molecule(Resource1.highlysymmetricmol2);
            Molecule m = MoleculeFactory.Molecule(Resource1.highlysymmetricmol);
        }

        [TestMethod]
        public void MoleculeFactory_CapitalElements()
        {
            string mol = @"
  ACCLDraw09251307432D

 15 15  0  0  0  0  0  0  0  0999 V2000
    6.6340   -4.0651    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.6763   -4.0646    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.6571   -3.4748    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.6763   -5.2460    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    6.6340   -5.2513    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.6597   -5.8356    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.2500   -3.3125    0.0000 CL  0  0  0  0  0  0  0  0  0  0  0  0
    7.5625   -7.5625    0.0000 BR  0  0  0  0  0  0  0  0  0  0  0  0
    9.6992   -3.4740    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   10.7221   -4.0646    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   11.7449   -3.4740    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   12.7678   -4.0646    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
   11.7449   -2.2929    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
   13.9087   -4.3703    0.0000 ZN  0  0  0  0  0  0  0  0  0  0  0  0
   15.0313   -3.7500    0.0000 BR  0  0  0  0  0  0  0  0  0  0  0  0
  6  4  1  0  0  0  0
  5  6  2  0  0  0  0
  2  3  1  0  0  0  0
  1  5  1  0  0  0  0
  4  2  2  0  0  0  0
  3  1  2  0  0  0  0
  1  7  1  0  0  0  0
  6  8  1  0  0  0  0
  2  9  1  0  0  0  0
  9 10  1  0  0  0  0
 10 11  1  0  0  0  0
 11 12  1  0  0  0  0
 11 13  2  0  0  0  0
 12 14  1  0  0  0  0
 14 15  1  0  0  0  0
M  END";
            GenericMolecule m = MoleculeFactory.FromMolV2000(mol);
            Assert.IsFalse(m.HasNonInChIfiableAtoms(), "Capitalized element symbols should not return noninchifiable atoms");
        }

        [TestMethod]
        public void V3000_FactoryTest()
        {
            GenericMolecule gm = MoleculeFactory.FromMolV3000(Resource1.benzeneV3000);
            Assert.IsTrue(gm.IndexedAtoms.Count == 6);
            Assert.IsTrue(gm.IndexedBonds.Count == 6);
        }

        [TestMethod]
        public void V3000_RoundTripTest()
        {
            GenericMolecule gm = MoleculeFactory.FromMolV3000(Resource1.benzeneV3000);
            Assert.AreEqual(Resource1.benzeneV3000.Length, gm.v3000ct().Length);
            Assert.AreEqual(Resource1.benzeneV3000, gm.v3000ct(), "output doesn't match original");
        }

        [TestMethod]
        public void MoleculeFactory_GenerateFromJSDrawOutputTest()
        {
            Molecule jsdraw = MoleculeFactory.Molecule(Resource1.neutralJSDraw);
            Assert.AreEqual("CC1N=CC(CO)=C(N)N=1", jsdraw.ToSMILES(true), "Wrong SMILES");
        }

        [TestMethod]
        public void Molecule_TautomerGoodnessScoreTest()
        {
            Molecule mol1 = MoleculeFactory.Molecule(Resource1.goodness126388);
            Molecule mol2 = MoleculeFactory.Molecule(Resource1.goodness40405);
            Assert.IsTrue(mol1.TautomerGoodnessScore() == 102);
            Assert.IsTrue(mol2.TautomerGoodnessScore() == 200);
        }

        [TestMethod]
        public void Molecule_ExpandSMARTSTests()
        {
            Assert.IsTrue(Molecule.ExpandSMARTSAbbn("{M}").Contains("K"));
            Assert.IsTrue(Molecule.ExpandSMARTSAbbn("{NM}").Contains("Se"));
        }

        [TestMethod]
        public void MoleculeConstructor_PubChemTest()
        {
            Molecule pubchem = MoleculeFactory.Molecule(Resource1.pubchemMol);
            // should have got here unless it throws an exception so all is well
        }
    }
}
