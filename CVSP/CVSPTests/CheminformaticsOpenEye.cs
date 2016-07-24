using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

using ChemSpider.Molecules;
using InChINet;
using OpenEyeNet;

namespace CVSPTests
{
    /// <summary>
    /// 2015-08-27: now in RSCCheminfToolkit.
    /// </summary>
    [TestClass]
    public class CheminformaticsOpenEye
    {
        /// <summary>
        /// If this passes, then everything is fine. If the bar turns white then there is a problem with the licence.
        /// </summary>
        [TestMethod]
        public void OpenEye_IsLicenceWorking()
        {
            OpenEyeUtility utility = OpenEyeUtility.GetInstance();
            string mol = utility.SMILESToMol("c1ccccc1");
            Assert.IsTrue(mol.Contains("-OEChem-"));
        }

        //should be fixed in OpenEyeUtility.GetInstance().SMILESToMol(smiles) 
        //that returns E stereo for both E- and Z-double bonds in smiles
        //[TestMethod]
        public void smiles2inchiTest()
        {
            string smiles1 = @"C1CCCCCC(=O)OCC/C=C\C1";
            
            string mol1 = MolUtils.SMILESToMol(smiles1);
            string inchi1 = InChIUtils.mol2inchiinfo(mol1, InChIFlags.Standard)[0];
            Assert.IsTrue(inchi1.Equals("InChI=1S/C12H20O2/c13-12-10-8-6-4-2-1-3-5-7-9-11-14-12/h5,7H,1-4,6,8-11H2/b7-5+"));
            
            string smiles2 = @"C1CCCCCC(=O)OCC/C=C/C1";
            string mol2 = MolUtils.SMILESToMol(smiles2);
            string inchi2 = InChIUtils.mol2inchiinfo(mol2, InChIFlags.Standard)[0];
            //Assert.IsTrue(inchi2.Equals("InChI=1S/C12H20O2/c13-12-10-8-6-4-2-1-3-5-7-9-11-14-12/h5,7H,1-4,6,8-11H2/b7-5-"));
        }
        
    }
}
