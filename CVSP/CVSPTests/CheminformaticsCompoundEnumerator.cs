using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using com.ggasoftware.indigo;
using MoleculeObjects;

namespace CVSPTests
{
    /// <summary>
    /// 2015-08-27.
    /// All of these tests are in RSCCheminfToolkit now.
    /// </summary>
    [TestClass]
    public class CheminformaticsCompoundEnumerator
    {
        /// <summary>
        /// Make sure double bonds are processed correctly.
        /// </summary>
        [TestMethod]
        public void CompoundEnumerator_COOHTest()
        {
            Cdx cdx = new Cdx(Resource1.papilistatin);
            Molecule papilistatin = MoleculeFactory.Molecule(cdx.ToMolFile(CdxEnumerateOptions.EnumerateMarkush));
            Assert.AreEqual("C18 H12 O7", papilistatin.IndigoGrossFormula());
        }

        /// <summary>
        /// In the bracketsN case, each label 
        /// </summary>
        //[TestMethod] Colin will fix after fall ACS of 2013
        public void CompoundEnumerator_BracketsNTest()
        {
            Cdx cdx = new Cdx(Resource1.bracketsN);
           var cts = cdx.ToMolFiles();
            Assert.IsTrue(cts.ToList().Count == 2,
                String.Format("wrong number of generics ({0}, should be 2) extracted", cts.ToList().Count));
            Sdf sdf = new Sdf(String.Concat(cts));
            CompoundEnumerator ce = new CompoundEnumerator();
            List<Molecule> result = new List<Molecule>();
            int i = 0;
            foreach (GenericMolecule gm in sdf.genericMolecules)
            {
                i++;
                Console.WriteLine("iteration " + i);
                Assert.IsTrue(gm.HasProperty("Enumeration"), "generic molecule lacks enumeration" + gm.ToString());
                result.AddRange(ce.SubstituteMarkush(gm, gm.Property("Enumeration")));
            }
            File.WriteAllText("bracketsntest.sdf", String.Concat(from r in result select r.ToString()));
            Assert.IsTrue(result.Count == 3,
                String.Format("Wrong number of ({0}, should be 3) generics extracted.", result.Count));
        }

        [TestMethod]
        public void CompoundEnumerator_ButForTBuTest()
        {
            string mol = new Cdx(Resource1.reactionWithAbbreviations).ToMolFile(CdxEnumerateOptions.EnumerateMarkush);
            GenericMolecule g = MoleculeFactory.FromMolV2000(mol);
        }

        //[TestMethod] Colin will fix after fall ACS of 2013
        public void CompoundEnumerator_ParaXyleneTest()
        {
            Cdx cdx = new Cdx(Resource1.paraxylene);
            var cts = cdx.ToMolFiles();
            Assert.IsTrue(cts.ToList().Count == 1, "wrong number of generics extracted");
            Sdf sdf = new Sdf(String.Concat(cts));
            CompoundEnumerator ce = new CompoundEnumerator();
            List<Molecule> result = new List<Molecule>();
            foreach (GenericMolecule gm in sdf.genericMolecules)
            {
                result.AddRange(ce.SubstituteMarkush(gm, gm.Property("Enumeration")));
            }
            Assert.AreEqual(1, result.Count);
        }

        /// <summary>
        /// Tests whether a non-standard abbreviation can be handled by the enumeration code.
        /// </summary>
        //[TestMethod] Colin will fix after fall ACS of 2013
        public void CompoundEnumerator_CpcyclohexaneTest()
        {
            Cdx cdx = new Cdx(Resource1.Cpcyclohexane);
            var cts = cdx.ToMolFiles();
            Assert.AreEqual(1, cts.ToList().Count, "wrong number of generics extracted");
            GenericMolecule scaffold = MoleculeFactory.FromMolV2000(cts.First());
            CompoundEnumerator ce = new CompoundEnumerator();
            Molecule result = MoleculeFactory.FromGenericMolecule(ce.SubstituteMarkush(scaffold, "Cp", "Cp"));
            Assert.AreEqual("C1CCCCC1C1C=CC=C1", result.ToSMILES(true));
        }

        //[TestMethod] Colin will fix after fall ACS of 2013
        public void CompoundEnumerator_TwoMarkushMoleculesTest()
        {
            Cdx cdx = new Cdx(Resource1.twomarkush);
            var cts = cdx.ToMolFiles();
            Assert.AreEqual(2, cts.ToList().Count, "wrong number of generics");
            Sdf sdf = new Sdf(String.Join("", cts));
            File.WriteAllText("twomarkushmolecules.sdf", sdf.ToString());
            Assert.IsTrue(sdf.genericMolecules[1].FirstProperty("Enumeration").StartsWith("R = OH, OMe"));
            CompoundEnumerator ce = new CompoundEnumerator();
            List<Molecule> result = new List<Molecule>();
            foreach (GenericMolecule gm in sdf.genericMolecules)
            {
                result.AddRange(ce.SubstituteMarkush(gm, gm.Property("Enumeration")));
            }
            Assert.AreEqual(14, result.Count);
            File.WriteAllText("result.sdf", String.Join("", from r in result select r.ToString()));

        }

        [TestMethod]
        public void CompoundEnumerator_MultipleMarkushTest()
        {
            CompoundEnumerator ce = new CompoundEnumerator();
            List<Molecule> result = ce.SubstituteMarkush(
                MoleculeFactory.FromMolV2000(Resource1.RBenzene),
                new List<string>() { "R = H, F, Cl, Br, Me, Et, Ph" }).ToList();
            Assert.AreEqual(7, result.Count, "fewer (or more than) 7 molecules returned");
        }

        //[TestMethod] Colin will fix after fall ACS of 2013
        public void CompoundEnumerator_SingleMarkushSingleAtomTest()
        {
            CompoundEnumerator ce = new CompoundEnumerator();
            Molecule toluene = MoleculeFactory.FromGenericMolecule(ce.SubstituteMarkush(MoleculeFactory.FromMolV2000(Resource1.RBenzene), "R", "Me"));
            Assert.AreEqual("CC1C=CC=CC=1", toluene.ToSMILES(true), "compound not toluene");
        }

        //[TestMethod] Colin will fix after fall ACS of 2013
        public void CompoundEnumerator_R1Test()
        {
            CompoundEnumerator ce = new CompoundEnumerator();
            GenericMolecule t = ce.SubstituteMarkush(
                MoleculeFactory.Molecule(Resource1.R1group), "R1", "Me");
            Molecule toluene = MoleculeFactory.FromGenericMolecule(t);
            Assert.AreEqual("CC1C=CC=CC=1", toluene.ToSMILES(true), "compound not toluene");
        }

        //[TestMethod] Colin will fix after fall ACS of 2013
        public void CompoundEnumerator_SingleMarkushGroupTest()
        {
            CompoundEnumerator ce = new CompoundEnumerator();
            Molecule result = MoleculeFactory.FromGenericMolecule(ce.SubstituteMarkush(
                MoleculeFactory.Molecule(Resource1.RBenzene), "R", "Et"));
            Assert.AreEqual("CCC1C=CC=CC=1", result.ToSMILES(true), "compound not ethylbenzene");
        }

        [TestMethod]
        public void CompoundEnumerator_ConstructorTest()
        {
            CompoundEnumerator ce = new CompoundEnumerator();
            Assert.IsTrue(true, "CompoundEnumerator constructor did not succeed");
        }
    }
}
