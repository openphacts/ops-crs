using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using com.ggasoftware.indigo;
using MoleculeObjects;
using RSC.CVSP;
using RSC.CVSP.Compounds;
using RSC.Logging;

namespace CVSPTests
{
    [TestClass]
    public class ValidationByCodes : CVSPTestBase
    {
        static Indigo i;
        static Dictionary<string, SDTagOptions> sdtagmap = new Dictionary<string, SDTagOptions>()
        {
            { "DEPOSITOR_SUBSTANCE_SYNONYM", SDTagOptions.DEPOSITOR_SUBSTANCE_SYNONYM },
            { "inchi", SDTagOptions.DEPOSITOR_SUBSTANCE_INCHI }
        };

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            i = new Indigo();
            i.setOption("ignore-stereochemistry-errors", "true");
        }

        public void PositiveSMILESTest(string smiles, string expectedCode)
        {
            IndigoObject obj = i.loadMolecule(smiles);
            obj.layout();
            var issues = Validation.Validate(obj.molfile()).Issues;
            Assert.IsTrue(issues.Select(c => c.Code).Contains(expectedCode), "validation should return code " + expectedCode
                + ": " + String.Join("; ", issues.Select(c => c.Code + " " + c.Message)));
        }

        public void NegativeSMILESTest(string smiles, string expectedCode)
        {
            IndigoObject obj = i.loadMolecule(smiles);
            obj.layout();
            var issues = Validation.Validate(obj.molfile()).Issues;
            Assert.IsFalse(issues.Select(c => c.Code).Contains(expectedCode), "validation should not return code " + expectedCode
                + ": " + String.Join("; ", issues.Select(c => c.Code + " " + c.Message)));
        }

        [TestMethod]
        public void Geometry_Angles()
        {
            var origin = Tuple.Create(0.0, 0.0, 0.0);
            var twelveOClock = Tuple.Create(0.0, 1.0, 0.0);
            var sixOClock = Tuple.Create(0.0, -1.0, 0.0);
            var threeOClock = Tuple.Create(1.0, 0.0, 0.0);
            Assert.AreEqual(180.0, Geometry.AngleBetweenThreePoints(origin, twelveOClock, sixOClock), 0.0001);
            Assert.AreEqual(0.0, Geometry.AngleBetweenThreePoints(origin, twelveOClock, twelveOClock), 0.0001);
            Assert.AreEqual(90.0, Geometry.AngleBetweenThreePoints(origin, twelveOClock, threeOClock), 0.0001);
            // and now test the one that uses doubles
            Assert.AreEqual(90.0, Geometry.AngleBetweenThreePoints(origin.Item1, origin.Item2, origin.Item3,
                threeOClock.Item1, threeOClock.Item2, threeOClock.Item3, sixOClock.Item1, sixOClock.Item2, sixOClock.Item3), 0.0001);
        }

        public void IndigoGoodValenceTest(string ct)
        {
            IndigoObject obj = i.loadMolecule(ct);
            Assert.IsTrue(String.IsNullOrEmpty(obj.checkBadValence()), "molecule should not have bad valence");
        }

        public void IndigoBadValenceTest(string ct, string expected)
        {
            IndigoObject obj = i.loadMolecule(ct);
            Assert.AreEqual(obj.checkBadValence(), expected);
        }

        public void TestGenericMolecule(string positiveExample, string negativeExample, string code)
        {
            List<Issue> issues = new List<Issue>();
            var positive = new ValidationGenericMolecule(positiveExample, issues);
            var pcodes = positive.runTests().Select(i => i.Code);
            Assert.IsTrue(pcodes.Contains(code),
                "positive example validation does not contain code " + code + ": " + String.Join("; ", pcodes));
            var negative = new ValidationGenericMolecule(negativeExample, issues);
            var ncodes = negative.runTests().Select(i => i.Code);
            Assert.IsFalse(ncodes.Contains(code),
                "negative example validation contains code " + code + ": " + String.Join("; ", ncodes));
        }

        public void TestValidation(string ctReturnsCode, string ctDoesNotReturnCode, string code, string message)
        {
            var failcodes = Validation.Validate(ctReturnsCode).Issues.Select(i => i.Code);
            Assert.IsTrue(failcodes.Any(), "validation should return issues (" + message + ")");
            Assert.IsTrue(failcodes.Contains(code), "should contain code " + code
                + " actual codes = " + String.Join("; ", failcodes));
            var passcodes = Validation.Validate(ctDoesNotReturnCode).Issues.Select(i => i.Code);
            Assert.IsFalse(passcodes.Contains(code), "should not contain code " + code
                 + " actual codes = " + String.Join("; ", failcodes));
        }

        /// Issue codes
        /// -------------------------
        /// 100.1 = validation failed
        /// This is a duplicate of 100.69, for which see later
        /// <summary>
        /// 100.2 = contains V3000
        /// </summary>
        [TestMethod]
        public void Validation_V3000()
        {
            var codes = Validation.Validate(Resource1.v3000example).Issues.Select(C => C.Code);
            Assert.IsTrue(codes.Contains("100.2")); // contains v3000
            Assert.IsTrue(codes.Contains("100.14")); // stereo bond between chiral centres
            Assert.IsTrue(codes.Contains("100.33")); // stereo with no chiral flag
        }

        /// <summary>
        /// 100.3 = v3000 not supported
        /// This catches files which are necessarily V3000 because V2000 cannot handle their features.
        /// </summary>
        [TestMethod]
        public void Validation_V3000NotSupported()
        {
            string molfile = i.loadMolecule(String.Concat(Enumerable.Range(1, 1000).Select(c => "CO"))).molfile();
            var codes = Validation.Validate(molfile).Issues.Select(c => c.Code);
            Assert.IsTrue(codes.Contains("100.3"));
        }

        /// <summary>
        /// 100.4 = non-InChIfiable atoms
        /// </summary>
        [TestMethod]
        public void Validation_NonInChIfiableAtoms()
        {
            // inner test
            TestGenericMolecule(Resource1.R1group, Resource1.benzeneAsMol, "100.4");
            // outer test
            var positive = Validation.Validate(Resource1.R1group).Issues.Select(i => i.Code);
            Assert.IsTrue(positive.Contains("100.4"));
            var negative = Validation.Validate(Resource1.benzeneAsMol).Issues.Select(i => i.Code);
            Assert.IsFalse(negative.Contains("100.4"));
        }

        /// <summary>
        /// 100.5 = non-aromatic query bonds
        /// </summary>
        [TestMethod]
        public void Validation_NonAromaticQueryBonds()
        {
            // inner test
            TestGenericMolecule(Resource1.querybonds, Resource1.benzeneAsMol, "100.5");
            // outer test
            var positive = Validation.Validate(Resource1.querybonds).Issues.Select(i => i.Code);
            var negative = Validation.Validate(Resource1.benzeneAsMol).Issues.Select(i => i.Code);
            Assert.IsTrue(positive.Contains("100.5"));
            Assert.IsFalse(negative.Contains("100.5"));
        }

        /// <summary>
        /// 100.6 = validationHasArgonAtom
        /// </summary>
        [TestMethod]
        public void Validation_ArgonChemistry()
        {
            List<Issue> issues = new List<Issue>();
            /// inner test
            string good = i.loadMolecule("[Ar]").molfile();
            string bad = i.loadMolecule("[Ar]c1ccccc1").molfile();
            ValidationGenericMolecule goodAr = new ValidationGenericMolecule(good, issues);
            ValidationGenericMolecule badAr = new ValidationGenericMolecule(bad, issues);
            var goodcodes = goodAr.runTests().Select(c => c.Code);
            var badcodes = badAr.runTests().Select(c => c.Code);
            Assert.IsFalse(goodcodes.Contains("100.6"), "validation (inner) should not return code 100.6");
            Assert.IsTrue(badcodes.Contains("100.6"), "validation (inner) should return code 100.6: "
                + String.Join("; ", badcodes));
            // outer test
            TestValidation(bad, good, "100.6", "argon");
        }

        /// <summary>
        /// 100.7 = fixed valence
        /// </summary>
        [TestMethod]
        public void Validation_FixedValence()
        {
            // inner test
            TestGenericMolecule(Resource1.fixedvalence, Resource1.benzeneAsMol, "100.7");
            Molecule fixedvalence = MoleculeFactory.Molecule(Resource1.fixedValenceMol);
            Console.WriteLine(fixedvalence.HasFixedValence());
            Molecule benzene = MoleculeFactory.Molecule(Resource1.benzeneAsMol);
            Molecule aluminium = MoleculeFactory.Molecule(Resource1.aluminiumForInChI);
            Assert.IsTrue(fixedvalence.HasFixedValence(), "fixed valence in original molfile not detected");
            Assert.IsFalse(benzene.HasFixedValence(), "spurious fixed valence in benzene identified");
            Assert.IsFalse(aluminium.HasFixedValence(), "spurious fixed valence in InChI-friendly aluminium identified");
            TestValidation(Resource1.fixedValenceMol, Resource1.aluminiumForInChI, "100.7", "fixed valence");
            var codes = Validation.Validate(Resource1.fixedvalence).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("100.7"));
        }

        /// <summary>
        /// 100.8 = partial ionization test
        /// Tests that the molecule has been ionized in the right order.
        /// </summary>
        [TestMethod]
        public void Validation_PartiallyIonizedMolecule()
        {
            var codes = Validation.Validate(Resource1.partiallyionized).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("100.8"));
        }

        /// <summary>
        /// 100.9 = all neutral molecules
        /// </summary>
        [TestMethod]
        public void Validation_AllNeutralMolecules()
        {
            // inner test
            ValidationMolecule vm = new ValidationMolecule(MoleculeFactory.FromMolV2000(Resource1.partiallyionized));
            var codes2 = vm.runTests().Select(i => i.Code);
            Assert.IsTrue(codes2.Contains("100.9"));

            // outer test
            var codes = Validation.Validate(Resource1.partiallyionized).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("100.9"));

            TestValidation(Resource1._7_goodNaOAc, Resource1.divinyl_benzene, "100.9", "all neutral molecules");
        }

        /// <summary>
        /// 100.10 = contains chair
        /// </summary>
        [TestMethod]
        public void Validation_Chair()
        {
            ValidationMolecule vm = new ValidationMolecule(MoleculeFactory.FromMolV2000(Resource1.chairglucose1));
            var codes3 = vm.runTests().Select(i => i.Code);
            Assert.IsTrue(codes3.Contains("100.10"));

            var codes = Validation.Validate(Resource1.chairglucose1).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("100.10"));
            var codes2 = Validation.Validate(Resource1.benzeneAsMol).Issues.Select(i => i.Code);
            Assert.IsFalse(codes2.Contains("100.10"));
        }

        /// <summary>
        /// 100.11 = contains perspective boat
        /// </summary>
        [TestMethod]
        public void Validation_Boat()
        {
            var codes = Validation.Validate(new Cdx(Resource1.boatsugar).ToMolFile()).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("100.11"));
        }

        /// <summary>
        /// 100.12 = contains Haworth hexagon
        /// </summary>
        [TestMethod]
        public void Validation_Haworth()
        {
            var codes = Validation.Validate(Resource1.horizontalhowarth).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("100.12"));
        }

        /// <summary>
        /// 100.13 = contains ring sp3 stereobond
        /// </summary>
        [TestMethod]
        public void Validation_RingStereoBond()
        {
            // inner first
            List<Issue> issues = new List<Issue>();
            Assert.IsTrue(ValidationStereoModule.ContainsRingSP3StereoBond(Resource1.ringstereo_present, issues));
            List<Issue> i2 = new List<Issue>();
            Assert.IsFalse(ValidationStereoModule.ContainsRingSP3StereoBond(Resource1.ringstereo_absent, i2));
            // then outer
            var codes = Validation.Validate(Resource1.ringstereo_present).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("100.13"));
            var codes2 = Validation.Validate(Resource1.ringstereo_absent).Issues.Select(i => i.Code);
            Assert.IsFalse(codes2.Contains("100.13"));
        }

        /// <summary>
        /// 100.14 = stereobonds between stereocentres
        /// </summary>
        [TestMethod]
        public void Validation_StereoBondBetweenStereoCentres()
        {
            // inner test
            List<Issue> issues = new List<Issue>();
            Assert.IsFalse(ValidationStereoModule.ContainsStereoBondBetweenStereoCenters(Resource1.stereobondbetweenstereocentres_false, issues));
            List<Issue> i2 = new List<Issue>();
            Assert.IsTrue(ValidationStereoModule.ContainsStereoBondBetweenStereoCenters(Resource1.stereobondbetweenstereocentres_true, issues));
            // outer tests
            string molfile_false = Resource1.stereobondbetweenstereocentres_false;
            var valid_dt = Validation.Validate(molfile_false);
            Assert.IsFalse((from i in valid_dt.Issues where i.Code == "100.14" select i).Any());

            string molfile_true = Resource1.stereobondbetweenstereocentres_true;
            var valid_dt2 = Validation.Validate(molfile_true);
            Assert.IsTrue((from i in valid_dt2.Issues where i.Code == "100.14" select i).Count() == 1);

            string molfile = Resource1.twostereobondsoneinwardoneoutward;
            var valid_dt3 = Validation.Validate(molfile).Issues.Select(i => i.Code);
            Assert.IsTrue(valid_dt3.Contains("100.14"));
            Assert.IsFalse(valid_dt3.Contains("100.15"));
        }

        /// <summary>
        /// 100.15 = stereocentres with more than 2 stereobonds
        /// </summary>
        [TestMethod]
        public void Validation_StereoCentersWithMoreThan2StereoBonds()
        {
            // inner test
            List<Issue> issues = new List<Issue>();
            Assert.IsTrue(ValidationStereoModule.ContainsStereoCentersWithMoreThan2StereoBonds(Resource1.fourstereobondsatcentre, issues));
            List<Issue> i2 = new List<Issue>();
            Assert.IsFalse(ValidationStereoModule.ContainsStereoCentersWithMoreThan2StereoBonds(Resource1.fourstereobondsatcentre_counterexample, i2));
            // outer test
            var valid_dt = Validation.Validate(Resource1.fourstereobondsatcentre).Issues;
            Assert.IsTrue((from i in valid_dt where i.Code == "100.15" select i).Count() == 1);
            valid_dt = Validation.Validate(Resource1.fourstereobondsatcentre_counterexample).Issues;
            Assert.IsFalse((from i in valid_dt where i.Code == "100.15" select i).Any());
        }

        /// <summary>
        /// 100.16 = stereocentre with 3 T-shaped bonds
        /// </summary>
        [TestMethod]
        public void Validation_StereoCenterWith1StereoAnd2PlainBonds_TShaped()
        {
            // inner test
            List<Issue> issues = new List<Issue>();
            Assert.IsTrue(ValidationStereoModule.ContainsStereoCenterWith3Bonds_TShaped(Resource1.tshapedmolecule, issues));
            List<Issue> i2 = new List<Issue>();
            Assert.IsTrue(ValidationStereoModule.ContainsStereoCenterWith3Bonds_TShaped(Resource1.tshapedmolecule_2, i2));
            // outer test
            var codes = Validation.Validate(Resource1.tshapedmolecule).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("100.16"));
            var codes2 = Validation.Validate(Resource1.tshapedmolecule_2).Issues.Select(i => i.Code);
            Assert.IsTrue(codes2.Contains("100.16"));
        }

        /// <summary>
        /// 100.17 = stereobond at double bond
        /// 100.18 = bad allene stereo
        /// 100.33 = stereo with no chiral flag
        /// 100.62  partial undefined stereo/mixtures
        /// </summary>
        [TestMethod]
        public void Validation_StereoBondAtDoubleBond()
        {
            string molfile = Resource1.stereobondatdoublebond;
            var codes = Validation.Validate(molfile).Issues.Select(i => i.Code);
            Assert.AreEqual(1, codes.Where(c => c == "100.17").Count());
            Assert.AreEqual(1, codes.Where(c => c == "100.33").Count());
            Assert.AreEqual(1, codes.Where(c => c == "100.62").Count());

            molfile = Resource1.stereobondatdoublebond_phosphate;
            var codes2 = Validation.Validate(molfile).Issues.Select(i => i.Code);
            Assert.IsFalse(codes2.Where(c => c == "100.17").Any());

            molfile = Resource1.stereobondatdoublebond_false;
            var codes3 = Validation.Validate(molfile).Issues.Select(i => i.Code);
            Assert.IsFalse(codes3.Where(c => c == "100.17").Any());

            molfile = Resource1.stereobondatdoublebond_allene;
            var codes4 = Validation.Validate(molfile).Issues.Select(i => i.Code);
            Assert.IsFalse(codes4.Where(c => c == "100.17").Any());
            Assert.IsFalse(codes4.Where(c => c == "100.18").Any());
        }

        /// <summary>
        /// 100.18 = bad allene stereo
        /// </summary>
        [TestMethod]
        public void Validation_BadAllene()
        {
            TestValidation(Resource1.badallene, Resource1.goodallene, "100.18", "should detect bad allene stereo for 'badallene' and not for 'goodallene'");
            TestValidation(Resource1.badallene2, Resource1.goodallene, "100.18", "should detect bad allene stereo for 'badallene2' and not for 'goodallene'");
            string molfile = Resource1.goodallene;
            var codes = Validation.Validate(molfile).Issues.Select(i => i.Code).Where(c => c != "100.64").Where(c => c != "100.50");
            Assert.AreEqual(2, codes.Count(), "wrong number of issues: " + String.Join("; ", codes));
            Assert.IsFalse(codes.Contains("100.18"), "bad allene stereo should not be detected for 'good allene'");
        }

        /// <summary>
        /// 100.19 = contains L-pyranose
        /// </summary>
        [TestMethod]
        public void Validation_LPyranose()
        {
            var codes = Validation.Validate(Resource1.LglucoseRegular).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("100.19"));
        }

        /// <summary>
        /// 100.20 = validationNonLinearTripleTest_C
        /// 100.21 = validationNonLinearTripleTest_N
        /// </summary>
        [TestMethod]
        public void Validation_NonlinearTriplyBondedCentreTests()
        {
            var codes = Validation.Validate(Resource1.bentacetonitrile).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("100.20"), String.Join("; ", codes));
            var codes2 = Validation.Validate(Resource1._5f_badtbutylisocyanide).Issues.Select(i => i.Code);
            Assert.IsTrue(codes2.Contains("100.21"), String.Join("; ", codes2));
            var codes3 = Validation.Validate(Resource1._5f_goodtbutylisocyanide).Issues.Select(i => i.Code);
            Assert.IsFalse(codes3.Contains("100.21"), String.Join("; ", codes3));
            Assert.IsFalse(codes3.Contains("100.20"), String.Join("; ", codes3));

            var codes4 = Validation.Validate(Resource1._5f_badbenzoylcyanide).Issues.Select(i => i.Code);
            Assert.IsTrue(codes4.Contains("100.20"), String.Join("; ", codes4));
            var codes5 = Validation.Validate(Resource1._5f_goodbenzoylcyanide).Issues.Select(i => i.Code);
            Assert.IsFalse(codes5.Contains("100.21"), String.Join("; ", codes5));
            Assert.IsFalse(codes5.Contains("100.20"), String.Join("; ", codes5));
        }

        /// <summary>
        /// 100.22 = is 0d
        /// </summary>
        [TestMethod]
        public void Validation_Is0D()
        {
            Molecule benzene = MoleculeFactory.Molecule(Resource1.benzene3D);
            Assert.IsFalse(benzene.Is0D(), benzene.ct());

            Indigo i = new Indigo();
            Molecule bonafide = MoleculeFactory.Molecule(i.loadMolecule("[Ar]").molfile());
            Assert.IsTrue(bonafide.Is0D(), "this is a bona fide 0d molecule");

            // 1. test method in GenericMolecules()
            Sdf sdf = new Sdf(Resource1.ZeroD_structures);
            foreach (var g in sdf.molecules)
            {
                Assert.IsTrue(g.Is0D(), g.ct());
            }
            // 2. now test validation does the right thing

            foreach (var c in sdf.molecules.Select(m => MoleculeFactory.Molecule(m.ct())))
            {
                var analysis = Validation.Validate(c.ct()).Issues.Select(a => a.Code);
                Assert.IsTrue(analysis.Contains("100.22"), "0d structure identified as not 0d");
            }

            string ar = i.loadMolecule("[Ar]").molfile();
            var codes = Validation.Validate(ar).Issues.Select(c => c.Code);
            Assert.IsFalse(codes.Contains("100.22"), "single-atom species don't count as 0d");
        }

        /// <summary>
        /// 100.23 = is 3d.
        /// </summary>
        [TestMethod]
        public void Validation_ThreeD()
        {
            // inner test of GenericMolecule object
            TestGenericMolecule(Resource1.benzene3D, Resource1.benzeneAsMol, "100.23");
            TestValidation(Resource1.benzene3D, Resource1.benzeneAsMol, "100.23", "3d vs. 2d benzene test");
            // outer test
            var positive = Validation.Validate(Resource1.benzene3D).Issues.Select(i => i.Code);
            var negative = Validation.Validate(Resource1.benzeneAsMol).Issues.Select(i => i.Code);
            Assert.IsTrue(positive.Contains("100.23"));
            Assert.IsFalse(negative.Contains("100.23"));
        }

        /// <summary>
        /// 100.24 = is congested
        /// </summary>
        [TestMethod]
        public void Validation_Congestion()
        {
            // inner test
            Molecule congested = MoleculeFactory.Molecule(Resource1.congestedsdf);
            Molecule crossedbonds = MoleculeFactory.Molecule(Resource1.crossedbondsdf);
            Molecule benzene = MoleculeFactory.Molecule(Resource1.benzeneAsMol);
            Assert.IsTrue(congested.IsCongested());
            Assert.IsTrue(crossedbonds.IsCongested());
            Assert.IsFalse(benzene.IsCongested());
            TestValidation(Resource1.congestedsdf, Resource1.benzeneAsMol, "100.24", "congested molecule");
        }

        /// <summary>
        /// 100.25 = uneven length bonds
        /// </summary>
        [TestMethod]
        public void Validation_UnevenLengthBonds()
        {
            GenericMolecule gm = MoleculeFactory.FromMolV2000(Resource1.congestedsdf);
            Assert.IsTrue(gm.HasUnevenLengthBonds());
            GenericMolecule notuneven = MoleculeFactory.FromMolV2000(Resource1.benzeneAsMol);
            Assert.IsFalse(notuneven.HasUnevenLengthBonds());
            TestValidation(Resource1.congestedsdf, Resource1.benzeneAsMol, "100.25", "uneven length bonds");
        }

        /// <summary>
        /// 100.26 = SMILES can't be generated
        /// </summary>
        [TestMethod]
        public void Validation_SmilesGeneration()
        {
            List<Issue> issues = new List<Issue>();
            string SMILES;
            bool negative = ValidationModule.CanIndigoGenerateSmiles("sjdifdifjdij", out SMILES, issues);
            Assert.IsFalse(negative);
            var codes = issues.Select(c => c.Code);
            Assert.IsTrue(codes.Contains("100.26"), String.Join("; ", codes));
            List<Issue> newIssues = new List<Issue>();
            bool positive = ValidationModule.CanIndigoGenerateSmiles("c1ccccc1", out SMILES, newIssues);
            Assert.IsTrue(positive);
            Assert.IsFalse(newIssues.Select(c => c.Code).Contains("100.26"));
        }

        /// <summary>
        /// 100.27 = Canonical SMILES can't be generated
        /// </summary>
        [TestMethod]
        public void Validation_CanonicalSmilesGeneration()
        {
            List<Issue> issues = new List<Issue>();
            string SMILES;
            bool boolean = ValidationModule.CanIndigoGenerateCanonicalSmiles("sjdifdifjdij", out SMILES, issues);
            var codes = issues.Select(c => c.Code);
            Assert.IsTrue(codes.Contains("100.27"), String.Join("; ", codes)); List<Issue> newIssues = new List<Issue>();
            bool positive = ValidationModule.CanIndigoGenerateSmiles("c1ccccc1", out SMILES, newIssues);
            Assert.IsTrue(positive);
            Assert.IsFalse(newIssues.Select(c => c.Code).Contains("100.27"));
        }

        /// <summary>
        /// 100.28 = generic molecule generation
        /// </summary>
        [TestMethod]
        public void Validation_GenericMoleculeGeneration()
        {
            List<Issue> badIssues = new List<Issue>();
            ValidationGenericMolecule vgm = new ValidationGenericMolecule("sdfidfjidjf", badIssues);
            Assert.IsTrue(badIssues.Select(i => i.Code).Contains("100.28"));
            List<Issue> goodIssues = new List<Issue>();
            ValidationGenericMolecule vg2 = new ValidationGenericMolecule(Resource1.benzeneAsMol, goodIssues);
            Assert.IsFalse(goodIssues.Select(i => i.Code).Contains("100.28"));
        }

        /// <summary>
        /// 100.29 = InChI generation failed
        /// </summary>
        [TestMethod]
        public void Validation_InChIGeneration()
        {
            string molfile = Resource1.v2000Ablock;
            var codes = Validation.Validate(molfile).Issues.Select(c => c.Code);
            Assert.IsTrue(codes.Contains("100.29"));
        }

        /// <summary>
        /// 100.30 = radical count failed
        /// </summary>
        [TestMethod]
        public void Validation_RadicalCountFailed()
        {
            List<Issue> issues = new List<Issue>();
            int result = ValidationModule.CountRadicalCenters("sdjiofjdifjidjfidj", issues);
            Assert.AreEqual(0, result, "dodgy molfiles return 0 by construction (boo)");
            Assert.IsTrue(issues.Select(c => c.Code).Contains("100.30"));
        }

        /// <summary>
        /// 100.31 = no atoms
        /// </summary>
        [TestMethod]
        public void Validation_AtomCountZero()
        {
            TestValidation(Resource1.empty, Resource1.benzeneAsMol, "100.31", "empty vs. benzene");
        }

        /// <summary>
        /// 100.32 = not overall neutral
        /// </summary>
        [TestMethod]
        public void Validation_OverallNeutral()
        {
            TestValidation(i.loadMolecule("[K+]").molfile(), Resource1.benzeneAsMol, "100.32", "potassium ion vs. benzene");
        }

        /// <summary>
        /// 100.33 = contains relative stereo
        /// </summary>
        [TestMethod]
        public void Validation_RelativeStereo()
        {
            Assert.IsTrue(ValidationStereoModule.ContainsRelativeStereo(Resource1.relativestereo),
                "should detect relative stereo");

            string molfile = Resource1.stereobondatdoublebond;
            var codes = Validation.Validate(molfile).Issues.Where(i => i != null).Select(i => i.Code);
            Assert.AreEqual(1, codes.Where(c => c == "100.33").Count());
        }

        /// <summary>
        /// 100.34 = free elements in mixtures
        /// </summary>
        [TestMethod]
        public void ValidationModules_HasForbiddenFreeElementsInMixtures()
        {
            var issues = new List<Issue>();
            bool result = ValidationModule.ContainsForbiddenFreeNeutralElementInMixture(Resource1.benzyllithium_freeneutral, issues);
            Assert.IsTrue(result);
            Assert.IsTrue(issues.Select(i => i.Code).Contains("100.34"));

            var i2 = new List<Issue>();
            bool r2 = ValidationModule.ContainsForbiddenFreeNeutralElementInMixture(Resource1.benzyllithium_bonded, i2);
            Assert.IsFalse(r2);
            Assert.IsFalse(i2.Select(i => i.Code).Contains("100.34"));

            var i3 = new List<Issue>();
            bool r3 = ValidationModule.ContainsForbiddenFreeNeutralElementInMixture(Resource1.benzyllithium_free, i3);
            Assert.IsFalse(r3);
            Assert.IsFalse(i3.Select(i => i.Code).Contains("100.34"));
        }

        /// <summary>
        /// 100.35 = forbidden valence/charge combination
        /// </summary>
        [TestMethod]
        public void Validation_ForbiddenValenceChargeCombination()
        {
            string molfile = Resource1.fourvalentN;
            List<Issue> issues = new List<Issue>();
            ValidationModule.ValidateValenceChargeRadicalAtAtoms(molfile, issues);
            Assert.IsTrue(issues.Select(c => c.Code).Contains("100.35"));
            var i2 = new List<Issue>();
            ValidationModule.ValidateValenceChargeRadicalAtAtoms(Resource1.pentavalentmol, i2);
            var codes = i2.Select(c => c.Code);
            Assert.IsTrue(codes.Contains("100.35"));
            Cdx hypervalents = new Cdx(Resource1.hypervalentmolecules);
            foreach (string ct in hypervalents.ToMolFiles())
            {
                var analysis = Validation.Validate(ct).Issues.Select(a => a.Code);
                if (analysis.Contains("100.35")) Assert.Fail("validation failing valid hypervalent molecule: " + ct);
            }
            // indigo tests
            IndigoGoodValenceTest(Resource1.benzeneAsMol);
            IndigoGoodValenceTest(Resource1.PF6anion);
            IndigoBadValenceTest(Resource1.pentavalentmol, "element: bad valence on C having 5 drawn bonds, charge 0, and 0 radical electrons");
            // inner test
            IndigoObject obj = i.loadMolecule("F[Fe-](F)(F)F");
            obj.layout();
            List<Issue> i3 = new List<Issue>();
            ValidationModule.ValidateValenceChargeRadicalAtAtoms(obj.molfile(), i3);
            Assert.IsTrue(i3.Select(c => c.Code).Contains("100.35"), "should return code 100.35 (forbidden valence):"
                + String.Join("; ", i3.Select(c => c.Code)));
            // outer tests
            NegativeSMILESTest("[Te+](C([H])([H])[H])C([H])([H])[H]", "100.35");
            NegativeSMILESTest("[Li+].Cl[Au-](Cl)(Cl)Cl", "100.35");
            PositiveSMILESTest("F[Fe-](F)(F)F", "100.35");
            PositiveSMILESTest("[Li+].Cl[Au](Cl)(Cl)Cl", "100.35");
        }

        /// <summary>
        /// 100.36 = duplicate molecules
        /// </summary>
        [TestMethod]
        public void Validation_ContainsDuplicateMolecules()
        {
            Assert.IsFalse(ValidationModule.ContainsDuplicateMolecules(Resource1.duplicatemolecules_counterexample),
                "false positive duplicate molecules");
            Assert.IsTrue(ValidationModule.ContainsDuplicateMolecules(Resource1.duplicatemolecules_example),
                "duplicate molecules not identified");
            var codes = Validation.Validate(Resource1.multiplebenzene).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("100.36"));
            // inner
            Assert.IsTrue(ValidationModule.ContainsDuplicateMolecules(Resource1.multiplebenzene));
            Assert.IsFalse(ValidationModule.ContainsDuplicateMolecules(Resource1.benzeneAsMol));
            // outer
            TestValidation(Resource1.multiplebenzene, Resource1.benzeneAsMol, "100.36", "duplicate molecules");
        }

        /// <summary>
        /// 100.37 = contains only multiple instances of same molecule
        /// </summary>
        [TestMethod]
        public void Validation_ContainsOnlyDuplicateMolecules()
        {
            var codes = Validation.Validate(Resource1.multiplebenzene).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Any(), "validation should return codes");
            Assert.IsTrue(codes.Contains("100.37"), "issues should contain 100.37: " + String.Join("; ", codes));
            var issues = Validation.Validate(Resource1.twowaters).Issues.Select(i => i.Code);
            Assert.IsTrue(issues.Contains("100.37"));
        }

        /// <summary>
        /// 100.38 = crossed double bond
        /// </summary>
        [TestMethod]
        public void Validation_CrossedDoubleBond()
        {
            TestValidation(Resource1.bromoiodopropene_crossedonly, Resource1.bromoiodopropene_flat,
                "100.38", // crossed double bond
                "should detect crossed double bond");
        }

        /// <summary>
        /// 100.39 = crossed double bond with adjacent wavy bond
        /// </summary>
        [TestMethod]
        public void Validation_CrossedDoubleBondWithAdjacentWavyBond()
        {
            List<Issue> issues = new List<Issue>();
            Assert.IsTrue(ValidationStereoModule.ContainsDoubleBondWithAdjacentWavyBond(Resource1.bromoiodopropene_wavy, issues));
            List<Issue> i2 = new List<Issue>();
            Assert.IsFalse(ValidationStereoModule.ContainsDoubleBondWithAdjacentWavyBond(Resource1.bromoiodopropene_flat, i2));

            string molfile = Resource1.bromoiodopropene_crossedandwavy;
            var valid_dt = Validation.Validate(molfile);
            Assert.IsTrue((from i in valid_dt.Issues where i.Code == "100.39" select i).Count() == 1);
            Assert.IsTrue((from i in valid_dt.Issues where i.Code == "100.38" select i).Count() == 1);

            molfile = Resource1.bromoiodopropene_flat;
            valid_dt = Validation.Validate(molfile);
            Assert.IsFalse((from i in valid_dt.Issues where i.Code == "100.39" select i).Any());
            Assert.IsFalse((from i in valid_dt.Issues where i.Code == "100.38" select i).Any());
        }

        /// <summary>
        /// 100.40 = double bond with adjacent wavy bond
        /// </summary>
        [TestMethod]
        public void Validation_ContainsDoubleBondWithAdjacentWavyBond()
        {
            string molfile = Resource1.doublebondwithattachedwavybond;
            var issues = Validation.Validate(molfile).Issues;
            Assert.IsNotNull(issues);
            var codes = issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("100.40"));
            Assert.IsFalse(codes.Contains("100.38"));
        }

        /// 100.41 = forbidden valence
        /// 2015-09-21: THIS IS NOT ACTUALLY USED ANYWHERE IN THE CODE
        ///
        /// <summary>
        /// 100.42 = too many atoms for validation
        /// </summary>
        [TestMethod]
        public void Validation_AtomCountExceeded()
        {
            // v2000
            // 2015-09-22
            // validation limit has been set to 1000, bafflingly, so we can't do this.
            string bigmol = i.loadMolecule(String.Concat(Enumerable.Range(1, 999).Select(c => "C"))).molfile();
            var codes1 = Validation.Validate(bigmol).Issues.Select(c => c.Code);
            //Assert.IsTrue(codes1.Contains("100.42"), "should be too many atoms (V2000): " + String.Join("; ", codes1));
            // v3000
            string hugemol = i.loadMolecule(String.Concat(Enumerable.Range(1, 1001).Select(c => "C"))).molfile();
            var codes = Validation.Validate(hugemol).Issues.Select(c => c.Code);
            foreach (var c in codes) Console.WriteLine(c);
            Assert.IsTrue(codes.Contains("100.42"), "should be too many atoms (V3000)");
            Assert.IsTrue(codes.Contains("100.2"), "should be V3000");
        }

        /// 100.43 = synonyms do not match the structure
        /// 2015-09-16: no longer!
        /// 
        /// 100.44 = synonyms could not be validated
        /// 2015-09-16: no longer!
        /// 
        /// 100.45 = synonyms *do* match the structure
        /// 2015-09-16: no longer!
        /// 
        /// 100.46 = InChI does not match the structure
        /// 2015-09-16: no longer!
        /// 
        /// 100.47 = InChI does match the structure
        /// 2015-09-16: no longer!
        /// 
        /// 100.48 = StdInChI could not be validated
        /// 2015-09-16: no longer!
        /// 
        /// 100.49 = validation non-std InChI could not be validated.
        /// 2015-09-16: no longer!
        ///
        /// 100.50 = No depositor-specified REGID
        /// 2015-09-16: no longer!
        ///
        /// 100.51 = Multiple REGIDs
        /// 2015-09-16: no longer!
        /// 
        /// 100.52 = Duplicate REGIDs
        /// 2015-09-16: no longer!

        /// <summary>
        /// 100.53 = free carbon monoxide in mixture
        /// </summary>
        [TestMethod]
        public void ValidationCarbonMonoxide_FreeCarbonMonoxide()
        {
            string notfree_co = @"
  Ketcher 04041417012D 1   1.00000     0.00000     0

  6  5  0     0  0            999 V2000
   -1.3500    0.9750    0.0000 Fe  0  0  0  0  0  0  0  0  0  0  0  0
   -2.3160    0.7162    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -2.5748   -0.2497    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -1.6088    1.9409    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -0.9017    2.6481    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
   -3.0231    1.4233    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
  1  2  1  0     0  0
  2  3  1  0     0  0
  1  4  1  0     0  0
  4  5  1  0     0  0
  2  6  2  0     0  0
M  END";
            string free_co = @"
  Ketcher 04041417052D 1   1.00000     0.00000     0

  8  6  0     0  0            999 V2000
   -1.3500    0.9750    0.0000 Fe  0  0  0  0  0  0  0  0  0  0  0  0
   -2.3160    0.7162    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -2.5748   -0.2497    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -1.6088    1.9409    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -0.9017    2.6481    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
   -3.0231    1.4233    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    1.8500    0.3000    0.0000 C   0  0  0  0  0  2  0  0  0  0  0  0
    2.7160   -0.2000    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
  1  2  1  0     0  0
  2  3  1  0     0  0
  1  4  1  0     0  0
  4  5  1  0     0  0
  2  6  2  0     0  0
  7  8  2  0     0  0
M  END";

            string co_fixedVal = @"
  Ketcher 04041417172D 1   1.00000     0.00000     0

  2  1  0     0  0            999 V2000
    1.8500    0.3000    0.0000 C   0  0  0  0  0  2  0  0  0  0  0  0
    2.7160   -0.2000    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
  1  2  2  0     0  0
M  END
";
            string co = @"
  Ketcher 04041417172D 1   1.00000     0.00000     0

  2  1  0     0  0            999 V2000
    1.8500    0.3000    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    2.7160   -0.2000    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
  1  2  2  0     0  0
M  END
";
            var validation = Validation.Validate(notfree_co);
            Assert.IsTrue((from p in validation.Issues where p.Code == "100.53" select p).Count() == 0);

            validation = Validation.Validate(free_co);
            Assert.IsTrue((from p in validation.Issues where p.Code == "100.53" select p).Count() == 1);

            validation = Validation.Validate(co_fixedVal);
            Assert.IsTrue((from p in validation.Issues where p.Code == "100.53" select p).Count() == 0);

            validation = Validation.Validate(co);
            Assert.IsTrue((from p in validation.Issues where p.Code == "100.53" select p).Count() == 0);
        }

        /// <summary>
        /// 100.54 = validationRadicalCenter_1
        /// 100.55 = validationRadicalCenters_2
        /// 100.56 = validationRadicalCenters_many
        /// </summary>
        [TestMethod]
        public void Validation_RadicalTests()
        {
            var r1 = Validation.Validate(Resource1.radical_one).Issues.Select(i => i.Code);
            Assert.IsTrue(r1.Contains("100.54"));
            var r2 = Validation.Validate(Resource1.radical_two).Issues.Select(i => i.Code);
            Assert.IsTrue(r2.Contains("100.55"));
            var r3 = Validation.Validate(Resource1.radical_three).Issues.Select(i => i.Code);
            Assert.IsTrue(r3.Contains("100.56"));
        }

        /// <summary>
        /// 100.57 = unknown stereocentres
        /// </summary>
        [TestMethod]
        public void Validation_UnknownStereo()
        {
            var codes = Validation.Validate(Resource1.unknownstereo2).Issues.Select(i => i.Code);
            Assert.IsTrue(codes.Contains("100.57"));
        }

        /// <summary>
        /// 100.58 = is enantiomer
        /// </summary>
        [TestMethod]
        public void ValidationAnalyzeStereo_isEnantiomer()
        {
            List<Issue> issues = new List<Issue>();
            ValidationModule.AnalyzeStereo(Resource1.G2_4_enantiomers, issues);
            Assert.IsTrue(issues.Select(i => i.Code).Contains("100.58"));
        }

        /// <summary>
        /// 100.59 = completely undefined mixture
        /// </summary>
        [TestMethod]
        public void ValidationAnalyzeStereo_isCompletelyUndefinedMixture()
        {
            List<Issue> issues = new List<Issue>();
            ValidationModule.AnalyzeStereo(Resource1.G2_42_completely_mixture, issues);
            Assert.IsTrue(issues.Select(i => i.Code).Contains("100.59"));
        }

        /// <summary>
        /// 100.60 = validation contains aromatic bonds after kekulization
        /// </summary>
        [TestMethod]
        public void Validation_NonUniqueDearomatization()
        {
            var codes = Validation.Validate(Resource1.borazine_nonunique).Issues.Select(i => i.Code);
            Assert.IsFalse(codes.Contains("100.69"), "validation should succeed for borazine: " + String.Join("; ", codes));
            Assert.IsTrue(codes.Contains("100.60"), String.Join("; ", codes));
        }

        /// <summary>
        /// 100.61 = epimer
        /// </summary>
        [TestMethod]
        public void ValidationAnalyzeStereo_isEpimer()
        {
            List<Issue> issues = new List<Issue>();
            ValidationModule.AnalyzeStereo(Resource1.G2_44_epimer, issues);
            Assert.IsTrue(issues.Select(i => i.Code).Contains("100.61"));
        }

        /// <summary>
        /// 100.62 = partially undefined mixture
        /// </summary>
        [TestMethod]
        public void ValidationAnalyzeStereo_isPartiallyUndefinedMixture()
        {
            List<Issue> i2 = new List<Issue>();
            ValidationModule.AnalyzeStereo(Resource1.G2_46_partially_mixture, i2);
            Assert.IsTrue(i2.Select(i => i.Code).Contains("100.62"));
        }

        /// 100.63 = non-matching SMILES
        /// 2015-09-16: no longer!
        /// 
        /// 100.64 = matching SMILES
        /// 2015-09-16: no longer!
        /// 
        /// 100.65 = SMILES could not be validated
        /// 2015-09-16: no longer!
        /// 
        /// 100.66 = InChItoCSID failed
        /// 2015-09-16: no longer!
        /// 
        /// 100.67 = InChI not found in CS
        /// 2015-09-16! no longer!

        /// <summary>
        /// 100.68 = Invalid CDX
        /// TODO 2015-09-25: to be written.
        /// </summary>
        //[TestMethod]
        public void Validation_InvalidCDX()
        {
            Assert.Inconclusive("not used in code: GCNCD-212");
        }

        /// <summary>
        /// 100.69 = General validation failure
        /// </summary>
        [TestMethod]
        public void Validation_GeneralValidationFailure()
        {
            var codes = Validation.Validate("sdofisiodfjsiodjfiosdj").Issues.Select(c => c.Code);
            Assert.IsTrue(codes.Contains("100.69"), "general validation failure not triggered. Codes returned: "
                + String.Join("; ", codes));
        }

        /// <summary>
        /// 100.70 = Custom validation information
        /// </summary>
        [TestMethod]
        public void Validation_CustomValidationInformation()
        {
            TestValidation(Resource1._5ai_pentavalentnitro, Resource1._5ai_tervalentnitro, "100.70", "pentavalent nitro");
            TestValidation(Resource1._5aii_nitrate, Resource1._5aii_tervalentnitrate, "100.70", "pentavalent nitrate");
            TestValidation(Resource1._5b_pyridineOxideBad, Resource1._5b_pyridineoxide, "100.70", "bad pyridine oxide");
            TestValidation(Resource1._5d_badazide, Resource1._5d_goodazide, "100.70", "bad azide");
            // 2015-09-25: there used to be a SMARTS rule covering hydrazones but this has gone
            // therefore I am disabling the test of Resource1._5e_badhydrazone for now.
            // TODO: revisit this?
        }        

        /// <summary>
        /// 100.71 = Custom validation warning
        /// </summary>
        [TestMethod]
        public void Validation_CustomValidationWarning()
        {
            TestValidation(i.loadMolecule("C1CCC1").molfile(), i.loadMolecule("CCCC").molfile(), "100.71", "cyclobutane");
            TestValidation(Resource1.adjacentcharges, Resource1.nonadjacentcharges, "100.71", "adjacent charges");
        }

        /// <summary>
        /// 100.72 = Custom validation error
        /// </summary>
        [TestMethod]
        public void Validation_CustomValidationError()
        {
            GenericMolecule benzene = MoleculeFactory.FromMolV2000(Resource1.benzeneAsMol);
            // genuinely ridiculous validation rule that tests for the presence of a carbon atom.
            ValidationRuleModule vrm = new RSC.CVSP.Compounds.ValidationRuleModule(Resource1.validationError);
            ValidationMolecule vm = new ValidationMolecule(benzene, vrm);
            var codes = vm.runTests().Select(c => c.Code);
            Assert.IsTrue(codes.Contains("100.72"));
        }
    }
}
