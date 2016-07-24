using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MoleculeObjects;

namespace CVSPTests
{
    /// <summary>
    /// 2015-08-27.
    /// Selectively copied to RSCCheminfToolkit.
    /// </summary>
    [TestClass]
    public class CheminformaticsSdf
    {
        public CheminformaticsSdf()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Sdf_CdxConstructorTest()
        {
            Sdf sdf = Cdx.ToSdf(Resource1.selenacalixtriazines);
            Assert.IsTrue(sdf.allAsGenerics.Count > 0, "no generics in sdf");
        }

        [TestMethod]
        public void Sdf_Utf8PreservedInSdfRoundTripTest()
        {
            Sdf sdf = new Sdf(Resource1.utf8containingsdf);
            string outfile = "utf8test.sdf";
            using (StreamWriter sw = new StreamWriter(outfile))
            {
                sw.WriteLine(sdf.ToString());
                Console.WriteLine("SDF OUT:" + sdf.ToString());
            }
            Console.WriteLine("reading it in:");
            Sdf savedSdf = new Sdf(File.ReadAllText(outfile));
            Assert.AreEqual(sdf.ToString(), savedSdf.ToString(), "roundtripped sdfs not identical");
        }

        [TestMethod]
        public void Sdf_SdfToStringTest()
        {
            Sdf sdf = new Sdf(Resource1.PartiallyDigestedSdf);
            string asString = sdf.ToString();
            Console.WriteLine(asString);
            Assert.IsTrue(asString.Contains("$$$$"), "No dollar signs in this sdf");
            Assert.IsTrue(asString.Contains("comp1"), "compound 1 missing");
            Assert.IsTrue(asString.Contains("comp2"), "compound 2 missing");
            Assert.IsTrue(asString.Contains("comp3"), "compound 3 missing");
        }

        [TestMethod]
        public void AddPropertyToGenericWithRoundTrippingTest()
        {
            Sdf sdf = new Sdf(Resource1.RBenzene,true);


            Sdf resultsdf = new Sdf();
            foreach (GenericMolecule gm in sdf.genericMolecules)
            {
                Dictionary<string,List<string>> props = gm.AddProperty("Regid", "test-comp1");
                GenericMolecule gm2 = new GenericMolecule(gm.Headers, gm.IndexedAtoms, gm.IndexedBonds, props);
                resultsdf.AddGenericMolecule(gm2);
            }
            Assert.IsTrue(resultsdf.ToString().Contains("test-comp1"), "resulting sdf " + resultsdf.ToString() + " does not contain 'test-comp1'");
            
        }
    }
}
