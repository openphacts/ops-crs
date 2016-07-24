using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using RSC.CVSP.Compounds;
using MoleculeObjects;
using com.ggasoftware.indigo;
using RSC.CVSP;
using RSC.Logging;

namespace CVSPTests
{
    [TestClass]
    public class ValidationIndigoByCodes : CVSPTestBase
    {
        static Indigo i;

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            i = new Indigo();
            i.setOption("treat-x-as-pseudoatom", true);
        }

        public void TestValidation(string ctReturnsCode, string ctDoesNotReturnCode, string code, string message)
        {
            var codes = Validation.Validate(ctReturnsCode).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains(code), message + " codes returned = " + String.Join("; ", codes));
            var noCodes = Validation.Validate(ctDoesNotReturnCode).Issues.Select(i => i.Code);
            Assert.IsFalse(noCodes.Contains(code), message + " codes returned = " + String.Join("; ", noCodes));
        }

        /// <summary>
        /// 200.1 = molecule (indigo)
        /// 2015-09-15: Deactivated because I can't find any examples in practice.
        /// </summary>
        //[TestMethod]
        public void Validation_IndigoMolecule()
        {
            List<Issue> issues = new List<Issue>();
            IndigoObject io = ValidationModule.TryLoadingSdfToIndigo(Resource1.pseudoatomMolecule, issues);
            foreach (var c in issues.Select(i => i.Code))
                Console.WriteLine(c);
            Assert.Inconclusive("not finished yet");
        }

        /// <summary>
        /// 200.2 = molfile loader
        /// </summary>
        [TestMethod]
        public void Validation_IndigoMolfileLoader()
        {
            List<Issue> issues = new List<Issue>();
            IndigoObject io = ValidationModule.TryLoadingSdfToIndigo(Resource1.pseudoatomMolecule, issues);
            Assert.IsTrue(issues.Select(c => c.Code).Contains("200.2"));
        }

        /// <summary>
        /// 200.3 = allene stereo (indigo)
        /// 2015-09-18: Deactivated because I can't find any examples in practice.
        /// </summary>
        //[TestMethod]
        public void Validation_IndigoAlleneStereo()
        {
            List<Issue> issues = new List<Issue>();
            IndigoObject io = ValidationModule.TryLoadingSdfToIndigo(Resource1.badallene, issues);
            Assert.IsTrue(issues.Any(), "there should be some issues");
            Assert.IsTrue(issues.Select(c => c.Code).Contains("200.3"), String.Join("; ", issues.Select(c => c.Code)));
        }

        /// <summary>
        /// 200.4 = element (indigo)
        /// Ideally one shouldn't get here, but just in case.
        /// 2015-09-18: Deactivated because I can't find any examples in practice.
        /// </summary>
        //[TestMethod]
        public void Validation_IndigoBadElement()
        {
            List<Issue> issues = new List<Issue>();
            IndigoObject io = ValidationModule.TryLoadingSdfToIndigo(Resource1.electron, issues);
            Assert.IsNotNull(io);
            Assert.IsTrue(issues.Any(), "there should be some issues");
            Assert.IsTrue(issues.Select(c => c.Code).Contains("200.4"), String.Join("; ", issues.Select(c => c.Code)));
        }

        /// <summary>
        /// 200.5 = miscellaneous stereocentre error
        /// </summary>
        [TestMethod]
        public void Validation_IndigoStereocentres()
        {
            var codes = Validation.Validate(Resource1.stereocentresOther).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("200.5"), "code 200.5 missing " + String.Join("; ", codes));
        }

        /// <summary>
        /// 200.6 = angle between bonds too small
        /// </summary>
        [TestMethod]
        public void Validation_IndigoAngleBetweenBondsTooSmall()
        {
            var codes = Validation.Validate(Resource1.toosmallangle).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("200.6"));
        }

        /// <summary>
        /// 200.7 = stereo types of the opposite bonds mismatch
        /// </summary>
        [TestMethod]
        public void Validation_IndigoStereoTypesOfOppositeBondsMismatch()
        {
            TestValidation(Resource1.oppositebondmismatch, Resource1.benzeneAsMol, "200.7", "stereo types of the opposite bonds mismatch");
        }

        /// <summary>
        /// 200.8 = one bond up, one bond down
        /// </summary>
        [TestMethod]
        public void Validation_IndigoOneBondUpOneDown()
        {
            TestValidation(Resource1.onebonduponebonddown, Resource1.benzeneAsMol, "200.8", "should warn about one bond up, one bond down");
        }

        /// <summary>
        /// 200.9 = non-opposite bonds match
        /// </summary>
        [TestMethod]
        public void Validation_IndigoStereoTypesOfNonOppositeBondsMatchNearAtom()
        {
            TestValidation(Resource1.nonoppositebondsmatch, Resource1.oppositebondmismatch, "200.9", "non-opposite bonds match");
        }

        /// <summary>
        /// 200.10 = hydrogen beside implicit hydrogen near stereocentre
        /// </summary>
        [TestMethod]
        public void Validation_IndigoImplicitHydrogenNearStereocentre()
        {
            var codes = Validation.Validate(Resource1.implicithydrogen).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("200.10"));
        }

        /// <summary>
        /// 200.11 = two hydrogens near stereocenters
        /// 2015-09-15: Deactivated because I can't find any examples in practice.
        /// </summary>
        //[TestMethod]
        public void Validation_IndigoTwoHydrogensAtStereocentre()
        {
            Assert.Inconclusive("test not written");
        }

        /// <summary>
        /// 200.12 = indigo stereocenters degenerate case
        /// </summary>
        [TestMethod]
        public void Validation_IndigoDegenerateStereocentres()
        {
            var codes = Validation.Validate(Resource1.degeneratestereocenters).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("200.12"));
        }

        /// <summary>
        /// 200.13 = indigo InChI generator
        /// 2005-09-15: Deactivated because I can't find any examples in practice.
        /// </summary>
        //[TestMethod]
        public void Validation_IndigoInChI()
        {
            Assert.Inconclusive("test not written");
        }

        /// <summary>
        /// 200.14 = indigo other
        /// 2005-09-15: Deactivated because I can't find any examples in practice.
        /// </summary>
        //[TestMethod]
        public void Validation_IndigoOther()
        {
            Assert.Inconclusive("test not written");
        }

        /// <summary>
        /// 200.15 = Indigo SMILES saver
        /// </summary>
        [TestMethod]
        public void Validation_IndigoSMILESsaver()
        {
            var codes = Validation.Validate(Resource1.dodgyiodine).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("200.15"), "code 200.15 missing " + String.Join("; ", codes));
        }

        /// <summary>
        /// 200.16 = Indigo SMILES loader
        /// 2005-09-15: Deactivated because I can't find any examples in practice.
        /// </summary>
        //[TestMethod]
        public void Validation_IndigoSMILESloader()
        {
            Assert.Inconclusive("test not written");
        }

        /// <summary>
        /// 200.17 = Indigo kekulization
        /// 2005-09-15: Deactivated because I can't find any examples in practice.
        /// </summary>
        //[TestMethod]
        public void Validation_IndigoKekulization()
        {
            Assert.Inconclusive("test not written");
        }

        /// <summary>
        /// 200.18 = Indigo automorphism search timeout
        /// 2005-09-15: Deactivated because I can't find any examples in practice.
        /// </summary>
        //[TestMethod]
        public void Validation_IndigoAutomorphismSearchTimeout()
        {
            Assert.Inconclusive("test not written");
        }

        /// Issue code 200.19 = bad valence (indigo) is not used anywhere in the program.
        /// <summary>
        /// 200.20 = ambiguous hydrogen (indigo)
        /// 2005-09-15: Deactivated because I can't find any examples in practice.
        /// </summary>
        //[TestMethod]
        public void Validation_IndigoAmbiguousH()
        {
            var codes = Validation.Validate(Resource1.borazine_nonunique).Issues.Select(c => c.Code);
            Assert.IsTrue(codes.Contains("200.20"), String.Join("; ", codes));
        }
    }
}