using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using com.ggasoftware.indigo;
using MoleculeObjects;

namespace CVSPTests
{
    /// <summary>
    /// 2015-08-27: set up in RSCCheminfToolkit.
    /// </summary>
    [TestClass]
    public class CheminformaticsReactions
    {
		/// <summary>
        /// The idea here is that we start with a cdx and finish with a mapped reaction.
        /// </summary>
        [TestMethod]
        public void Rxn_ImplicitHydrogenInReactantTest()
        {
            PartitionedCdx p = new PartitionedCdx(Resource1.ringcontraction);
            Rxn r = p.ToRxn();
            Assert.AreEqual(1, r.Reactants.Count(), "reactants" + Environment.NewLine + String.Concat(from rr in r.Reactants select rr.ToString()));
            Assert.AreEqual(1, r.Products.Count(), "products" + Environment.NewLine + String.Concat(from pp in r.Products select pp.ToString()));
            MappedRxn m = p.ToMappedRxn(); // will throw on fail
        }

        List<string> FCreactants = new List<string>()
            {
                "c1ccccc1", "C(C)(C)Cl"
            };
        List<string> FCproducts = new List<string>()
            {
                "c1ccccc1C(C)(C)Cl"
            };
        List<string> DAreactants = new List<string>()
        {
            "C=C", "C=CC=C"
        };
        List<string> DAproducts = new List<string>()
        {
            "C1C=CCCC1"
        };

        [TestMethod]
        public void Rxn_RoundTripTest()
        {
            MappedRxn vrn = RxnFactory.MappedRxn(Resource1.acylationRxn_unmapped); // throws on failure
            string oot = vrn.ToString();
            Rxn r2 = RxnFactory.Rxn(oot);
            foreach (GenericMolecule g in r2.Products)
            {
                Assert.IsTrue(g.HasAtomAtomMapping(), "product molecule has no aam");
            }
            foreach (GenericMolecule g in r2.Reactants)
            {
                Assert.IsTrue(g.HasAtomAtomMapping(), "reactant molecule has no aam");
            }
        }

        [TestMethod]
        public void Rxn_ConstructorFromSdfsTest()
        {
            string reactantsdf = String.Concat(from r in FCreactants
                                               select MoleculeFactory.Molecule(r, ChemicalFormat.SMILES));
            string productsdf = String.Concat(from p in FCproducts
                                              select MoleculeFactory.Molecule(p, ChemicalFormat.SMILES));
            Rxn rxn = RxnFactory.Rxn(new Sdf(reactantsdf), new Sdf(productsdf));
        }

        [TestMethod]
        public void Rxn_ValidationTest()
        {
            IEnumerable<Molecule> FCr = from r in FCreactants select MoleculeFactory.Molecule(r, ChemicalFormat.SMILES);
            IEnumerable<Molecule> FCp = from p in FCproducts select MoleculeFactory.Molecule(p, ChemicalFormat.SMILES);
            IEnumerable<Molecule> DAr = from r in DAreactants select MoleculeFactory.Molecule(r, ChemicalFormat.SMILES);
            IEnumerable<Molecule> empty = Enumerable.Empty<Molecule>();
            MappedRxn vrxn = new MappedRxn(FCr, FCp, empty); // throws on failure
            try
            {
                MappedRxn vbadrxn = new MappedRxn(DAr, FCp, empty);
                // if we get through to here then something has gone wrong!
                Assert.Fail("Bad reaction (Diels-Alder reactants, Friedel-Crafts products) should not validate");
            }
            catch
            {
                // success!
            }
        }

        [TestMethod]
        public void Rxn_ConstructorFromRxnTest()
        {
            Rxn rxn = RxnFactory.Rxn(Resource1.acylationRxn);
            Assert.AreEqual(1, rxn.Products.Count());
            Assert.AreEqual(2, rxn.Reactants.Count());
        }

        [TestMethod]
        public void Rxn_AbbreviationsTest()
        {
            PartitionedCdx p = new PartitionedCdx(Resource1.reactionWithAbbreviations);
            Rxn rxn = p.ToRxn();
            Assert.AreNotEqual(0, rxn.BothSides.Count(), "no reagents detected");
        }

       

        [TestMethod]
        public void Rxn_MoleculesAreMappedTest()
        {
            Rxn rxn = RxnFactory.Rxn(Resource1.acylationRxn);
            foreach (GenericMolecule m in rxn.Products)
            {
                Assert.IsTrue(m.HasAtomAtomMapping());
            }
            foreach (GenericMolecule m in rxn.Reactants)
            {
                Assert.IsTrue(m.HasAtomAtomMapping());
            }
        }

        [TestMethod]
        public void Molecule_HasAtomAtomMappingTest()
        {
            Molecule m = MoleculeFactory.Molecule(Resource1.aamAcidChloride);
            foreach (Atom a in m.IndexedAtoms.Values)
            {
                Console.WriteLine(a.AtomAtomMappingNo);
            }
            Assert.IsTrue(m.HasAtomAtomMapping(), "Atom–atom mapped molecule mappings not detected");
        }
    }

    [TestClass]
    public class CdxReactionTests
    {
        [TestMethod]
        public void PartitionedCdx_ByteArrayConstructorTest()
        {
            PartitionedCdx cdx = new PartitionedCdx(Resource1._101_2Mos);
        }

        [TestMethod]
        public void PartitionedCdx_RxnTest()
        {
            PartitionedCdx cdx = new PartitionedCdx(Resource1.reactionpartitioning);
            Rxn rxn = cdx.ToRxn();
            string rstring = rxn.ToString();
            Console.WriteLine(rstring);
            Assert.IsTrue(rstring.Contains("  2  1"));
        }

        [TestMethod]
        public void PartitionedCdx_NoBoundingBoxTest()
        {
            PartitionedCdx pc = new PartitionedCdx(Resource1.arrownoboundingbox);
            Assert.AreEqual(2, pc.reactants().Item1, "wrong number of reactants");
            Assert.AreEqual(1, pc.products().Item1, "wrong number of products");
            Assert.AreEqual(1, pc.reagents().Item1, "wrong number of reagents");
        }

        [TestMethod]
        public void RdFileNoPropsTest()
        {
            PartitionedCdx cdx = new PartitionedCdx(Resource1.reactionpartitioning);
            Rxn rxn = cdx.ToRxn();
            var rxns = new List<Tuple<Rxn, Dictionary<string, string>>>();
            Enumerable.Range(0, 10).ToList().ForEach(i => rxns.Add(new Tuple<Rxn, Dictionary<string, string>>(rxn, new Dictionary<string, string>())));
            RdFile rdf = new RdFile(rxns);
            string result = rdf.ToString();
            Assert.IsTrue(result.Contains("$RFMT"), result);
            Assert.IsFalse(result.Contains("$DTYPE") || result.Contains("$DATUM"), result);
            Console.WriteLine(result);
        }

        /// <summary>
        /// Assumes we can't do anything with purely textual reactants and so forth.
        /// </summary>
        [TestMethod]
        public void ReactionPartitioning_UnderTheArrowTest()
        {
            PartitionedCdx cdx = new PartitionedCdx(Resource1.underthearrow);
            Sdf reactants = new Sdf(String.Concat(cdx.reactants().Item2));
            Sdf products = new Sdf(String.Concat(cdx.products().Item2));
            Sdf catalysts = new Sdf(String.Concat(cdx.reagents().Item2));
            Assert.IsTrue(reactants.molecules.Count == 2,
                String.Format("wrong number ({0}) of reactants: {1}",
                    reactants.molecules.Count, reactants.ToString()));
            Assert.IsTrue(products.molecules.Count == 1,
                String.Format("wrong number ({0}) of products: {1}",
                    products.molecules.Count, products.ToString()));
            Assert.IsTrue(catalysts.molecules.Count == 2,
                String.Format("wrong number ({0}) of catalysts: {1}",
                    catalysts.molecules.Count, catalysts.ToString()));
            Console.WriteLine(catalysts.ToString());
        }

        [TestMethod]
        public void ReactionPartitioningTest_Basic()
        {
            PartitionedCdx cdx = new PartitionedCdx(Resource1.reactionpartitioning);
            Sdf reactants = new Sdf(String.Concat(cdx.reactants().Item2));
            Sdf products = new Sdf(String.Concat(cdx.products().Item2));

            Console.WriteLine("R: " + cdx.reactants().Item1 + " P:" + cdx.products().Item1 + " C: " + cdx.reagents().Item1);

            Sdf catalysts = new Sdf(String.Concat(cdx.reagents().Item2));
            Indigo i = new Indigo();
            IEnumerable<string> reactantSMILES = from m in reactants.molecules select m.ToSMILES(true);
            IEnumerable<string> productSMILES = from m in products.molecules select m.ToSMILES(true);

            Assert.AreEqual(2, reactants.molecules.Count, "wrong number of reactants");
            Assert.AreEqual(1, products.molecules.Count, "wrong number of products");
            Assert.AreEqual(0, catalysts.molecules.Count, "wrong number of reagents");

            Assert.IsTrue(reactantSMILES.Contains("C1CCCCC1"),
                "problem with reactants " + String.Join("; ", reactantSMILES));
            Assert.IsTrue(reactantSMILES.Contains("CCCCCCC"),
                "problem with reactants " + String.Join("; ", reactantSMILES));
            Assert.IsTrue(productSMILES.Contains("CCCCCCC1CCCCC1"),
                "problem with products " + String.Join("; ", productSMILES));
        }

        /// <summary>
        /// Tests that the unrecognized abbreviations PMP and Tol are processed.
        /// Doesn't work yet - scope for development!
        /// </summary>
        //[TestMethod]
        public void Reaction_UnrecognizedAbbreviationTest()
        {
            PartitionedCdx cdx = new PartitionedCdx(Resource1.PMPTol);
            string reactantsdf = String.Concat(cdx.reactants().Item2);
            string productsdf = String.Concat(cdx.products().Item2);
            Console.WriteLine(reactantsdf);
            Console.WriteLine();
            Console.WriteLine(productsdf);
            Sdf reactants = new Sdf(reactantsdf);
            Sdf products = new Sdf(productsdf);
            Assert.IsTrue(reactants.molecules.Count == 2,
                String.Format("wrong number ({0}) of reactants", reactants.molecules.Count));
            Assert.IsTrue(products.molecules.Count == 1,
                String.Format("wrong number ({0}) of products", products.molecules.Count));
        }

		[TestMethod]
		public void Reaction_ProblematicRxnMappingTest_Ongoing()
		{
			string rxn = @"$RXN

      JSDraw2  0901141316

  2  1
$MOL

   JSDraw209011413162D

  7  7  0  0  0  0              0 V2000
   12.2063  -11.5970    0.0000 C   0  0  0  0  0  0  0  0  0  0  1
   10.6549  -11.7602    0.0000 N   0  0  0  1  0  0  0  0  0  0  3
    9.8749  -13.1111    0.0000 C   0  0  0  0  0  0  0  0  0  0  7
    8.3490  -12.7868    0.0000 C   0  0  0  0  0  0  0  0  0  0  6
    8.1860  -11.2353    0.0000 C   0  0  0  0  0  0  0  0  0  0  5
    9.6111  -10.6009    0.0000 C   0  0  0  0  0  0  0  0  0  0  4
   13.1232  -12.8591    0.0000 C   0  0  0  0  0  0  0  0  0  0  2
  1  2  1  0  0  0  0
  2  3  4  0  0  0  0
  3  4  4  0  0  0  0
  4  5  4  0  0  0  0
  5  6  4  0  0  0  0
  6  2  4  0  0  0  0
  1  7  1  0  0  0  0
M  END
$MOL

   JSDraw209011413162D

  2  1  0  0  0  0              0 V2000
   16.2432  -11.8560    0.0000 C   0  0  0  0  0  0  0  0  0  0  8
   17.8032  -11.8560    0.0000 O   0  0  0  0  0  0  0  0  0  0  9
  1  2  2  0  0  0  0
M  END
$MOL

   JSDraw209011413162D

  9  9  0  0  0  0              0 V2000
   30.4066   -9.9698    0.0000 C   0  0  0  0  0  0  0  0  0  0  1
   29.7721  -11.3949    0.0000 N   0  0  0  1  0  0  0  0  0  0  3
   30.5521  -12.7460    0.0000 C   0  0  0  0  0  0  0  0  0  0  7
   29.5082  -13.9053    0.0000 C   0  0  0  0  0  0  0  0  0  0  6
   28.0831  -13.2707    0.0000 C   0  0  0  0  0  0  0  0  0  0  5
   28.2462  -11.7193    0.0000 C   0  0  0  0  0  0  0  0  0  0  4
   27.0869  -10.6755    0.0000 C   0  0  0  0  0  0  0  0  0  0  8
   25.6032  -11.1575    0.0000 O   0  0  0  0  0  0  0  0  0  0  9
   31.9580   -9.8067    0.0000 C   0  0  0  0  0  0  0  0  0  0  2
  1  2  1  0  0  0  0
  2  3  4  0  0  0  0
  3  4  4  0  0  0  0
  4  5  4  0  0  0  0
  5  6  4  0  0  0  0
  6  2  4  0  0  0  0
  6  7  1  0  0  0  0
  7  8  2  0  0  0  0
  1  9  1  0  0  0  0
M  END
";
		Indigo s_indigo  = new Indigo();
		IndigoObject obj = s_indigo.loadReaction(rxn);
		string smiles = obj.smiles();
		Assert.IsTrue(smiles.Equals("C(C)[n]1cccc1.C=O>>C(C)[n]1c(C=O)ccc1"));
		}
    }
}