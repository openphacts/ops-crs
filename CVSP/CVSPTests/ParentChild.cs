using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSC.Compounds;
using RSC.CVSP.Compounds;
using RSC.CVSP.Compounds.Operations;
using com.ggasoftware.indigo;

namespace CVSPTests
{
    using Microsoft.Ajax.Utilities;
    using Parent = RSC.CVSP.Compounds.Parent;

    [TestClass]
    public class ParentChild : CVSPTestBase
    {
        private static Indigo i;
        private static string sodiumCarboxylate;

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            ParentChild.i = new Indigo();
            ParentChild.sodiumCarboxylate = ParentChild.i.loadMolecule("[Na+].[O-]C=O").molfile();
        }

        /// <summary>
        /// Checks that we've plumbed in Autofac and so on correctly.
        /// </summary>
        [TestMethod]
        public void ParentChild_Constructor()
        {
            // should pass
            var cp = new CalculateParents(this.StandardizationModule, this.StandardizationChargesModule);
            Assert.IsNotNull(cp);
            try
            {
                // should fail
                var cpFail = new CalculateParents(null, null);
                Assert.Fail("should have failed: " + cpFail);
            }
            catch (ArgumentNullException)
            {
                // success!
            }
        }

        [TestMethod]
        public void ParentChild_OPSTestCase()
        {
            var cp = new CalculateParents(this.StandardizationModule, this.StandardizationChargesModule);
            var parents = cp.GenerateParents(Resource1.OPS1136544);
            this.TestWellformedness(parents);
            Assert.AreEqual(4, parents.Count, "wrong number of parents for molecule");
            foreach (var p in parents)
                Assert.IsFalse(p.Smiles.IsNullOrWhiteSpace(), p.StdInChI + " has null or empty SMILES");
        }

        [TestMethod]
        public void ParentChild_GenerateParentsSingleSpecies()
        {
            var cp = new CalculateParents(this.StandardizationModule, this.StandardizationChargesModule);
            var parents = cp.GenerateParents(Resource1.stereoIsotopeDMolecule);
            Assert.AreEqual(3, parents.Count, "wrong number of parents");
            this.TestWellformedness(parents);
            Assert.IsFalse(parents.Any(p => p.Relationship == ParentChildRelationship.Fragment),
                "no fragment parent should be generated");
            Assert.IsFalse(this.InChIForParent(parents, ParentChildRelationship.IsotopInsensitive).Contains("/i"),
                "isotope-insensitive parent should not contain isotope layer");
            Assert.IsFalse(this.InChIForParent(parents, ParentChildRelationship.StereoInsensitive).Contains("/t"),
                "stereo-insensitive parent should not contain stereo layer");
            Assert.IsFalse(this.InChIForParent(parents, ParentChildRelationship.StereoInsensitive).Contains("/b"),
                "stereo-insensitive parent should not contain double-bond layer");
        }

        [TestMethod]
        public void ParentChild_IsotopeParent()
        {
            var cp = new CalculateParents(this.StandardizationModule, this.StandardizationChargesModule);
            var parents = cp.GenerateIsotopeParents(Resource1.stereoIsotopeDMolecule);
            Assert.AreEqual(1, parents.Count, "wrong number of isotope parents");
            Assert.AreEqual("O[C@@H]1CC(F)CCC1", parents.First().Smiles, "wrong SMILES for parent");
        }

        [TestMethod]
        public void ParentChild_SuperParent()
        {
            var cp = new CalculateParents(this.StandardizationModule, this.StandardizationChargesModule);
            var parents = cp.GenerateSuperParents(Resource1.stereoIsotopeDMolecule);
            Assert.AreEqual(1, parents.Count, "wrong number of superparents");
            Assert.AreEqual("OC1CC(F)CCC1", parents.First().Smiles, "wrong SMILES for superparent");

            var p2 = cp.GenerateSuperParents(Resource1.stereoIsotopeChargeFragmentMolecule);
            Assert.AreEqual(1, p2.Count, "wrong number of superparents (fragment case)");
            Assert.AreEqual("[NaH].CC(C1C=C(O)C(C=CS)O1)C(O)=O", p2.First().Smiles, "wrong SMILES for superparent");
        }

        [TestMethod]
        public void ParentChild_FragmentParent()
        {
            var cp = new CalculateParents(this.StandardizationModule, this.StandardizationChargesModule);
            var parents = cp.GenerateParents(Resource1.stereoIsotopeChargeFragmentMolecule);
            this.TestWellformedness(parents);
            Assert.IsTrue(parents.Any(p => p.Relationship == ParentChildRelationship.Fragment),
                "fragment parent should be generated");
            var fragmentParentInChI = this.InChIForParent(parents, ParentChildRelationship.Fragment);
            Assert.IsFalse(fragmentParentInChI.Contains("."),
                "fragment parent InChI should not contain full stop (separates fragments in formula layer): "
                + fragmentParentInChI);
            Assert.IsFalse(fragmentParentInChI.Contains(";"),
                "fragment parent InChI should not contain semicolon (separates fragments outside formula layer): "
                + fragmentParentInChI);
        }

        [TestMethod]
        public void ParentChild_ChargeParent()
        {
            var cp = new CalculateParents(this.StandardizationModule, this.StandardizationChargesModule);
            var parents = cp.GenerateParents(ParentChild.sodiumCarboxylate);
            this.TestWellformedness(parents);
            Assert.AreEqual(10, parents.Count);
            Assert.IsTrue(parents.Any(p => p.Relationship == ParentChildRelationship.ChargeInsensitive),
                "charge parent should be generated");
            foreach (var p in parents) Console.WriteLine(@"{0} {1}", p.Smiles, p.Relationship);
        }

        public void TestWellformedness(IEnumerable<Parent> parents)
        {
            var enumerable = parents as IList<Parent> ?? parents.ToList();
            CollectionAssert.AllItemsAreNotNull(enumerable.Select(p => p.Relationship).ToList(), "all parents should have relationships");
            CollectionAssert.AllItemsAreNotNull(enumerable
                .Where(p => p.Relationship != ParentChildRelationship.TautomerInsensitive)
                .Select(p => p.MolFile).ToList(), "all parents (except the tautomer-insensitive one) should have mol files");
            CollectionAssert.AllItemsAreNotNull(enumerable.Select(p => p.StdInChI).ToList(), "all parents should have StdInChIs");
            CollectionAssert.AllItemsAreNotNull(enumerable.Select(p => p.NonStdInChI).ToList(), "all parents should have NonStdInChIs");
            CollectionAssert.AllItemsAreNotNull(enumerable.Select(p => p.Smiles).ToList(), "all parents should have SMILES");
            // and tautomeric InChIs
            CollectionAssert.AllItemsAreNotNull(enumerable.Where(p => p.Relationship == ParentChildRelationship.TautomerInsensitive)
                .Select(p => p.TautomericInChI).ToList(), "all parents should have tautomeric InChIs");
        }

        public string InChIForParent(ICollection<Parent> parents, ParentChildRelationship relationship)
        {
            return parents.First(r => r.Relationship == relationship).StdInChI.Inchi;
        }

        public void TestChargeParent(CalculateParents cp, string inputSmiles, string outputSmiles, string message)
        {
            var molfile = ParentChild.i.loadMolecule(inputSmiles).molfile();
            var qparents = cp.GenerateChargeParents(molfile);
            Assert.AreEqual(1, qparents.Count, string.Format("wrong number of charge parents ({0})", message));
            Assert.AreEqual(outputSmiles, qparents.First().Smiles.Split(' ')[0], string.Format("wrong smiles generated ({0})", message));
        }

        public void TestNoChargeParent(CalculateParents cp, string inputSmiles)
        {
            var molfile = ParentChild.i.loadMolecule(inputSmiles).molfile();
            var qparents = cp.GenerateChargeParents(molfile);
            Assert.IsFalse(qparents.Any(), "there should be no parents generated for structure " + inputSmiles);
        }

        [TestMethod]
        public void ParentChild_ReactorTest()
        {
            var reactor = new Reactor();
            var input = ParentChild.i.loadMolecule("[H][C-]CCCCC").molfile();
            var output = reactor.Product(input, "[C-,c-,N-,n-:1]>>[C,c,N,n:1]");
            Assert.AreEqual("CCCCCC", ParentChild.i.loadMolecule(output.Item1).canonicalSmiles(), "wrong product");
            Assert.IsTrue(output.Item2, "transform should have taken place");
        }

        [TestMethod]
        public void ParentChild_ChargeExamples()
        {
            var cp = new CalculateParents(this.StandardizationModule, this.StandardizationChargesModule);
            var neutralanyway = cp.GenerateParents(Resource1.benzeneAsMol);
            Assert.IsFalse(neutralanyway.Any(p => p.Relationship == ParentChildRelationship.ChargeInsensitive),
                "benzene should not generate a charge-insensitive parent");
            //CN01
            var neutralsodium = cp.GenerateChargeParents(ParentChild.sodiumCarboxylate);
            Assert.AreEqual("[NaH].OC=O", neutralsodium.First().Smiles, "wrong smiles for CN1");
            this.TestChargeParent(cp, "[CH5+]", "[CH5]", "CN02");
            this.TestChargeParent(cp, "CCCCCCCCCCCCCC[N+](C)(C)CCCCCCCCCCCCCCCCCCC", "CN(CCCCCCCCCCCCCC)CCCCCCCCCCCCCCCCCCC", "CN03");
            this.TestChargeParent(cp, "CCCCCCCCCCCCCC[N+](CC)(CC)CCCCCCCCCCCCCCCCC", "CCN(CCCCCCCCCCCCCC)CCCCCCCCCCCCCCCCC", "CN04");
            this.TestChargeParent(cp, "CCCC[O-]", "CCCCO", "CN05");
            this.TestChargeParent(cp, "[O-]CCCCC[O-]", "OCCCCCO", "CN05");
            this.TestChargeParent(cp, "C1CCCCC1[N-]", "NC1CCCCC1", "CN06");
            this.TestChargeParent(cp, "[Se-]SC1CCCC1", "[SeH]SC1CCCC1", "CN07");
            this.TestChargeParent(cp, "[H][C-]1CCCCC1", "C1CCCCC1", "CN08");

            this.TestChargeParent(cp, "[O-]N1CCCCC1", "ON1CCCCC1", "CN09");
            // sometimes the algorithm can't neutralize a compound
            this.TestNoChargeParent(cp, "CCC[N+](CCC)(CCC)CCC");
        }
    }
}
