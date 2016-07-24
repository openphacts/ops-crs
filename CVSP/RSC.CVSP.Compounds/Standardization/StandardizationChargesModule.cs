using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using com.ggasoftware.indigo;
using InChINet;

namespace RSC.CVSP.Compounds
{
	public class StandardizationChargesModule : IStandardizationChargesModule
	{
		Indigo s_indigo;
        IStandardizationModule standardizationModule;

		public StandardizationChargesModule(IStandardizationModule standardizationModule)
		{
            if (standardizationModule == null)
                throw new ArgumentNullException("standardizationModule");

            this.standardizationModule = standardizationModule;

			s_indigo = new Indigo();
			s_indigo.setOption("ignore-stereochemistry-errors", "true");
			s_indigo.setOption("ignore-noncritical-query-features", "true");
		}

		/// <summary>
		/// returns molfile and reference to new inchi and inchi key
        /// 2015-09-18: Should probably return a StandardizationResult with issues 
        /// 2015-09-21: returns null if charges not neutralized.
		/// </summary>
		public string NeutralizeCharges(string mol, out string new_inchi, out string new_inchi_key, InChIFlags flags)
		{
            var reactions = new List<string>()
            {
                "[*+:1][H]>>[*:1]", // remove extra proton (CN01 on Confluence)
                "[N+:1]~[CH3]>>[N:1]", // remove extra methyl (CN02)
                "[*+:1][CH2][CH3]>>[*:1]", // remove extra ethyl (CN03)
                "[#6:1][O-,S-,Se-,Te-,N-;X1:2]>>[#6:1][O,S,Se,Te,N:2]", // neutralize chalcogens (CN04)
                "[C,c:1][N-:2]>>[C,c:1][NH:2]", // neutralize amines (CN05)
                "[S+0,Se+0,Te+0,P+0,O:1][O-,S-,Se-,Te-;X1:2]>>[S,Se,Te,P,O:1][O,S,Se,Te:2]", // neutralize acids (CN06)
                "[C-,c-,N-,n-:1]>>[C,c,N,n:1]", // add proton to negative carbon or nitrogen (CN07)
                "[O-:1][N+0:2]>>[O:1][N:2]" // neutralize singly-bonded O- to neutral N (CN08)
            };
			new_inchi = String.Empty;
			new_inchi_key = String.Empty;
			try
			{
				lock (s_indigo)
				{
					IndigoObject molecule = s_indigo.loadMolecule(mol);
					//check that there are charges
					bool chargeFound = false;
					foreach (IndigoObject atom in molecule.iterateAtoms())
					{
						if (atom.charge() != 0)
						{
							chargeFound = true;
							break;
						}
					}
                    if (!chargeFound) return null;
                    
					string[] input_charge_non_std_inchi = InChIUtils.mol2inchiinfo(mol, flags);
					new_inchi = input_charge_non_std_inchi[0];
					new_inchi_key = input_charge_non_std_inchi[1];

					//if free atoms found neutralize charges
					foreach (IndigoObject atom in molecule.iterateAtoms())
					{
						bool isFreeAtom = true;
						foreach(IndigoObject n_atom in atom.iterateNeighbors())
						{
							isFreeAtom = false;
							break;
						}
						if (isFreeAtom)
							atom.resetCharge();
					}
                    string result = standardizationModule.ApplyReactions(molecule.molfile(), reactions);

                    string[] output_charge_non_std_inchi = InChIUtils.mol2inchiinfo(result, flags);
					new_inchi = output_charge_non_std_inchi[0];
					new_inchi_key = output_charge_non_std_inchi[1];
					if (!input_charge_non_std_inchi[1].Equals(output_charge_non_std_inchi[1]))
						return result;
					else return null;
				}
			}
            // 2015-09-21
            // actually if anything goes wrong we shouldn't generate a parent
			catch (Exception)
			{
				return null;
			}
		}
	}
}
