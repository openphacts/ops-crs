using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using RSC.CVSP.Compounds;
using MoleculeObjects;
using com.ggasoftware.indigo;
using RSC.CVSP;

namespace CVSPTests
{
	[TestClass]
	public class ValidationSmartsPattern : CVSPTestBase
	{
        [TestMethod]
        public void Validation_FreeBTest()
        {
            Indigo i = new Indigo();
            string badB = i.loadMolecule("[B].c1ccccc1").molfile();
            var codes = Validation.Validate(badB).Issues.Select(c => c.Code);
            string goodB = i.loadMolecule("[B]").molfile();
            var goodcodes = Validation.Validate(goodB).Issues.Select(c => c.Code);
            Assert.IsFalse(goodcodes.Contains("100.71"), "codes returned = " + String.Join("; ", goodcodes));
            Assert.IsTrue(codes.Contains("100.71"), "codes returned = " + String.Join("; ", codes));
        }

        [TestMethod]
        public void Validation_FreeSTest()
        {
            Indigo i = new Indigo();
            string badS = i.loadMolecule("[S].c1ccccc1").molfile();
            var codes = Validation.Validate(badS).Issues.Select(c => c.Code);
            string goodS = i.loadMolecule("[S]").molfile();
            var goodcodes = Validation.Validate(goodS).Issues.Select(c => c.Code);
            Assert.IsTrue(codes.Contains("100.71"));
            Assert.IsFalse(goodcodes.Contains("100.71"));
        }

        [TestMethod]
        public void ValidationModules_UnkekulizableEnol()
        {
            Assert.IsTrue(ValidationModule.ContainsNotKekulizedAromaticRings(Resource1.enol));
        }
	}
}
