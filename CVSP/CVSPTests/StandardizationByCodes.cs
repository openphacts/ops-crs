using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSC.CVSP;
using RSC.CVSP.Compounds;
using RSC.Logging;
using com.ggasoftware.indigo;
using MoleculeObjects;
using ChemSpider.Molecules;

namespace CVSPTests
{
    [TestClass]
    public class StandardizationByCodes : CVSPTestBase
    {
        static Indigo i;

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            i = new Indigo();
        }

        /// 400.1 = not tried
        /// 2015-09-15: no code generates this so we can't check it!

        /// <summary>
        /// 400.2 = Layout altered InChI
        /// </summary>
        [TestMethod]
        public void Standardization_LayoutAlteredInChI()
        {
            var codes = Standardization.Standardize(Resource1.eitherbond_molecule, Resources.Vendor.Indigo)
                .Issues.Select(c => c.Code);
            Assert.IsTrue(codes.Contains("400.2"));
        }

        /// 400.3 = layout failed
        /// 2015-09-16: can't find an example of this
        ///
        /// 400.4 = layout timed out
        /// 2015-09-16: can't find an example of this

        /// <summary>
        /// 400.5 = no atoms
        /// </summary>
        [TestMethod]
        public void Standardization_NoAtoms()
        {
            string input = Resource1.empty;
            StandardizationResult result = Standardization.Standardize(input, Resources.Vendor.Indigo);
            Assert.IsTrue(result.Issues.Select(i => i.Code).Contains("400.5"), "'no atoms' issue not detected");
        }

        /// <summary>
        /// 400.6 = too many atoms
        /// </summary>
        [TestMethod]
        public void Standardization_TooManyAtoms()
        {
            string input = i.loadMolecule(String.Concat(Enumerable.Range(1, 1000).Select(c => "CO"))).molfile();
            StandardizationResult result = Standardization.Standardize(input, Resources.Vendor.Indigo);
            Assert.IsTrue(result.Issues.Select(c => c.Code).Contains("400.6"), "'too many atoms' issue not detected");
        }

        /// <summary>
        /// 400.7 = stereo removal failed
        /// 2015-09-16: can't find an example of this
        /// </summary>

        /// <summary>
        /// 400.8 = chemaxon tautomer canonicalization error
        /// (not doing this now)
        /// </summary>
        /// <summary>
        /// 400.9 = chemaxon tautomer canonicalization warning
        /// (not doing this now)
        /// </summary>
        /// <summary>
        /// 400.10 = too many atoms for tautomer canonicalization
        /// (on hold until we have some sort of tautomer canonicalization code)
        /// </summary>

        /// <summary>
        /// 400.11 = chiral flag added
        /// 2015-10-02: no examples of this in practice.
        /// </summary>
        //[TestMethod]
        public void Standardization_ChiralFlagAdded()
        {
            string input = Resource1.chiralflagset;
            var codes = Standardization.Standardize(input, Resources.Vendor.Indigo).Issues.Select(c => c.Code);
            foreach (var c in codes) Console.WriteLine(c);
            Assert.IsFalse(codes.Contains("400.11"));
            string unset = StandardizationStereoModule.RemoveChiralFlag(input);
            var c2 = Standardization.Standardize(unset, Resources.Vendor.Indigo).Issues.Select(c => c.Code);
            foreach (var c in c2) Console.WriteLine(c);
            Assert.IsTrue(c2.Contains("400.11"));
        }

        /// 400.12 = standardization failed
        /// 2015-09-16: can't find an example of this
    }
}
