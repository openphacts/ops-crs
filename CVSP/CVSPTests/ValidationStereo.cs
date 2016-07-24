using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSC.CVSP.Compounds;
using RSC.CVSP;
using RSC;
using RSC.Logging;

namespace CVSPTests
{
    [TestClass]
    public class ValidationStereo : CVSPTestBase
    {
        [TestMethod]
        public void ValidationStereo_BadAllene()
        {
            Assert.IsTrue(ValidationStereoModule.ContainsBadAlleneStereo(Resource1.badallene));
            Assert.IsTrue(ValidationStereoModule.ContainsBadAlleneStereo(Resource1.badallene2));
            Assert.IsFalse(ValidationStereoModule.ContainsBadAlleneStereo(Resource1.goodallene));
        }

        [TestMethod]
        public void Validation_ThreePlainBondsAndAWedge()
        {
            Assert.IsTrue(ValidationStereoModule.Contains_3_PlainBondsAndStereoBond_ST_1_1_4(Resource1.st114mol));
            Assert.IsFalse(ValidationStereoModule.Contains_3_PlainBondsAndStereoBond_ST_1_1_4(Resource1.goodallene));
        }

        [TestMethod]
        public void ValidationStereo_FlatAndChiral()
        {
            string fakeChiral = StandardizationStereoModule.AddChiralFlag(Resource1.flatsugar);
            Assert.IsTrue(ValidationStereoModule.IsChiralFlagSet(fakeChiral));
            Assert.IsFalse(ValidationStereoModule.IsChiralFlagSet(Resource1.flatsugar));
            Assert.IsFalse(ValidationStereoModule.ContainsNoUpOrDownBondWithChiralFlag(Resource1.flatsugar));
            Assert.IsTrue(ValidationStereoModule.ContainsNoUpOrDownBondWithChiralFlag(fakeChiral));
        }

        [TestMethod]
        public void ValidationStereo_NoFlagButChiral()
        {
            Assert.IsFalse(ValidationStereoModule.ContainsUpAndDownBondsWithNoChiralFlag(Resource1.flatsugar));
            string fakeNotChiral = StandardizationStereoModule.RemoveChiralFlag(Resource1.degeneratestereocenters);
            Assert.IsTrue(ValidationStereoModule.ContainsUpAndDownBondsWithNoChiralFlag(fakeNotChiral));
        }
    }
}
