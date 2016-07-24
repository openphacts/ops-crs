using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSC;
using RSC.Compounds;
using RSC.CVSP.Compounds;
using InChINet;

namespace CVSPTests
{
    [TestClass]
    public class ParentFactoryTests
    {
        [TestMethod]
        public void ParentFactory_GenerateParents()
        {
            CtMonad input = new CtMonad(Resource1._14i_goodSodiumSulfosalicylate, new List<Issue>(), new List<string>());
            var result = ParentFactory.GenerateParents(input);
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void ParentFactory_GenerateSuperParent()
        {
            CtMonad input = new CtMonad(Resource1._14i_goodSodiumSulfosalicylate, new List<Issue>(), new List<string>());
            var result = ParentFactory.GenerateSuperParent(input, true, InChIFlags.CRS);
            Assert.AreEqual(ParentChildRelationship.SuperInsensitive, result.Relationship, "wrong relationship type");
            Assert.AreEqual("[NaH].OC(=O)C1C=C(C=CC=1O)S(O)(=O)=O", result.Smiles);
        }
    }
}
