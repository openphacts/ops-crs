using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

using RSC.CVSP.Compounds;
using InChINet;
using com.ggasoftware.indigo;
using RSC.CVSP;
using RSC.Logging;

namespace CVSPTests
{
	[TestClass]
	public class StandardizationIsotope : CVSPTestBase
	{
		[TestMethod]
		public void Standardization_Iodine123()
		{
			List<Issue> issues = new List<Issue>();
			Indigo i = new Indigo();
			//string orig_smiles = i.loadMolecule(molfile).smiles();
			//string std_mol = StandardizationModulesMetals.DisconnectMetalsFromNonMetals(molfile, true);

			var result = Standardization.Standardize(Resource1.iodine123, Resources.Vendor.Indigo);

			string std_smiles = i.loadMolecule(result.Standardized).smiles();
			Assert.IsTrue(std_smiles.Equals("NC(NCC1C=C([123I])C=CC=1)=N"));
		}
	}
}
