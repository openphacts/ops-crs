using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ChemSpider.Molecules;
using MoleculeObjects;

namespace RSC.CVSP.Compounds
{
	public static class GenericMoleculeExtensions
	{
		/// <summary>
		/// ST-4.4
		/// </summary>
		public static bool ContainsCrossedDoubleBond(this GenericMolecule gm)
		{
			foreach (KeyValuePair<int, MoleculeObjects.Bond> b in gm.IndexedBonds)
			{
				if (b.Value.order == BondOrder.Double && b.Value.bondStereo == BondStereo.CisTransEither)
					return true;
			}
			return false;
		}

		/// <summary>
		/// SP3 stereo bond attached to C=C double bond; omits allenes
		/// </summary>
		public static bool ContainsStereoBondAdjacentToDoubleBond(this GenericMolecule gm)
		{
			List<int> stereo_atom_indexes = new List<int>();

            //dictionary of double bond source and destination indexes
			List<int> DoubleBondAtoms = new List<int>();
			List<int> AlleneMiddleCarbonIndexes = new List<int>();
            foreach (MoleculeObjects.Bond b in gm.IndexedBonds.Values)
			{
                if (b.order == BondOrder.Double && b.bondStereo != BondStereo.CisTransEither)
				{
                    if (!b.Atom1.Element.Equals("C") || !b.Atom2.Element.Equals("C"))
						continue;

                    if (!DoubleBondAtoms.Contains(b.firstatomID))
                        DoubleBondAtoms.Add(b.firstatomID);
                    else AlleneMiddleCarbonIndexes.Add(b.firstatomID);

                    if (!DoubleBondAtoms.Contains(b.secondatomID))
                        DoubleBondAtoms.Add(b.secondatomID);
                    else AlleneMiddleCarbonIndexes.Add(b.secondatomID);
				}

                if (b.order == BondOrder.Single && (b.bondStereo == BondStereo.Up || b.bondStereo == BondStereo.Down))
                    stereo_atom_indexes.Add(b.firstatomID);
			}

            foreach (MoleculeObjects.Bond b in gm.IndexedBonds.Values.Where(b => b.order == BondOrder.Double && b.bondStereo != BondStereo.CisTransEither))
				{
                if ((!AlleneMiddleCarbonIndexes.Contains(b.firstatomID) && !AlleneMiddleCarbonIndexes.Contains(b.secondatomID)) &&
                    (stereo_atom_indexes.Contains(b.firstatomID) || stereo_atom_indexes.Contains(b.secondatomID)))
					{
                    if (!b.Atom1.Element.Equals("C") || !b.Atom2.Element.Equals("C"))
							continue;

						return true;
					}
				}
			return false;
		}

		/// <summary>
		/// Wavy bond attached to crossed C=C double bond;
		/// </summary>
		public static bool ContainsWavyBondAdjacentToCrossedDoubleBond(this GenericMolecule gm)
		{
			//collect atoms with wavy bond
			List<int> AtomsWithWavyBond = new List<int>();
			foreach (KeyValuePair<int, MoleculeObjects.Bond> b in gm.IndexedBonds)
			{
				if (b.Value.bondStereo == BondStereo.Either)
					AtomsWithWavyBond.Add(b.Value.firstatomID);
			}

			foreach (KeyValuePair<int, MoleculeObjects.Bond> b in gm.IndexedBonds)
			{
				if (b.Value.order == BondOrder.Double && b.Value.bondStereo == BondStereo.CisTransEither)
				{
					if (AtomsWithWavyBond.Contains(b.Value.firstatomID) || AtomsWithWavyBond.Contains(b.Value.secondatomID))
						return true;
				}
			}
			return false;
		}
	}
}
