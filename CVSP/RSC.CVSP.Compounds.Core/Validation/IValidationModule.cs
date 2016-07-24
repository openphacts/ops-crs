using com.ggasoftware.indigo;
using MoleculeObjects;
using RSC.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
	public interface IValidationModule
	{
		IndigoObject TryLoadingSdfToIndigo(string molfile, ICollection<Issue> issues);

		/// <summary>
		/// Relies on InChI.
		/// </summary>
		bool ContainsDuplicateMolecules(string molfile, ICollection<Issue> issues = null);

		bool IsAtomCountZero(string molfile, ICollection<Issue> issues);

		bool DoesAtomCountExceedMaximum(string molfile, int maxAtomCountAllowed, ICollection<Issue> issues = null);

		bool ContainsRelativeStereoInV2000(string molfile, ICollection<Issue> issues);

		bool ContainsForbiddenFreeNeutralElementInMixture(string molfile, List<Issue> issues);

        bool CanIndigoGenerateSmiles(string molfile, out string Smiles, ICollection<Issue> issues = null);
        bool CanIndigoGenerateCanonicalSmiles(string molfile, out string canonicalSmiles, ICollection<Issue> issues = null);
        bool HasUniqueDearomatization(string molfile, ICollection<Issue> issues = null);

		bool IndigoIsAmbiguousHydrogenPresent(string molfile, ICollection<Issue> issues);

		bool ContainsRadicals(string molfile, ICollection<Issue> issues);

		bool ContainsEitherBond(string molfile);

		bool ContainsNotKekulizedAromaticRings(string mol);

		bool ContainsSmarts(string molfile, string smarts);

		int CountRadicalCenters(string molfile, ICollection<Issue> issues);

		/// <summary>
		/// Relies on InChI.
		/// </summary>
		bool ContainsOnlyMultipleInstancesOfSameMolecules(string molfile, ICollection<Issue> issues);

		bool IsV3000(string molfile);

		bool IsMolfileFormatValid(string sdf_or_molfile, out string fixedSdf, bool trytofix = false, ICollection<Issue> issues = null);

		AnalysedInChI AnalyzeStereo(string molfile, ICollection<Issue> issues = null);

        bool ContainsMixtureWithFreeCO(string molfile, ICollection<Issue> issues = null);
        bool ContainsUnevenLengthBonds(string molfile, ICollection<Issue> issues = null);

        string ConvertV3000ToV2000(string v3000, ICollection<Issue> issues = null);
        bool IsOverallSystemCharged(string molfile, ICollection<Issue> issues = null);
        bool TryGeneratingStdInChI(string molfile, out string std_inchi, ICollection<Issue> issues = null);

        bool ValidateSMILES(string molfile, string stdInChI, List<string> SMILESs, ICollection<Issue> issues);
        string ValidateSMILES(string SMILES, ICollection<Issue> issues, bool ignoreIndigoStereoException,
            bool ignoreIndigoUniqueDearomatizationException, bool deAromatizeReturnedSmiles);
        bool ValidateValenceChargeRadicalAtAtoms(string molfile, ICollection<Issue> issues = null);
	}
}
