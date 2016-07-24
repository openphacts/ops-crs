using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSC.CVSP.Compounds;
using com.ggasoftware.indigo;
using InChINet;
using RSC.CVSP;

namespace CVSPTests
{
	[TestClass]
	public class StandardizationPartialIonization : CVSPTestBase
	{
		[TestMethod]
        public void StandardizationPartialIonization_SodiumSulfateThiol()
		{
            string input = Resource1.partialIonization_sodiumSulfateThiol;
			Indigo i = new Indigo();
			string out_mol = Standardization.StandardizeByAcidBaseSMIRKS(input);
			IndigoObject output = i.loadMolecule(out_mol);
			Assert.AreEqual(@"[Na+].CC(CC(=O)OC)C1=CC(=CC(S)=C1)C(O)OS([O-])(=O)=O", output.canonicalSmiles());
		}

		[TestMethod]
		public void StandardizationPartialIonization_SulfateSulfonate()
		{
            string mol = Resource1.partialIonization_sulfateSulfonate;
			string res = Standardization.StandardizeByAcidBaseSMIRKS(mol);
			string nonstdinchi_output = InChIUtils.mol2InChI(res, InChIFlags.CRS);
            Assert.AreEqual(@"InChI=1/C6H6O7S2/c7-14(8,9)6-3-1-5(2-4-6)13-15(10,11)12/h1-4H,(H,7,8,9)(H,10,11,12)/p-1/fC6H5O7S2/h7H/q-1",
                nonstdinchi_output);
		}

		[TestMethod]
        public void StandardizationPartialIonization_SulfateThiol()
        {
            string mol = Resource1.partialIonization_sulfateThiol;
			Indigo i = new Indigo();
			List<string> Transformations = new List<string>();
			string res = Standardization.StandardizeByAcidBaseSMIRKS(mol);
			IndigoObject output = i.loadMolecule(res);
			Assert.AreEqual(@"[O-]S(=O)(=O)OC1C=CC(S)=CC=1", output.canonicalSmiles());
		}

		[TestMethod]
		public void StandardizationPartialIonization_SulfateMethyl()
        {
            string mol = Resource1.partialIonization_sulfateMethyl;
			Indigo i = new Indigo();
			List<string> Transformations = new List<string>();
			string res = Standardization.StandardizeByAcidBaseSMIRKS(mol);
			IndigoObject output = i.loadMolecule(res);
			Assert.AreEqual(@"CC1NC(C)=NC=1OS([O-])(=O)=O", output.canonicalSmiles());
		}

		[TestMethod]
		public void StandardizationPartialIonization_AmineAmide()
		{
            string mol = Resource1.partialIonization_amineAmide;
			List<string> Transformations = new List<string>();
			string res = Standardization.StandardizeByAcidBaseSMIRKS(mol);
			Indigo i = new Indigo();
            string smiles = i.loadMolecule(res).canonicalSmiles();
            Assert.AreEqual(@"NC(=O)C1=CN(C=C[C@H]1N1C=C2C(=S)N(C=NC2=N1)[C@H](C1C=CC=CC=1)C(=O)N1CCCCCC1)[C@@H]1O[C@@H](CO[P@]([O-])(=O)O[P@@]([O-])(=O)OC[C@@H]2O[C@H]([C@H]([O-])[C@H]2[O-])N2C=NC3=C2N=CN=C3N)[C@H](O)[C@H]1O",
                smiles);
		}
	}
}
