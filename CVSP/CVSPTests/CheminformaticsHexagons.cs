using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MoleculeObjects;
using RSC.CVSP.Compounds;

namespace CVSPTests
{
    /// <summary>
    /// 2015-08-27.
    /// Copied across to RSCCheminfToolkit now.
    /// </summary>
    [TestClass]
    public class CheminformaticsHexagons : CVSPTestBase
    {
        public double AngleInDegrees(double angleinradians)
        {
            return 360 * angleinradians / (Math.PI * 2);
        }

        [TestMethod]
        public void Hexagon_MonosaccharideConfigurationTest()
        {
            Molecule l = MoleculeFactory.Molecule(Resource1.LglucoseRegular);
            Molecule d = MoleculeFactory.Molecule(Resource1.DglucoseRegular);
            Molecule lneg = MoleculeFactory.Molecule(Resource1.nystatin);
            Molecule lneg2 = MoleculeFactory.Molecule(Resource1.reboxetine);
            Hexagon hl = l.Hexagons().First();
            Hexagon hd = d.Hexagons().First();
            Assert.AreEqual(MonosaccharideConfiguration.D, hd.Configuration(d));
            Assert.AreEqual(MonosaccharideConfiguration.L, hl.Configuration(l));
            Assert.IsFalse(d.HasLPyranose(), "D-sugar wrongly identified as L-");
            Assert.IsTrue(l.HasLPyranose(), "L-sugar wrongly identified as not L-");
            Assert.IsFalse(lneg.HasLPyranose(), "nystatin wrongly identified as L-");
            Assert.IsFalse(lneg2.HasLPyranose(), "reboxetine wrongly identified as a sugar!");
        }

        [TestMethod]
        public void Hexagon_OrderedAnglesTest()
        {
            Molecule mol = MoleculeFactory.Molecule(Resource1.phantomchair);
            Substructure h = mol.Hexagons().First();
            var ob = h.OrderedBonds(h.Bonds.First().firstatomID);
            foreach (var pair in ob)
            {
                Console.WriteLine(String.Format("{0:F} {1}",
                    AngleInDegrees(pair.Item1.AngleXYPlane(pair.Item2)), pair.Item2));
            }
            Assert.AreEqual(210, AngleInDegrees(ob.First().Item1.AngleXYPlane(ob.First().Item2)), 0.001);
            Assert.AreEqual(180, AngleInDegrees(ob.ElementAt(1).Item1.AngleXYPlane(ob.ElementAt(1).Item2)), 0.001);
            Assert.AreEqual(150, AngleInDegrees(ob.ElementAt(2).Item1.AngleXYPlane(ob.ElementAt(2).Item2)), 0.001);
            Assert.AreEqual(30, AngleInDegrees(ob.ElementAt(3).Item1.AngleXYPlane(ob.ElementAt(3).Item2)), 0.001);
            Assert.AreEqual(0, AngleInDegrees(ob.ElementAt(4).Item1.AngleXYPlane(ob.ElementAt(4).Item2)), 0.001);
            Assert.AreEqual(330, AngleInDegrees(ob.Last().Item1.AngleXYPlane(ob.Last().Item2)), 0.001);

        }

        [TestMethod]
        public void Hexagon_OrderedBondTest()
        {
            Molecule mol = MoleculeFactory.Molecule(Resource1.phantomchair);
            Substructure h = mol.Hexagons().First();
            var ob = h.OrderedBonds(h.Bonds.First().firstatomID);
            foreach (var pair in ob)
            {
                Console.WriteLine(pair.Item1.ctline() + " " + pair.Item2);
            }
            int end = 0, start = 0;
            for (int i = 0; i < 4; i++)
            {
                end = ob.ElementAt(i).Item2 == BondSense.Forward
                    ? ob.ElementAt(i).Item1.secondatomID
                    : ob.ElementAt(i).Item1.firstatomID;
                start = ob.ElementAt(i + 1).Item2 == BondSense.Forward
                    ? ob.ElementAt(i + 1).Item1.firstatomID
                    : ob.ElementAt(i + 1).Item1.secondatomID;
                Assert.AreEqual(end, start, "Mismatch at position " + i + " between " + end + " and " + start);
            }
            // and by a commodious vicus of recirculation
            end = ob.Last().Item2 == BondSense.Forward
                ? ob.Last().Item1.secondatomID
                : ob.Last().Item1.firstatomID;
            start = ob.First().Item2 == BondSense.Forward
                ? ob.First().Item1.firstatomID
                : ob.First().Item1.secondatomID;
            Assert.AreEqual(end, start, "Mismatch at position 6 between " + end + " and " + start);

        }

        [TestMethod]
        public void Hexagon_PhantomChairTest()
        {
            Molecule mol = MoleculeFactory.Molecule(Resource1.phantomchair);
            Assert.IsFalse(mol.HasChair(), "PHANTOM CHAIR! (regular hexagon mis-identified as a chair)");
        }

        [TestMethod]
        public void Molecule_SugarRingTest()
        {
            Console.WriteLine(DateTime.Now.Second + " " + DateTime.Now.Millisecond);
            Cdx cdx = new Cdx(Resource1.chairglucose);
            Console.WriteLine(DateTime.Now.Second + " " + DateTime.Now.Millisecond);

            var mols = cdx.ToMolFiles();
            Console.WriteLine(DateTime.Now.Second + " " + DateTime.Now.Millisecond);

            Molecule chair = MoleculeFactory.Molecule(mols.First());
            Console.WriteLine(DateTime.Now.Second + " " + DateTime.Now.Millisecond);

            Molecule notchair = MoleculeFactory.Molecule(mols.ElementAt(1));
            Console.WriteLine(DateTime.Now.Second + " " + DateTime.Now.Millisecond);

            Assert.IsTrue(chair.HasSugarRing());
            Console.WriteLine(DateTime.Now.Second + " " + DateTime.Now.Millisecond);


            Assert.IsFalse(notchair.HasSugarRing());
            Console.WriteLine(DateTime.Now.Second + " " + DateTime.Now.Millisecond);

        }


        [TestMethod]
        public void Hexagon_MajorAxisTest()
        {
            Molecule m = MoleculeFactory.Molecule(Resource1.chairsAtAllAngles);
            foreach (Hexagon h in m.Hexagons())
            {
                var majoraxis = h.MajorAxis();
                Console.WriteLine(majoraxis);
                Assert.IsTrue(m.IndexedAtoms[majoraxis.Item1].x < m.IndexedAtoms[majoraxis.Item2].x, "major axis returned wrong way round");
            }
        }

        [TestMethod]
        public void Molecule_DoubleChairTests()
        {
            Molecule upup = MoleculeFactory.Molecule(Resource1.doublechairUU);
            IEnumerable<Substructure> hexagons = upup.RingsOfSize(6);
            Console.WriteLine(hexagons.First().Signature());

            IEnumerable<Substructure> decagons = upup.RingsOfSize(10);
            Console.WriteLine("UPUP: " + decagons.First().Signature());
            Molecule updown = MoleculeFactory.Molecule(Resource1.doublechairUD);
            Console.WriteLine("UPDOWN: " + updown.RingsOfSize(10).First().Signature());
            Molecule downdown = MoleculeFactory.Molecule(Resource1.doublehexagonDD);
            Console.WriteLine("DOWNDOWN: " + downdown.RingsOfSize(10).First().Signature());
        }


        [TestMethod]
        public void Molecule_SugarSignaturesTest()
        {
            Molecule flat = MoleculeFactory.Molecule(Resource1.regularHexagon);
            Assert.IsTrue(flat.Hexagons().Count() == 1, "problem with hexagon signature for tetrahydropyran");
            Molecule benzene = MoleculeFactory.Molecule(Resource1.benzeneAsMol);
            Assert.IsTrue(benzene.Hexagons().Count() == 0, "problem with hexagon signature for benzene " + String.Join("; ", from h in benzene.Hexagons() select h.Signature()));
        }

        [TestMethod]
        public void Molecule_PerspectiveSugarsTest()
        {
            Molecule flat = MoleculeFactory.Molecule(Resource1.regularHexagon);
            Molecule chair = MoleculeFactory.Molecule(Resource1.chair);
            Molecule twistboat = MoleculeFactory.Molecule(Resource1.twistboat);
            Assert.AreEqual(HexagonGeometry.Regular, flat.Hexagons().First().Geometry());
            Assert.AreEqual(HexagonGeometry.Chair, chair.Hexagons().First().Geometry());
            Assert.AreEqual(HexagonGeometry.TwistBoat, twistboat.Hexagons().First().Geometry());
        }

        [TestMethod]
        public void Hexagon_BoatTest()
        {
            Molecule boat = MoleculeFactory.Molecule(new Cdx(Resource1.boatsugar).ToMolFiles().First());
            foreach (Hexagon h in boat.Hexagons()) Console.WriteLine(h.Signature());
            Assert.IsTrue(boat.HasBoat());
        }

        [TestMethod]
        public void Hexagon_ExternalBondsTest()
        {
            Molecule howarth = MoleculeFactory.Molecule(new Cdx(Resource1.haworth).ToMolFiles().First());
            Hexagon h = howarth.Hexagons().First();
            Assert.AreEqual(10, h.ExternalBonds(howarth).ToList().Count, "wrong number of external bonds identified");
        }

        [TestMethod]
        public void Hexagon_DetectHaworthTest()
        {
            Molecule howarth = MoleculeFactory.Molecule(new Cdx(Resource1.haworth).ToMolFiles().First());
            Assert.IsTrue(howarth.HasHexacycleWithBadStereo());
            Assert.IsTrue(howarth.HasHaworth());
            Molecule negative = MoleculeFactory.Molecule(Resource1.fusedringsnohowarth);
            Assert.IsFalse(negative.HasHaworth(), "false positive Haworth ring in fused structure");
            Molecule neg2 = MoleculeFactory.Molecule(Resource1.phantomhaworth);
            Assert.IsFalse(neg2.HasHaworth(), "false positive regular identified as Haworth: check wedginess");
            Molecule neg3 = MoleculeFactory.Molecule(Resource1.phantomhaworthNovobiocin);
            Assert.IsFalse(neg3.HasHaworth(), "false positive regular identified as Haworth: novobiocin");
            Molecule neg4 = MoleculeFactory.Molecule(Resource1.phantomHowarthGlucosamine);
            Assert.IsFalse(neg4.HasHaworth(), "false positive regular identified as Haworth: glucosamine");
            Console.WriteLine("ERYTHROMYCIN");
            Molecule neg5 = MoleculeFactory.Molecule(Resource1.erythromycin);
            Assert.IsFalse(neg5.HasHaworth(), "false positive regular: erythromycin");
            Console.WriteLine("JOSAMYCIN");
            Molecule neg6 = MoleculeFactory.Molecule(Resource1.josamycin);
            Assert.IsFalse(neg6.HasHaworth(), "false positive regular: josamycin");

        }

        [TestMethod]
        public void Hexagon_FusedRingTest()
        {
            Molecule unfused = MoleculeFactory.Molecule(new Cdx(Resource1.boatsugar).ToMolFiles().First());
            Hexagon h = unfused.Hexagons().First();
            Assert.IsFalse(h.FusedRing(unfused));
            Molecule unfusedhexagon = MoleculeFactory.Molecule(new Cdx(Resource1.unfusedhexagonfusedrings).ToMolFiles().First());
            Hexagon k = unfusedhexagon.Hexagons().First();
            Assert.IsFalse(k.FusedRing(unfusedhexagon));
        }

        [TestMethod]
        public void Hexagon_DetectHorizontalHaworthsTest()
        {
            Molecule horizontalhowarth = MoleculeFactory.Molecule(Resource1.horizontalhowarth);
            var Haworths = (from h in horizontalhowarth.Hexagons() where h.IsHaworth(horizontalhowarth) select h).ToList();
            Assert.AreEqual(5, Haworths.Count(), "wrong number " + Haworths.Count() + " of Haworth rings detected.");
        }
    }
}
