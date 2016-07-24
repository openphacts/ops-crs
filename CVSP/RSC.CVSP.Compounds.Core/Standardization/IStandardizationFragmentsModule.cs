
namespace RSC.CVSP.Compounds
{
    public interface IStandardizationFragmentsModule
    {
        string removeChargedAcidBaseResidues(string molfile);

        string removeWater(string molfile);

        string removeOrganicSolvents(string molfile);

        string removeGasMolecules(string molfile);

        string removeNeutralInorganicAcidBaseResidues(string molfile);

        /// <summary>
        /// returns Indigo canonical smiles
        /// </summary>
        /// <param name="smiles"></param>
        /// <returns></returns>
		string getLargestOrganicFragment(string mol, out string new_inchi, out string new_inchi_key);
    }
}
