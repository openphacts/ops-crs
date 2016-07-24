using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSC.CVSP.Compounds;
using MoleculeObjects;
using com.ggasoftware.indigo;

namespace CVSPTests
{
    /// <summary>
    /// 2015-08-27: Not transferred to RSCCheminfToolkit as I (Colin) don't have licence.
    /// 2015-09-15: Deactivated until we find out what's going on about the licence.
    /// </summary>
    //[TestClass]
    public class CheminformaticsN2S
    {

        [TestMethod]
        public void N2S_Benzene()
        {
            string molfile = N2S.N2str(new List<string> { "benzene" });
            Indigo i = new Indigo();
            IndigoObject obj = i.loadMolecule(molfile);
            Assert.AreEqual(obj.canonicalSmiles(), "C1C=CC=CC=1");
        }

        [TestMethod]
        public void N2S_NoDoubleBondStereoInName()
        {
            string molfile = N2S.N2str(new List<string> { "1-chloro-3-ethylpent-1-en-4-yn-3-ol" });
            
            Indigo i = new Indigo();
            IndigoObject obj = i.loadMolecule(molfile);
            Assert.AreEqual(obj.canonicalSmiles(), "CCC(O)(C=CCl)C#C");
            
        }

		//[TestMethod]
		//public void NoDoubleBondStereoInName_WebServiceTestMethod_1()
		//{
		//	Molecule mol = RemoteMoleculeFactory.FromName("1-chloro-3-ethylpent-1-en-4-yn-3-ol");
            
		//	Indigo i = new Indigo();
		//	IndigoObject obj = i.loadMolecule(mol.ct());
		//	Assert.AreEqual(obj.canonicalSmiles(), "CCC(O)(C=CCl)C#C");

		//}

		//[TestMethod]
		//public void NoDoubleBondStereoInName_WebServiceTestMethod_2()
		//{
		//	Molecule mol = RemoteMoleculeFactory.FromName("n-ethyl-n-(2-methylphenyl)but-2-enamide ");

		//	Indigo i = new Indigo();
		//	IndigoObject obj = i.loadMolecule(mol.ct());
		//	Assert.AreEqual(obj.canonicalSmiles(), "CC1=CC=CC=C1N(CC)C(=O)C=CC");

		//}

        [TestMethod]
        public void N2S_DoubleBondStereoInName()
        {
            string molfile = N2S.N2str(new List<string> { "(1E)-1-chloro-3-ethylpent-1-en-4-yn-3-ol" });
            
            Indigo i = new Indigo();
            IndigoObject obj = i.loadMolecule(molfile);
            Assert.AreEqual(obj.canonicalSmiles(), "CCC(O)(/C=C/Cl)C#C");

        }
    }
}
