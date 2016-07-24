using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MoleculeObjects;
using RSC.CVSP;
using RSC.CVSP.Compounds;
using com.ggasoftware.indigo;
using RSC.Logging;

namespace CVSPTests
{
    [TestClass]
    public class ValidationTests : CVSPTestBase
    {
        static Indigo i;
        static Dictionary<string, string> sdTagMap;

		[TestInitialize()]
		public void Initialize()
		{
            i = new Indigo();
            sdTagMap = new Dictionary<string, string>();
            sdTagMap.Add("Name", "CHEMSPIDER_DEPOSITOR_SUBSTANCE_NAME");
            sdTagMap.Add("Synonyms", "CHEMSPIDER_DEPOSITOR_SUBSTANCE_SYNONYM");
            sdTagMap.Add("Id", "CHEMSPIDER_DEPOSITOR_SUBSTANCE_ID");
            sdTagMap.Add("MY_SMILES", "CHEMSPIDER_DEPOSITOR_SUBSTANCE_SMILES");
            sdTagMap.Add("MY_INCHI", "CHEMSPIDER_DEPOSITOR_SUBSTANCE_INCHI");
            sdTagMap.Add("MySubstanceUrl", "CHEMSPIDER_DEPOSITOR_SUBSTANCE_URL");
            sdTagMap.Add("MyHomePage", "CHEMSPIDER_DATASOURCE_URL");
		}

        [TestMethod]
        public void Acidity_GetSubstructureTransformMapping()
        {
			Assert.IsTrue(Acidity.GetSubstructureTransformMapping().Any(), "No substructure--transform mappings found in database");
        }

        [TestMethod]
        public void Acidity_SulfoVsCarboxyTest()
        {
            Molecule good = MoleculeFactory.Molecule(Resource1._14i_goodSodiumSulfosalicylate);
            Molecule bad = MoleculeFactory.Molecule(Resource1._14i_badSodiumSulfosalicylate);

            var goodresults = Acidity.AcidBaseMatches(good);
            var badresults = Acidity.AcidBaseMatches(bad);

            Assert.AreEqual(2, goodresults.Where(r => r.Item2.Contains(ProtonationState.Acid)).Count(), "should be two acid groups");
            Assert.AreEqual(1, badresults.Where(r => r.Item2.Contains(ProtonationState.Base)).Count(), "should be one base group");
            int firstacidgood = goodresults.First(r => r.Item2.Contains(ProtonationState.Acid)).Item1;
            int lastbasegood = goodresults.Last(r => r.Item2.Contains(ProtonationState.Base)).Item1;
            Assert.IsTrue(lastbasegood < firstacidgood, "wrong ionizability order in the good molecule");
            int firstacidbad = badresults.First(r => r.Item2.Contains(ProtonationState.Acid)).Item1;
            int lastbasebad = badresults.Last(r => r.Item2.Contains(ProtonationState.Base)).Item1;
            Assert.IsTrue(lastbasebad > firstacidbad, "wrong ionizability order in the bad molecule");
        }

        [TestMethod]
        public void Validation_NotReallyNeutralMoleculesTest()
        {
            Molecule m = MoleculeFactory.Molecule(Resource1._8i_goodAmmoniumLactate);
            Assert.IsFalse(m.AllNeutralMolecules());
            Molecule b = MoleculeFactory.Molecule(Resource1.benzeneAsMol);
            Assert.IsTrue(b.AllNeutralMolecules());
        }

        [TestMethod]
        public void Validation_ChainTests()
        {
            var FOOOOFanalysis = Validation.Validate(Resource1.FOOOOF).Issues.Select(i => i.Code);
            Assert.IsTrue(FOOOOFanalysis.Contains("100.71"), "FOOOOF should raise a warning");
            var fivenitrogenanalysis = Validation.Validate(Resource1.pentahydropentazole).Issues.Select(i => i.Code);
            Assert.IsTrue(fivenitrogenanalysis.Contains("100.70"), "pentahydropentazole should raise an information issue");
        }

        [TestMethod]
        public void ValidationModule_ContainsUnevenlengthBonds()
        {
            Assert.IsTrue(ValidationModule.ContainsUnevenLengthBonds(Resource1.congestedsdf));
            Assert.IsFalse(ValidationModule.ContainsUnevenLengthBonds(Resource1.benzeneAsMol));
        }

        [TestMethod]
        public void ValidationModule_ContainsFreeCO()
        {
            string freeCO = i.loadMolecule("[Fe].[C-]#[O+].[C-]#[O+]").molfile();
            Assert.IsTrue(ValidationModule.ContainsMixtureWithFreeCO(freeCO));
        }

        [TestMethod]
        public void ValidationModule_TryGeneratingStdInChI()
        {
            string stdInChI;
            Assert.IsTrue(ValidationModule.TryGeneratingStdInChI(Resource1.relativestereo, out stdInChI));
            Assert.AreEqual("InChI=1S/C13H17FN2O3S/c1-2-13(17)15-11-7-8-16(9-11)20(18,19)12-5-3-10(14)4-6-12/h3-6,11H,2,7-9H2,1H3,(H,15,17)/t11-/m1/s1", stdInChI);
            Assert.IsFalse(ValidationModule.TryGeneratingStdInChI("wsdfoisjdfiojsdofijsdoifj", out stdInChI)); 
        }

        [TestMethod]
        public void ValidationModule_CanIndigoGenerateSMILES()
        {
            string smiles;
            Assert.IsTrue(ValidationModule.CanIndigoGenerateSmiles(Resource1.relativestereo, out smiles));
            Assert.IsFalse(ValidationModule.CanIndigoGenerateSmiles("sdfjiosdjoidjoisdjfoisdfjiosd", out smiles));
        }

        [TestMethod]
        public void ValidationModule_CanIndigoGenerateCanonicalSMILES()
        {
            string smiles;
            Assert.IsTrue(ValidationModule.CanIndigoGenerateCanonicalSmiles(Resource1.relativestereo, out smiles));
            Assert.IsFalse(ValidationModule.CanIndigoGenerateCanonicalSmiles("sdfjiosdjoidjoisdjfoisdfjiosd", out smiles));
        }

        [TestMethod]
        public void ValidationModule_ContainsRelativeStereoInV2000()
        {
            Assert.IsTrue(ValidationModule.ContainsRelativeStereoInV2000(Resource1.relativestereo, new List<Issue>()));
            Assert.IsFalse(ValidationModule.ContainsRelativeStereoInV2000(Resource1.flatsugar, new List<Issue>()));
        }

        [TestMethod]
        public void ValidationModule_DoesAtomCountExceedMaximum()
        {
            Assert.IsTrue(ValidationModule.DoesAtomCountExceedMaximum(Resource1.benzeneAsMol, 5));
            Assert.IsFalse(ValidationModule.DoesAtomCountExceedMaximum(Resource1.benzeneAsMol, 100000));
        }

        [TestMethod]
        public void Validation_Electron()
        {
            var result = Validation.Validate(Resource1.electron).Issues;
            Assert.IsTrue(result.Select(c => c.Code).Contains("100.4"), "an electron on an atom line is non-InChIfiable");
        }

        [TestMethod]
        public void Validation_Iodine124()
        {
            string mol = Resource1.iodine124;
            var codes = Validation.Validate(mol).Issues.Select(i => i.Code);
            Assert.IsFalse(codes.Any(), "wrong isotope should not pose any problems");
        }

        [TestMethod]
        public void ValidationModule_AmbiguousH()
        {
            Assert.IsTrue(ValidationModule.IndigoIsAmbiguousHydrogenPresent(Resource1.borazine_nonunique, new List<Issue>()));
            Assert.IsFalse(ValidationModule.IndigoIsAmbiguousHydrogenPresent(Resource1.flatsugar, new List<Issue>()));
        }

        [TestMethod]
        public void ValidationModule_StereoCountTest()
        {
            AnalysedInChI noExpected = ValidationModule.AnalyzeStereo(Resource1.noexpectedstereo);
            AnalysedInChI missingExpected = ValidationModule.AnalyzeStereo(Resource1.missingexpectedstereo);
            AnalysedInChI unknownExpected = ValidationModule.AnalyzeStereo(Resource1.unknownstereo);
            Assert.AreEqual(noExpected.DefinedStereoCenters, 0,
                String.Format("problem with none expected: {0}", noExpected.DefinedStereoCenters));
            Assert.AreEqual(missingExpected.UndefinedStereoCenters, 4,
                String.Format("problem with none expected: {0}", missingExpected.UndefinedStereoCenters));
            Assert.AreEqual(unknownExpected.UnknownStereoCenters, 1,
                String.Format("problem with none expected: {0}", unknownExpected.UnknownStereoCenters));
            Assert.AreEqual(unknownExpected.UnknownStereoBond, 2,
                String.Format("problem with none expected: {0}", unknownExpected.UnknownStereoBond));
        }

        [TestMethod]
        public void ValidationModule_ContainsDuplicatesTest()
        {
            Assert.IsTrue(ValidationModule.ContainsDuplicateMolecules(Resource1.multiplebenzene));
        }

        [TestMethod]
        public void ValidationRadicals_TenRadicals()
        {
            string sdf = Resource1.tenradicalcentres;
            var result = Validation.Validate(sdf);
            Assert.IsNotNull(result, "result should not be null");
            Assert.IsTrue(result.Issues.Select(i => i.Code).Contains("100.56"));
        }

        [TestMethod]
        public void ValidationModule_IsAtomCountZero()
        {
            Assert.IsTrue(ValidationModule.IsAtomCountZero(Resource1.empty, new List<Issue>()));
            Assert.IsFalse(ValidationModule.IsAtomCountZero(Resource1.benzeneAsMol, new List<Issue>()));
        }

        [TestMethod]
        public void ValidationModule_ContainsEitherBond()
        {
            Assert.IsTrue(ValidationModule.ContainsEitherBond(Resource1.eitherbond_molecule));
            Assert.IsFalse(ValidationModule.ContainsEitherBond(Resource1.flatsugar));
        }

        [TestMethod]
        public void ValidationModule_ContainsRadicals()
        {
            Assert.IsTrue(ValidationModule.ContainsRadicals(Resource1.tenradicalcentres, new List<Issue>()));
            Assert.IsFalse(ValidationModule.ContainsRadicals(Resource1.flatsugar, new List<Issue>()));
        }

        [TestMethod]
        public void ValidationModule_ContainsSmarts()
        {
            Assert.IsTrue(ValidationModule.ContainsSmarts(Resource1.flatsugar, "O~C~C~C~C~C~O"));
            Assert.IsFalse(ValidationModule.ContainsSmarts(Resource1.flatsugar, "O~C~C~N~C~C~O"));
        }

        [TestMethod]
        public void ValidationModule_ConvertV3000toV2000()
        {
            string result = ValidationModule.ConvertV3000ToV2000(Resource1.benzeneV3000);
            Assert.AreEqual("C1C=CC=CC=1", i.loadMolecule(result).canonicalSmiles());
            string r2 = ValidationModule.ConvertV3000ToV2000("djfidjfidjfi");
            Assert.IsNull(r2);
        }

        [TestMethod]
        public void ValidationModule_IsMolfileFormatValid()
        {
            string result;
            List<Issue> issues = new List<Issue>();
            bool v2000 = ValidationModule.IsMolfileFormatValid(Resource1.benzeneAsMol.Trim(), out result, false, issues);
            foreach (var i in issues) Console.WriteLine(i.Code + " line ='" + i.AuxInfo + "'");
            Assert.IsTrue(v2000);
            Assert.IsTrue(ValidationModule.IsMolfileFormatValid(Resource1.benzeneV3000, out result));
            Assert.IsFalse(ValidationModule.IsMolfileFormatValid("C1C=CC==C1", out result));
            Assert.IsFalse(ValidationModule.IsMolfileFormatValid("sjdofisodifjsiodfjosdj", out result));
        }

        [TestMethod]
        public void ValidationModule_CountRadicalCenters()
        {
            Assert.AreEqual(10, ValidationModule.CountRadicalCenters(Resource1.tenradicalcentres, new List<Issue>()));
            Assert.AreEqual(0, ValidationModule.CountRadicalCenters(Resource1.flatsugar, new List<Issue>()));
        }

        [TestMethod]
        public void ValidationModule_IsV3000()
        {
            Assert.IsTrue(ValidationModule.IsV3000(Resource1.v3000example));
            Assert.IsFalse(ValidationModule.IsV3000(Resource1.flatsugar));
            Assert.IsTrue(ValidationModule.IsV3000(Resource1.benzeneV3000));
        }

        [TestMethod]
        public void ValidationModule_IsOverallSystemCharged()
        {
            Assert.IsTrue(ValidationModule.IsOverallSystemCharged(i.loadMolecule("CCCCCCCCCCCC[C-]").molfile()));
            Assert.IsFalse(ValidationModule.IsOverallSystemCharged(Resource1.flatsugar));
        }

        [TestMethod]
        public void ValidationModule_TryLoadingSdfToIndigo()
        {
            IndigoObject result = ValidationModule.TryLoadingSdfToIndigo(Resource1.flatsugar, new List<Issue>());
            Assert.AreEqual("OCC1OC(O)C(O)C(O)C1O", result.canonicalSmiles());
            IndigoObject result2 = ValidationModule.TryLoadingSdfToIndigo(Resource1.v3000example, new List<Issue>());
            Assert.AreEqual("CC(C)[C@@H](C)CC[C@H](C)[C@@H]1CCC(=O)[C@H]2C[C@@H]3C(=C)[C@H](O)CC[C@@]3(C)C3CC[C@]1(C)C=32 |&1:3,7,9,14,16,19,23,28|",
                result2.canonicalSmiles());
        }

        /* TODO: Investigate!
        [TestMethod]
        public void Validation_SodiumAcetateTest()
        {
            Warnings(Validation, Resource1._7_goodNaOAc, Resource1._7_badNaOAc);
            Warnings(Validation, Resource1._7_goodNaOAc, Resource1._7_badNaOAcNeutral);
        }
        */


        /* - need to simplify test
        [TestMethod]
        public void Validation_AmmoniumTests()
        {
            CSMolecule fpAmmonia = RemoteMoleculeFactory.CSMolecule(Resource1.ammoniaFalsePositive);
            var analysis = from t in ValidationAcidityStandardizationInitializer.s_instance.validation.Analyse(fpAmmonia) where t.Item1 == IssueSeverity.Warning select t.Item2;
            Assert.IsTrue(analysis.Count() == 0, String.Join("; ", from a in analysis select a.Title));
            Warnings( ChemValidatorLib.ValidationAcidityStandardizationInitializer.s_instance.validation, Resource1._8i_goodAmmoniumChloride, Resource1._8i_badAmmoniumChloride);
            Warnings( ChemValidatorLib.ValidationAcidityStandardizationInitializer.s_instance.validation, Resource1._8i_goodAmmoniumLactate, Resource1._8i_badAmmoniumLactate);
            Warnings( ChemValidatorLib.ValidationAcidityStandardizationInitializer.s_instance.validation, Resource1._8iii_goodQuaternaryAmmonium, Resource1._8iii_badQuaternaryAmmonium);
            Warnings( ChemValidatorLib.ValidationAcidityStandardizationInitializer.s_instance.validation, Resource1._9i_goodNEt3HCl, 1, Resource1._9i_badNEt3HCl, 0);
        }
        */
        /*
         * need to reevaluate how these FDA-type ones fit in with what we're doing now...
        [TestMethod]
        public void Validation_GuanidineTests()
        {
            Warnings(Validation, Resource1._9ii_goodAminoguanidineSulfate, Resource1._9ii_badAminoguanidineSulfate);
        }

        [TestMethod]
        public void Validation_MercuryBondsTest()
        {
            Warnings(Validation, Resource1._19bi_goodthimerosal, Resource1._19bii_badthimerosal);
        }

        [TestMethod]
        public void Validation_KetoEnolTest()
        {
            Warnings( Validation, Resource1._20b_goodAcetophenone, Resource1._20b_badAcetophenone);
            CSMolecule distractor = RemoteMoleculeFactory.CSMolecule(Resource1._20b_distractor);
            var dWarnings = from t in Validation.Analyse(distractor) where t.Item1 == Severity.Warning select t.Item2;
            Assert.IsTrue(dWarnings.Count() == 0, String.Join("; ", from a in dWarnings select a.Message));
        }

        [TestMethod]
        public void Validation_AzacarbonylTest()
        {
            Warnings( Validation, Resource1._20c_goodPhenobarbital, Resource1._20c_badPhenobarbital);
        }

        [TestMethod]
        public void Validation_OximeTest()
        {
            Warnings( Validation, Resource1._20e_goodBenzaldoxime, Resource1._20e_badBenzaldoxime);
        }


        [TestMethod]
        public void Validation_TetrazoleTest()
        {
            Warnings( Validation, Resource1._20g_goodTetrazole, Resource1._20g_badTetrazole);
        }
        */

        [TestMethod]
        public void Validation_NonInChIfiableAtomsTest()
        {
            List<Issue> issues = new List<Issue>();
            Cdx cdx = new Cdx(Resource1.bigGeneric);
            string sdf = String.Concat(cdx.ToMolFiles());
			ValidationGenericMolecule vm = new ValidationGenericMolecule(sdf, issues);
            var fatalErrors = vm.runTests();
            Assert.IsTrue((from i in fatalErrors join  et in LogManager.Logger.EntryTypes on i.Code equals et.Code where et.Severity == Severity.Error select i).Any());

			vm = new ValidationGenericMolecule(Resource1.cyclohexylcompound, issues);
			fatalErrors = vm.runTests();
			Assert.IsTrue((from i in fatalErrors join  et in LogManager.Logger.EntryTypes on i.Code equals et.Code where et.Severity == Severity.Error select i).Any());

			vm = new ValidationGenericMolecule(Resource1._1_methylphenylhydrazine, issues);
			fatalErrors = vm.runTests();
			Assert.IsTrue((from i in fatalErrors join  et in LogManager.Logger.EntryTypes on i.Code equals et.Code where et.Severity == Severity.Error select i).Any());

			//GenericMolecule cyclohexylR = MoleculeFactory.FromMolV2000(Resource1.cyclohexylcompound);
			//var cyclohexylErrors =  Validation.Analyse(cyclohexylR);
			//Assert.IsTrue(cyclohexylErrors.Any());

			//GenericMolecule methylphenylhydrazine = MoleculeFactory.FromMolV2000(Resource1._1_methylphenylhydrazine);
			//var mphErrors =  Validation.Analyse(methylphenylhydrazine);
			//Assert.IsTrue(mphErrors.Any());
        }

        /// <summary>
        /// This molecule containing deuterium atoms as D should pass validation.
        /// </summary>
        [TestMethod]
        public void Validation_DeuteriumAsD()
        {
            var codes = Validation.Validate(Resource1.deuteratedmolecule).Issues.Select(c => c.Code)
                .Where(c => c != "100.64");
            Assert.IsFalse(codes.Any(), "deuterated molecule should not fail validation");
        }

        /// <summary>
        /// This mol file (ValidationResources.substitutedatommolecule) contains stuff we can consider supporting.
        /// </summary>
        [TestMethod]
        public void Validation_DuplicateIssues()
        {
            var issues = Validation.Validate(Resource1.substitutedatommolecule).Issues;
            Assert.AreEqual(issues.Distinct().Count(), issues.Count(), "duplicate issues");
        }

        [TestMethod]
        public void ValidationModules_ContainsDuplicateMolecules()
        {
            Assert.IsTrue(ValidationModule.ContainsDuplicateMolecules(Resource1.multiplebenzene));
        }

        [TestMethod]
		public void Validation_NoProblemsMolecule()
        {
            var valid_dt = Validation.Validate(Resource1.noproblemsmolecule).Issues;
            Assert.AreEqual(0, valid_dt.Count());
		}

        [TestMethod]
        public void ValidationModules_ContainsOnlyDuplicateMolecules()
        {
            List<Issue> issues = new List<Issue>();
            Assert.IsTrue(ValidationModule.ContainsOnlyMultipleInstancesOfSameMolecules(Resource1.multiplebenzene, issues));
        }

		[TestMethod]
		public void Validation_epoxy_r_stereo()
		{
			var mol = Resource1.epoxystereo;
            var issues = Validation.Validate(mol).Issues;
            var codes = issues.Select(i => i.Code);
            Assert.IsFalse(codes.Contains("100.69"), "validation should not fail");
		}
	}
}