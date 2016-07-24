using System;
using System.Collections.Generic;
using System.Diagnostics;
using com.ggasoftware.indigo;
using InChINet;

namespace RSC.CVSP.Compounds
{
    public class StandardizationFragmentsModule : IStandardizationFragmentsModule
    {
        private Indigo s_indigo = new Indigo();

		private readonly IStandardizationModule standardizationModule = null;

		public StandardizationFragmentsModule(IStandardizationModule standardizationModule)
        {
			if (standardizationModule == null)
				throw new ArgumentNullException("standardizationModule");

			this.standardizationModule = standardizationModule;

            s_indigo.setOption("ignore-stereochemistry-errors", "true");
            s_indigo.setOption("unique-dearomatization", "false");
            s_indigo.setOption("ignore-noncritical-query-features", "false");
            s_indigo.setOption("timeout", "600000");
        }

        public string removeChargedAcidBaseResidues(string molfile)
        {
            IndigoObject input_obj = s_indigo.loadMolecule(molfile);
            string input_non_std_inchi = InChIUtils.mol2InChI(molfile, InChIFlags.CRS);

            List<string> reactions = new List<string>();
            reactions.Add("[#6:1].[Cl-,F-,Br-,I-:2]>>[#6:1]");
            reactions.Add("[#6:1].[O-][N](=[O])=[O]>>[#6:1]");
            reactions.Add("[#6:1].[O-][S](=[O])(=[O])[OH]>>[#6:1]");
            reactions.Add("[#6:1].[O-][S](=[O])(=[O])[O-]>>[#6:1]");
            reactions.Add("[#6:1].[O-][P](=[O])([OH])[OH]>>[#6:1]");
            reactions.Add("[#6:1].[O-][P](=[O])([O-])[OH]>>[#6:1]");
            reactions.Add("[#6:1].[O-][P](=[O])([O-])[O-]>>[#6:1]");
            reactions.Add("[#6:1].[S-;v1][H]>>[#6:1]");
            reactions.Add("[#6:1].[S-2;v0]>>[#6:1]");
            reactions.Add("[#6:1].[O-][H]>>[#6:1]");//hydroxy
            foreach (string reaction in reactions)
            {
                IndigoObject reaction_obj = s_indigo.loadReactionSmarts(reaction);//neutralize alcohols
                s_indigo.transform(reaction_obj, input_obj);
            }

            string output_non_std_inchi = InChIUtils.mol2InChI(input_obj.molfile(), InChIFlags.CRS);

            if (!input_non_std_inchi.Equals(output_non_std_inchi))
                return input_obj.molfile();
            else return molfile;
        }

        public string removeWater(string molfile)
        {
            List<string> reactions = new List<string>();
            reactions.Add("[#6:1].[OH2]>>[#6:1]");//water
            return standardizationModule.ApplyReactions(molfile, reactions);
        }

        public string removeOrganicSolvents(string molfile)
        {
            List<string> reactions = new List<string>();
            reactions.Add("[#6:1].[CH3][OH]>>[#6:1]");//methanol
            reactions.Add("[#6:1].[CH3][CH2][OH]>>[#6:1]");//ethanol
            reactions.Add("[#6:1].[CH3][CH2][CH2][OH]>>[#6:1]");//propanol
            reactions.Add("[#6:1].[CH3][CH2](OH)[CH32]>>[#6:1]");//isopropanol
            reactions.Add("[#6:1].[CH3][CH2][CH2][CH2][OH]>>[#6:1]");//butanol
            reactions.Add("[#6:1].[CH3][CH](OH)[CH2][CH3]>>[#6:1]");//isobutanol
            reactions.Add("[#6:1].[CH3][C]([CH3])(OH)[CH3]>>[#6:1]");//tertbutanol
            reactions.Add("[#6:1].[CH2]1[CH2][CH2][CH2][CH2][CH2]1>>[#6:1]");//cyclohexanol
            reactions.Add("[#6:1].[CH3][CH2][CH2][CH2][CH2][CH3]>>[#6:1]");//hexanol
            reactions.Add("[#6:1].[CH2]1[CH2][O][CH2][CH2]1>>[#6:1]");//THF
            reactions.Add("[#6:1].[CH3][C](=[O])[CH3]>>[#6:1]");//acetone
            return standardizationModule.ApplyReactions(molfile, reactions);
        }

        public string removeGasMolecules(string molfile)
        {
            List<string> reactions = new List<string>();
            reactions.Add("[#6:1].[CH4]>>[#6:1]");
            reactions.Add("[#6:1].[CH3][CH3]>>[#6:1]");
            reactions.Add("[#6:1].[CH3][CH2][CH3]>>[#6:1]");
            reactions.Add("[#6:1].[CH3][CH2][CH2][CH3]>>[#6:1]");
            reactions.Add("[#6:1].[NH3]>>[#6:1]");

            return standardizationModule.ApplyReactions(molfile, reactions);
        }

        public string removeNeutralInorganicAcidBaseResidues(string molfile)
        {
            List<string> reactions = new List<string>();
            reactions.Add("[#6:1].[Cl,F,Br,I][H]>>[#6:1]");//hyfrochlorides
            reactions.Add("[#6:1].[H][O][N](=[O])=[O]>>[#6:1]");//nitrate
            reactions.Add("[#6:1].[H][O][S](=[O])(=[O])[OH]>>[#6:1]");//sulfates
            reactions.Add("[#6:1].[H][O][P](=[O])([OH])[OH]>>[#6:1]");//phosphates
            reactions.Add("[#6:1].[SH2]>>[#6:1]");//sulfide
            reactions.Add("[#6:1].[NH3]>>[#6:1]");//ammonia

            //reactions.Add("[#6:1].[Na+, K+, Rb+, Cs+]>>[#6:1]");//hydroxy

            //reactions.Add("[*:1].[CH4]>>[*:1]");//methane
            //reactions.Add("[*:1].[CH3][CH3]>>[*:1]");//methane

            return standardizationModule.ApplyReactions(molfile,reactions);
        }

        /// <summary>
        /// returns Indigo canonical smiles
        /// </summary>
        /// <param name="smiles"></param>
        /// <returns></returns>
        public string getLargestOrganicFragment(string mol,out string new_inchi, out string new_inchi_key)
        {
			new_inchi = String.Empty;
			new_inchi_key = String.Empty;
            try
            {
                IndigoObject obj = s_indigo.loadMolecule(mol);
                string[] input_charge_non_std_inchi = InChIUtils.mol2inchiinfo(mol, InChIFlags.CRS);
				new_inchi = input_charge_non_std_inchi[0];
				new_inchi_key = input_charge_non_std_inchi[1];

                IndigoObject big_fragment = null;

                int iter = 0;
                bool isOrganic = false;

                IndigoObject org_pattern = s_indigo.loadSmarts("[C,c]");//at least one carbion is present
                foreach (IndigoObject comp in obj.iterateComponents())
                {
                    IndigoObject compMolecule = comp.clone();

                    IndigoObject match = s_indigo.substructureMatcher(compMolecule).match(org_pattern);
                    iter++;
                    if (iter == 1)
                    {
                        big_fragment = compMolecule;
                        if (match != null) isOrganic = true;
                    }
                    else if (!isOrganic && match != null)
                    {
                        big_fragment = compMolecule;
                        isOrganic = true;
                    }
                    else if (compMolecule.molecularWeight() > big_fragment.molecularWeight() && match == null && !isOrganic)
                    {
                        big_fragment = compMolecule;
                        if (match != null) isOrganic = true;
                    }
                    else if (compMolecule.molecularWeight() > big_fragment.molecularWeight() && match != null && isOrganic)
                    {
                        big_fragment = compMolecule;

                    }
                }
                string[] output_charge_non_std_inchi = InChIUtils.mol2inchiinfo(big_fragment.molfile(), InChIFlags.CRS);

				if (!input_charge_non_std_inchi[0].Equals(output_charge_non_std_inchi[0]))
				{
					new_inchi = output_charge_non_std_inchi[0];
					new_inchi_key = output_charge_non_std_inchi[1];
					return big_fragment.molfile();
				}
				else return mol;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Fragment-unsensitive parent generation failed:" + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                return mol;
            }
        }
    }
}
