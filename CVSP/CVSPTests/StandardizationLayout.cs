using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Collections.Generic;

using RSC.CVSP.Compounds;
using MoleculeObjects;
using com.ggasoftware.indigo;
using RSC.CVSP;

namespace CVSPTests
{
    [TestClass]
    public class StandardizationLayout : CVSPTestBase
    {
        /// <summary>
        /// INdigo Layout affecting stereo on double bond: reported to GGA
        /// https://groups.google.com/forum/#!topic/indigo-bugs/2WJWti65vUk
        /// </summary>
        [TestMethod]
        public void Standardization_Layout()
        {
            bool StdInChIChanged;
            StandardizationModule.Layout(Resource1.rifampin, Resources.Vendor.OpenEye, out StdInChIChanged);
            Assert.IsTrue(StdInChIChanged, "Layout should return zero for this molecule");

            //rifampin CHEMBL374478
            string out_mol2 = StandardizationModule.Layout(Resource1.layoutfail, Resources.Vendor.OpenEye,  out StdInChIChanged);
            Assert.IsTrue(StdInChIChanged, "Layout should return zero for this molecule");
        }

        /// <summary>
        /// OE Layout affecting InChI. Swithcing to GGA
        /// </summary>
        [TestMethod]
        public void Standardization_OELayoutAffectingInChI()
        {
            bool StdInChIChanged;
            StandardizationModule.Layout(Resource1.layoutfail, Resources.Vendor.OpenEye, out StdInChIChanged);
            Assert.IsTrue(StdInChIChanged, "Layout should return zero for this molecule");
        }
    }
}
