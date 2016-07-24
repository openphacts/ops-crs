using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSC.CVSP.Compounds;
using com.ggasoftware.indigo;
using InChINet;

namespace CVSPTests
{
    /// <summary>
    /// 2015-08-27.
    /// Test now in RSCCheminfToolkit (may be modified significantly as part of refactoring).
    /// </summary>
    [TestClass]
    public class CheminformaticsIndigo
    {
        /// <summary>
        /// indigo's inchi plugin treats absolute stereo as relative in case of non standard inchi
        /// reported to indigo https://groups.google.com/forum/#!topic/indigo-bugs/g21U28bFx4U
        /// </summary>
        [TestMethod]
        public void CheminformaticsIndigo_nonStdInChIforAbsoluteStereo()
        {
            string mol = Resource1.chiralflagset;
            Indigo indigo = new Indigo();
            IndigoInchi i_inchi = new IndigoInchi(indigo);
            IndigoObject obj = indigo.loadMolecule(mol);
            indigo.setOption("inchi-options", "/FixedH /SUU /SLUUD /SUCF");
            string non_std_inchi = i_inchi.getInchi(obj);

            string orig_nonStdInchi = InChIUtils.mol2InChI(mol, InChIFlags.CRS);
            string orig_StdInchi = InChIUtils.mol2InChI(mol,InChIFlags.Standard);
            //Assert.IsFalse(non_std_inchi.Equals("InChI=1/C16H14O6/c17-10-2-1-8-13-9-4-12(19)11(18)3-7(9)5-16(13,21)6-22-15(8)14(10)20/h1-4,13,17-21H,5-6H2/t13-,16+/s2"), "inchi should have been generated with absolute stereo");
            Assert.IsTrue(orig_nonStdInchi.Equals("InChI=1/C16H14O6/c17-10-2-1-8-13-9-4-12(19)11(18)3-7(9)5-16(13,21)6-22-15(8)14(10)20/h1-4,13,17-21H,5-6H2/t13-,16+/m1/s1"), "inchi is not what expected");
            Assert.IsTrue(orig_StdInchi.Equals("InChI=1S/C16H14O6/c17-10-2-1-8-13-9-4-12(19)11(18)3-7(9)5-16(13,21)6-22-15(8)14(10)20/h1-4,13,17-21H,5-6H2/t13-,16+/m1/s1"), "std inchi is not what expected");
            //InChIUtils.mol2InChI(mol,InChIFlags.InChICustom.FixedH | InChIFlags.InChICustom.SUU | InChIFlags.InChICustom.SLUUD);
            
            //check that original inchi is generating correctly

        }
    }
}
