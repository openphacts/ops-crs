using InChINet;
using RSC.Logging;
using System.Collections.Generic;

namespace RSC.CVSP.Compounds
{
	public interface IStandardizationModule
    {
        string ApplyReactions(string molfile, IList<string> reactions);

        string Aromatize(string molFile);

        /// <summary>
        ///returns molfile if vendor is ChemAxon and InChIs with flag InChIFlags.CRS_Tautomer;
        /// if vendor is Indigo - returns null (molfile) and InChIs with flag InChIFlags.CRS
        /// </summary>
        /// <param name="molFile"></param>
        /// <param name="isCRSDeposition"></param>
        /// <param name="issueCollection"></param>
        /// <param name="TransformationCollection"></param>
        /// <returns></returns>
        string CanonicalizeTautomer(string molFile, bool isCRSDeposition, Resources.Vendor vendor, out string newInchi, out string newInchiKey, InChIFlags flags, IList<Issue> issueCollection = null);

        string FoldAllHydrogens(string molfile);

        string FoldNonStereoHydrogens(string molFile);

        string Kekulize(string molFile);

        string Layout(string molFile, Resources.Vendor vendor, out bool hasStandardInchiChanged, IList<Issue> issueCollection = null);

        string RemoveAmbiguousSp3Stereo(string molFile);

        string RemoveStereo(string molFile, out string newInchi, out string newInchiKey, InChIFlags flags, IList<Issue> issueCollection = null);

        string ReplaceIsotopes(string molFile, out string newInchi, out string newInchiKey, InChIFlags flags);

        bool ShouldRunTautomerCanonicalizer(string molFile, int tautomerCanonicalizationAtomLimit);

        string StandardizeByInChIRules(string mol);

        string TreatAmmonia(string molFile);
	}
}
