using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Configuration;

using MoleculeObjects;
using com.ggasoftware.indigo;
using InChINet;
using RSC.Logging;

namespace RSC.CVSP.Compounds
{
    public class Validation
    {
        private readonly IAcidity acidity = null;
        private readonly IValidationModule validationModule = null;
        private readonly IValidationStereoModule validationStereoModule = null;
        private readonly IValidationRuleModule validationRuleModule = null;

        readonly static IList<Enums.ProcessingModule> s_validationModules = new List<Enums.ProcessingModule>() 
		{
			Enums.ProcessingModule.ContainsAmbiguousHydrogen,
			Enums.ProcessingModule.ContainsRelativeStereoInV2000,
			Enums.ProcessingModule.ContainsDoubleBondWithAdjacentWavyBond,
			Enums.ProcessingModule.CanGenerateSmiles,
			Enums.ProcessingModule.ContainsDuplicateMolecules,
			Enums.ProcessingModule.ContainsOnlyMultipleInstancesOfSameMolecules,
			Enums.ProcessingModule.ContainsRadicals,
			Enums.ProcessingModule.ContainsMixtureWithFreeCO,
			Enums.ProcessingModule.ValidateRegIds,
			Enums.ProcessingModule.AnalyzeStereoByInChI,
			Enums.ProcessingModule.ValidateOverallCharge,
			Enums.ProcessingModule.ValidateValenceCharge,
			Enums.ProcessingModule.ContainsRingSP3StereoBond,
			Enums.ProcessingModule.ContainsStereoBondBetweenStereoCenters,
			Enums.ProcessingModule.ContainsStereoCentersWithMoreThan2StereoBonds,
			Enums.ProcessingModule.ContainsStereoCenterWith3BondsTShaped,
			Enums.ProcessingModule.ContainsBadAlleneStereo,
            Enums.ProcessingModule.ContainsUnevenLengthBonds,
// 2015-09-16: and remove these from the enum when we're sure it won't break anything
//            Enums.ProcessingModule.ValidateInChI,
//            Enums.ProcessingModule.ValidateSmiles,
//            Enums.ProcessingModule.ValidateSynonyms
		};

        public Validation(IAcidity acidity, IValidationModule validationModule, IValidationStereoModule validationStereoModule,
            IValidationRuleModule validationRuleModule)
        {
            if (acidity == null)
                throw new ArgumentNullException("acidity");

            if (validationModule == null)
                throw new ArgumentNullException("validationModule");

            if (validationStereoModule == null)
                throw new ArgumentNullException("validationStereoModule");

            if (validationRuleModule == null)
                throw new ArgumentNullException("validationRuleModule");
            
            this.acidity = acidity;
            this.validationModule = validationModule;
            this.validationStereoModule = validationStereoModule;
            this.validationRuleModule = validationRuleModule;
        }

        /// <summary>
        /// </summary>
        /// <returns> returns validation issues</returns>
        public ValidationResult Validate(string molfile)
        {
            Trace.TraceInformation("Validation started: " + DateTime.Now);
            var issues = new List<Issue>();
            // TODO: (2015-09-25) revisit this!
            int maxAtomLimit = 300;
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["MaxAtomLimit_4_Validation2Run"]))
                maxAtomLimit = Convert.ToInt32(ConfigurationManager.AppSettings["MaxAtomLimit_4_Validation2Run"]);

            try
            {
                //if V3000  - convert to V2000
                bool isV3000 = validationModule.IsV3000(molfile);
                if (isV3000)
                {
                    issues.Add(new Issue() { Code = "100.2" });
                    // do this in two places because aargh
                    if (validationModule.DoesAtomCountExceedMaximum(molfile,maxAtomLimit, issues))
                    {
                        // 100.3 = V3000 feature we can't handle
                        issues.Add(new Issue() { Code = "100.3" });
                        // 100.42 = too many atoms
                        issues.Add(new Issue() { Code = "100.42" });
                        return new ValidationResult() { Issues = issues.Distinct() };
                    }
                    string sdf_rec_V2000 = validationModule.ConvertV3000ToV2000(molfile);
                    if (validationModule.IsV3000(sdf_rec_V2000))
                    {
                        // 100.3 = V3000 feature we can't handle
                        issues.Add(new Issue() { Code = "100.3" });
                        return new ValidationResult() { Issues = issues.Distinct() };
                    }
                    else
                        molfile = sdf_rec_V2000;
                }

                string fixedSdf;
                validationModule.IsMolfileFormatValid(molfile, out fixedSdf, true, issues);
                molfile = fixedSdf;

                //check if atom count is zero
                if (validationModule.IsAtomCountZero(molfile, issues))
                    return new ValidationResult() { Issues = issues.Distinct() };

                //check if atom count exceeds max limit

                if (maxAtomLimit != 0 && validationModule.DoesAtomCountExceedMaximum(molfile, maxAtomLimit, issues))
                {
                    Trace.TraceInformation("Atom count exceeded the allowed number of " + maxAtomLimit);
                    //Trace.TraceInformation(mol_input.molfile());
                    Trace.TraceInformation("Validation stopped: " + DateTime.Now);
                    issues.Add(new Issue() { Code = "100.42" });
                    return new ValidationResult() { Issues = issues.Distinct() };
                }

                // run GenericMolecule tests and acquire FACTS
                ValidationGenericMolecule v_gm = new ValidationGenericMolecule(molfile, issues);
                List<Issue> gm_issues = v_gm.runTests();
                issues.AddRange(gm_issues);
                if (v_gm.gm == null || (from issue in gm_issues join et in LogManager.Logger.EntryTypes on issue.Code equals et.Code where et.Severity == Severity.Error select issue).Any())
                    return new ValidationResult() { Issues = issues.Distinct() };

                // check for unique dearomatization
                if (!validationModule.HasUniqueDearomatization(molfile, issues))
                {
                    return new ValidationResult() { Issues = issues.Distinct() };
                }
                // try loading to Indigo with stricter stereo and dearomatization options
                IndigoObject mol_input = validationModule.TryLoadingSdfToIndigo(molfile, issues);
                if (mol_input == null)
                    return new ValidationResult() { Issues = issues.Distinct() };
                //need to dearomatize before running Molecule test
                mol_input.dearomatize();

                GenericMolecule gm_dearomatized = MoleculeFactory.FromMolV2000(mol_input.molfile());
                ValidationMolecule v_m = new ValidationMolecule(gm_dearomatized);
                foreach (Issue i in v_m.runTests())
                    if (!issues.Contains(i))
                        issues.Add(i);

                string StdInChI = String.Empty, Smiles = String.Empty, canonicalSmiles = String.Empty;

                //can InChI be generated?
                bool CanGenerateInChI = validationModule.TryGeneratingStdInChI(mol_input.molfile(), out StdInChI, issues);

                //Parallel.ForEach(s_validationModules, module =>
                foreach (Enums.ProcessingModule module in s_validationModules)
                {
                    if (module == Enums.ProcessingModule.ContainsAmbiguousHydrogen)
                        validationModule.IndigoIsAmbiguousHydrogenPresent(mol_input.molfile(), issues);
                    else if (module == Enums.ProcessingModule.ContainsDoubleBondWithAdjacentWavyBond)
                        validationStereoModule.ContainsDoubleBondWithAdjacentWavyBond(molfile, issues);
                    else if (module == Enums.ProcessingModule.ContainsRelativeStereoInV2000)
                        validationModule.ContainsRelativeStereoInV2000(molfile, issues);
                    else if (module == Enums.ProcessingModule.CanGenerateSmiles)
                    {
                        validationModule.CanIndigoGenerateSmiles(mol_input.molfile(), out Smiles, issues);
                        validationModule.CanIndigoGenerateCanonicalSmiles(mol_input.molfile(), out canonicalSmiles, issues);
                    }
                    else if (module == Enums.ProcessingModule.ContainsDuplicateMolecules)
                        validationModule.ContainsDuplicateMolecules(mol_input.molfile(), issues);
                    else if (module == Enums.ProcessingModule.ContainsOnlyMultipleInstancesOfSameMolecules)
                        validationModule.ContainsOnlyMultipleInstancesOfSameMolecules(mol_input.molfile(), issues);
                    else if (module == Enums.ProcessingModule.ContainsRadicals)
                        validationModule.ContainsRadicals(mol_input.molfile(), issues);
                    else if (module == Enums.ProcessingModule.ContainsMixtureWithFreeCO)
                        validationModule.ContainsMixtureWithFreeCO(mol_input.molfile(), issues);

                    else if (module == Enums.ProcessingModule.AnalyzeStereoByInChI && CanGenerateInChI)
                        validationModule.AnalyzeStereo(mol_input.molfile(), issues);
                    else if (module == Enums.ProcessingModule.ValidateOverallCharge)
                        validationModule.IsOverallSystemCharged(mol_input.molfile(), issues);
                    else if (module == Enums.ProcessingModule.ValidateValenceCharge)
                        validationModule.ValidateValenceChargeRadicalAtAtoms(mol_input.molfile(), issues);

                    else if (module == Enums.ProcessingModule.ContainsRingSP3StereoBond)
                        validationStereoModule.ContainsRingSP3StereoBond(molfile, issues);
                    else if (module == Enums.ProcessingModule.ContainsStereoBondBetweenStereoCenters)
                        validationStereoModule.ContainsStereoBondBetweenStereoCenters(molfile, issues);
                    else if (module == Enums.ProcessingModule.ContainsStereoCentersWithMoreThan2StereoBonds)
                        validationStereoModule.ContainsStereoCentersWithMoreThan2StereoBonds(molfile, issues);
                    else if (module == Enums.ProcessingModule.ContainsStereoCenterWith3BondsTShaped)
                        validationStereoModule.ContainsStereoCenterWith3Bonds_TShaped(molfile, issues);

                    else if (module == Enums.ProcessingModule.ContainsBadAlleneStereo)
                        validationStereoModule.ContainsBadAlleneStereo(molfile, issues);
                    else if (module == Enums.ProcessingModule.ContainsUnevenLengthBonds)
                        validationModule.ContainsUnevenLengthBonds(mol_input.molfile(), issues);
                }
            }
            catch (Exception ex)
            {
                issues.Add(ex.ParseIndigoException());
            }
            Trace.TraceInformation("Validation finished: " + DateTime.Now);
            return new ValidationResult() { Issues = issues.Distinct() };
        }
    }
}