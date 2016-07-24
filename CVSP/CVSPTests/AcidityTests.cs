using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSC.CVSP.Compounds;
using MoleculeObjects;

namespace CVSPTests
{
    [TestClass]
    public class AcidityTests
    {
        [TestMethod]
        public void Acidity_ConstructorTest()
        {
            Assert.IsTrue(ValidationAcidityStandardizationInitializer.acidity.SubstructureTransformMapping.Any(), "No substructure--transform mappings found in database");
        }

        [TestMethod]
        public void Acidity_SulfoVsCarboxyTest()
        {
            Molecule good = MoleculeFactory.Molecule(Resource1._14i_goodSodiumSulfosalicylate);
            Molecule bad = MoleculeFactory.Molecule(Resource1._14i_badSodiumSulfosalicylate);

            var goodresults = ValidationAcidityStandardizationInitializer.acidity.AcidBaseMatches(good);
            var badresults = ValidationAcidityStandardizationInitializer.acidity.AcidBaseMatches(bad);
            Assert.AreEqual(2, goodresults.Where(r => r.Item2.Contains(ProtonationState.Acid)).Count());
            Assert.AreEqual(1, badresults.Where(r => r.Item2.Contains(ProtonationState.Base)).Count());
            int firstacidgood = goodresults.Where(r => r.Item2.Contains(ProtonationState.Acid)).First().Item1;
            int lastbasegood = goodresults.Where(r => r.Item2.Contains(ProtonationState.Base)).Last().Item1;
            Assert.IsTrue(lastbasegood < firstacidgood);
            int firstacidbad = badresults.Where(r => r.Item2.Contains(ProtonationState.Acid)).First().Item1;
            int lastbasebad = badresults.Where(r => r.Item2.Contains(ProtonationState.Base)).Last().Item1;
            Assert.IsTrue(lastbasebad > firstacidbad);
        }

        [TestMethod]
        public void Acidity_WeakerAcidIonizedBeforeStrongerAcid()
        {
            Molecule good = MoleculeFactory.Molecule(Resource1._14i_goodSodiumSulfosalicylate);
            Molecule bad = MoleculeFactory.Molecule(Resource1._14i_badSodiumSulfosalicylate);
            Acidity a = ValidationAcidityStandardizationInitializer.acidity;
            Assert.IsTrue(a.WeakerAcidIonizedBeforeStrongerAcid(bad));
            Assert.IsFalse(a.WeakerAcidIonizedBeforeStrongerAcid(good));
        }

        [TestMethod]
        public void Acidity_AcidBaseSMIRKS()
        {
            Acidity a = ValidationAcidityStandardizationInitializer.acidity;
            var badMonad = new CtMonad(Resource1._14i_badSodiumSulfosalicylate, new List<RSC.Issue>(), new List<string>());
            var badResult = a.AcidBaseSMIRKS(badMonad);
            Assert.AreEqual(2, badResult.Count);
            foreach (string smirks in badResult.Values) Assert.IsTrue(smirks.Contains(">>"), "invalid SMIRKS " + smirks);
            var goodMonad = new CtMonad(Resource1._14i_goodSodiumSulfosalicylate, new List<RSC.Issue>(), new List<string>());
            var goodResult = a.AcidBaseSMIRKS(goodMonad);
            Assert.AreEqual(0, goodResult.Count);

        }
    }
}
