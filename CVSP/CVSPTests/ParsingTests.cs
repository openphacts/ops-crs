using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSC.CVSP;
using RSC.CVSP.Compounds;
using RSC.Logging;

namespace CVSPTests
{
    [TestClass]
    public class ParsingTests : CVSPTestBase
    {
        /// 300.1 = Structure generation from synonym failed.
        /// 2015-09-16: this is not covered by existing code
        /// 
        /// 300.2 = Structure generation from SMILES failed.
        /// 2015-09-16: this is not covered by existing code.
        /// 
        /// 300.3 = Structure generation from InChI failed.
        /// 2015-09-16: this is not covered by existing code

        /// <summary>
        /// 300.4 = Header line missing.
        /// </summary>
        [TestMethod]
        public void Parsing_HeaderLineMissing()
        {
            var codes = Validation.Validate(Resource1.molHeaderLineMissing).Issues.Select(c => c.Code);
            Assert.IsTrue(codes.Contains("300.4"), "missing header line should be detected");
        }

        /// <summary>
        /// 300.5 = 'M END' missing.
        /// </summary>
        [TestMethod]
        public void Parsing_M_END_Missing()
        {
            var codes = Validation.Validate("sdjoifiosdjfodjfosidj").Issues.Select(c => c.Code);
            Assert.IsTrue(codes.Contains("300.5"), "'M END' being missing should be detected");
        }

        /// <summary>
        /// 300.6 = Unable to interpret line.
        /// </summary>
        [TestMethod]
        public void Parsing_UnableToInterpretLine()
        {
            string outsdf;
            List<Issue> issues = new List<Issue>();
            Assert.IsFalse(ValidationModule.IsMolfileFormatValid(Resource1.benzeneUninterpretableLine, out outsdf, true, issues));

            var codes = issues.Select(c => c.Code);
            foreach (var c in codes) Console.WriteLine(c);
            Assert.IsTrue(codes.Contains("300.6"), "uninterpretable line should be detected");
        }

        /// <summary>
        /// 300.7 = Peculiar atom coordinate.
        /// </summary>
        [TestMethod]
        public void Parsing_PeculiarAtomCoordinate()
        {
            string outsdf;
            List<Issue> issues = new List<Issue>();
            Assert.IsTrue(ValidationModule.IsMolfileFormatValid(Resource1.benzenePeculiarCoordinates, out outsdf, true, issues));
            foreach (var i in issues) Console.WriteLine(i.Code);
            Assert.IsTrue(issues.Select(c => c.Code).Contains("300.7"), "peculiar coordinates (inner test)"); 
            var codes = Validation.Validate(Resource1.benzenePeculiarCoordinates).Issues.Select(c => c.Code);
            foreach (var c in codes) Console.WriteLine(c);
            Assert.IsTrue(codes.Contains("300.7"), "peculiar coordinates (outer test)");
        }

        /// <summary>
        /// 500.1 = Processing operation failed.
        /// Test has to go somewhere!
        /// TODO 2015-09-25: to be written
        /// </summary>
        //[TestMethod]
        public void Processing_OperationFailed()
        {
            Assert.Inconclusive("no idea how to test this");
        }
    }
}