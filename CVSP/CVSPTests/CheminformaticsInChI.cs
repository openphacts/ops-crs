using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSC.CVSP.Compounds;
using InChINet;

namespace CVSPTests
{
    /// <summary>
    /// 2015-08-27.
    /// Test now in RSCCheminfToolkit.
    /// </summary>
    [TestClass]
    public class CheminformaticsInChI
    {
        [TestMethod]
        public void CheminformaticsInChI_StdInChIandKey()
        {
            string[] std_inchi_array = InChIUtils.mol2inchiinfo(Resource1.chloronium, InChIFlags.CRS);
            string std_inchi = std_inchi_array[0];
            string std_inchi_key = std_inchi_array[1];
            Assert.IsTrue(std_inchi.Equals("InChI=1/C7H14ClFNS/c1-5(9)6-4-11-7(10-6)2-3-8/h5-8,10H,2-4H2,1H3/q+1/t5-,6?,7?/s2"));
            Assert.IsTrue(std_inchi_key.Equals("RVARFFRBSDYRFC-WJQSQFAPNA-N"));
        }
    }
}
