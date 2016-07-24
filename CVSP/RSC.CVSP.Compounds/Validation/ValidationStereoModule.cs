using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoleculeObjects;

using com.ggasoftware.indigo;
using RSC.Logging;

namespace RSC.CVSP.Compounds
{
	public class ValidationStereoModule : IValidationStereoModule
	{
		static Indigo s_indigo;
		static ValidationStereoModule()
		{
			s_indigo = new Indigo();
			s_indigo.setOption("ignore-stereochemistry-errors", true);
			s_indigo.setOption("ignore-noncritical-query-features", true);
			s_indigo.setOption("unique-dearomatization", false);
			s_indigo.setOption("timeout", "600000");
		}

		public bool ContainsUpAndDownBondsWithNoChiralFlag(string molfile)
		{
			GenericMolecule gm = MoleculeFactory.FromMolV2000(molfile);
			string AtomCountLine = molfile.Split('\n')[3];
			int chiralFlag = Convert.ToInt32(AtomCountLine.Substring(14, 1));
			if (chiralFlag == 1)
				return false;
			foreach (KeyValuePair<int, Bond> bond in gm.IndexedBonds)
			{
				if (bond.Value.bondStereo == BondStereo.Down || bond.Value.bondStereo == BondStereo.Up)
					return true;
			}
			return false;
		}

        /// <summary>
        /// Mystifyingly the isChiral() method in Indigo doesn't do what is advertised.
        /// Here is a replacement.
        /// </summary>
        public bool IsChiralFlagSet(string molfile)
        {
            return molfile.SplitOnNewLines().Skip(3).First().Substring(14, 1) == "1";
        }

		/// <summary>
		/// there is a chiral flag but no up or down bonds
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>
		public bool ContainsNoUpOrDownBondWithChiralFlag(string molfile)
		{
			IndigoObject obj = s_indigo.loadMolecule(molfile);
			int k = 0;
            bool isChiral = IsChiralFlagSet(molfile);
			foreach (IndigoObject sp3atoms in obj.iterateStereocenters())
				k++;

			if (isChiral && k == 0)
				return true;
			return false;
		}

		public bool ContainsRelativeStereo(string mol)
		{
			IndigoObject mol_obj = s_indigo.loadMolecule(mol);
			if (mol_obj.isChiral())
				return false;
			foreach (IndigoObject bonds in mol_obj.iterateBonds())
			{
				if (bonds.bondOrder() == 1 && (bonds.bondStereo() == Indigo.DOWN || bonds.bondStereo() == Indigo.UP))
					return true;
			}
			return false;
		}

		/// <summary>
		/// ST-1.1.4
		/// Three plain bonds and one wedged bond, with one pair of plain bonds separated by 180° or more and
		/// the wedged bond positioned within that largest space between plain bonds.
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="issues"></param>
		/// <returns></returns>
		public bool Contains_3_PlainBondsAndStereoBond_ST_1_1_4(string mol, ICollection<Issue> issues = null)
		{
            if (issues == null) issues = new List<Issue>();
            GenericMolecule gm = MoleculeFactory.FromMolV2000(mol);
            List<Tuple<int, int>> bondsByAtom = new List<Tuple<int, int>>();
            foreach (var b in gm.IndexedBonds)
            {
                bondsByAtom.Add(Tuple.Create(b.Value.firstatomID, b.Key));
                bondsByAtom.Add(Tuple.Create(b.Value.secondatomID, b.Key));
            }
            Dictionary<int, List<int>> newBondsByAtom = bondsByAtom.GroupBy(b => b.Item1)
                .ToDictionary(b => b.Key, b => b.Select(c => c.Item2).ToList());
            var fourbonders = newBondsByAtom.Where(b => b.Value.Count == 4).ToDictionary(p => p.Key, p => p.Value);
            Console.WriteLine(fourbonders.Count);
            var candidates = fourbonders.Where(b =>
                b.Value.Where(c => gm.IndexedBonds[c].bondStereo == BondStereo.None).Count() == 3 &&
                b.Value.Where(c => gm.IndexedBonds[c].bondStereo == BondStereo.Down || gm.IndexedBonds[c].bondStereo == BondStereo.Up).Count() == 1)
                .ToDictionary(p => p.Key, p => p.Value);
            foreach (var candidate in candidates)
            {
                Console.WriteLine(candidate.Key + " " + String.Join(" ", candidate.Value));
                int wedgeBondId = candidate.Value.First(i => gm.IndexedBonds[i].bondStereo != BondStereo.None);
                // get angles and sort into the correct order
                var angles = candidate.Value.Except(new[] { wedgeBondId }).Select(b => Geometry.AngleBetweenThreePoints(gm.IndexedAtoms[candidate.Key].xyz,
                    gm.IndexedAtoms[gm.IndexedBonds[wedgeBondId].OtherAtomID(candidate.Key)].xyz,
                    gm.IndexedAtoms[gm.IndexedBonds[b].OtherAtomID(candidate.Key)].xyz)).OrderBy(a => a);
                if (angles.First() + angles.ElementAt(1) >= 180.0) return true;
            }
            // we have got this far and not found any
            return false;
		}

		public bool ContainsRingSP3StereoBond(string mol, ICollection<Issue> issues)
		{
			IndigoObject mol_obj = s_indigo.loadMolecule(mol);
			foreach (IndigoObject bond in mol_obj.iterateBonds())
				if (bond.topology() == Indigo.RING && (bond.bondStereo() == Indigo.DOWN || bond.bondStereo() == Indigo.UP))
				{
					issues.Add(new Issue() { Code = "100.13" });
					return true;
				}
			return false;
		}

		public bool ContainsStereoCentersWithMoreThan2StereoBonds(string mol, ICollection<Issue> issues)
		{
			IndigoObject mol_obj = s_indigo.loadMolecule(mol);

			foreach (IndigoObject stereo_center in mol_obj.iterateStereocenters())
			{
				int stereo_bond_count = 0;
				foreach (IndigoObject neighbor in stereo_center.iterateNeighbors())
				{
					if ((neighbor.bond().bondStereo() == Indigo.DOWN || neighbor.bond().bondStereo() == Indigo.UP) && neighbor.bond().source().index() == stereo_center.index())
						stereo_bond_count++;

					if (stereo_bond_count > 2)
					{
						issues.Add(new Issue() { Code = "100.15" });
						return true;
					}
				}
			}
			return false;
		}


		/// <summary>
		/// ST-2.3 T-shaped; should be no Z coordinate
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="issues"></param>
		/// <returns></returns>
		public bool ContainsStereoCenterWith3Bonds_TShaped(string mol, ICollection<Issue> issues)
		{
			IndigoObject mol_obj = s_indigo.loadMolecule(mol);

			foreach (IndigoObject stereo_center in mol_obj.iterateStereocenters())
			{
				if (stereo_center.xyz()[2] != 0)
					return false;

				List<IndigoObject> plainAtoms = new List<IndigoObject>();
				List<IndigoObject> stereoAtoms = new List<IndigoObject>();
				foreach (IndigoObject neighbor in stereo_center.iterateNeighbors())
				{
					if (neighbor.xyz()[2] != 0)
						return false;

					if (neighbor.bond().bondStereo() == Indigo.DOWN || neighbor.bond().bondStereo() == Indigo.UP)
						stereoAtoms.Add(neighbor);
					else plainAtoms.Add(neighbor);
				}

				if (plainAtoms.Count + stereoAtoms.Count > 3)
					return false;

				if (stereoAtoms.Count > 2)
					return false;

				if (stereoAtoms.Count == 2)
				{
					double angle = Geometry.AngleBetweenThreePoints(stereo_center.xyz()[0], stereo_center.xyz()[1], stereo_center.xyz()[2],
						stereoAtoms[0].xyz()[0], stereoAtoms[0].xyz()[1], stereoAtoms[0].xyz()[2],
						stereoAtoms[1].xyz()[0], stereoAtoms[1].xyz()[1], stereoAtoms[1].xyz()[2]
					);
					if (angle <= 180 && angle > 178)
					{
						issues.Add(new Issue() { Code = "100.16" });
						return true;
					}
				}
				else if (plainAtoms.Count == 2)
				{
					double angle = Geometry.AngleBetweenThreePoints(stereo_center.xyz()[0], stereo_center.xyz()[1], stereo_center.xyz()[2],
						plainAtoms[0].xyz()[0], plainAtoms[0].xyz()[1], plainAtoms[0].xyz()[2],
						plainAtoms[1].xyz()[0], plainAtoms[1].xyz()[1], plainAtoms[1].xyz()[2]
					);
					if (angle <= 180 && angle > 178)
					{
						issues.Add(new Issue() { Code = "100.16" });
						return true;
					}
				}
			}
			return false;
		}


		/// <summary>
		/// ST-1.6
		/// </summary>
		/// <param name="mol"></param>
		public bool ContainsBadAlleneStereo(string mol, ICollection<Issue> issues = null)
		{
            if (issues == null) issues = new List<Issue>();
			IndigoObject mol_obj = s_indigo.loadMolecule(mol);
			List<string> bad_allene_stereo_SMARTS = new List<string>() { 
				"[*]\\[C](/[*])=[C]=[C](/[*])(\\[*])",
				"[*]\\[C]([*])=[C]=[C](/[*])([*])","[*][C](/[*])=[C]=[C](/[*])([*])","[*]\\[C]([*])=[C]=[C](\\[*])([*])"
			};

			foreach (string allene_smarts in bad_allene_stereo_SMARTS)
			{
				IndigoObject allene = s_indigo.loadSmarts(allene_smarts);
				int count = s_indigo.substructureMatcher(mol_obj).countMatches(allene);
				if (count > 0)
				{
					issues.Add(new Issue() { Code = "100.18" });
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// ST-0.5. Stereobonds between stereocenters. Stereo-capable centers are identified only by stereo bonds. 
		/// Page 1908 in Graphical Representation of Stereo Centers
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="issues"></param>
		/// <returns></returns>
		public bool ContainsStereoBondBetweenStereoCenters(string mol, ICollection<Issue> issues = null)
		{
			IndigoObject mol_obj = s_indigo.loadMolecule(mol);
			List<int> stereo_atom_indexes = new List<int>();
			//foreach (IndigoObject stereo_center in mol_obj.iterateStereocenters())
			//	stereo_atom_indexes.Add(stereo_center.index());
			foreach (IndigoObject bond in mol_obj.iterateBonds())
				if ((bond.bondStereo() == Indigo.UP || bond.bondStereo() == Indigo.DOWN) && !stereo_atom_indexes.Contains(bond.source().index()))
					stereo_atom_indexes.Add(bond.source().index());

			if (stereo_atom_indexes.Count() < 2)
				return false;

			foreach (IndigoObject bond in mol_obj.iterateBonds())
				if (bond.bondStereo() == Indigo.DOWN || bond.bondStereo() == Indigo.UP)
				{
					int index_dest = bond.destination().index();
					int index_source = bond.source().index();
					if (stereo_atom_indexes.Contains(index_dest) && stereo_atom_indexes.Contains(index_source))
					{
						issues.Add(new Issue() { Code = "100.14" });

						return true;
					}
				}
			return false;
		}

		public bool ContainsDoubleBondWithAdjacentWavyBond(string mol, ICollection<Issue> issues = null)
		{
            if (issues == null) issues = new List<Issue>();
			GenericMolecule m = MoleculeFactory.FromMolV2000(mol);

			//get atoms with wavy bonds
			List<int> atomsWithWavyBonds = new List<int>();
			foreach (KeyValuePair<int, Bond> b in m.IndexedBonds)
			{
				if (b.Value.order == BondOrder.Single && b.Value.bondStereo == BondStereo.Either)
					atomsWithWavyBonds.Add(b.Value.firstatomID);
			}

			if (atomsWithWavyBonds.Count == 0)
				return false;

			foreach (KeyValuePair<int, Bond> b in m.IndexedBonds)
			{
				if (b.Value.order == BondOrder.Double)
				{
					if (atomsWithWavyBonds.Contains(b.Value.firstatomID) || atomsWithWavyBonds.Contains(b.Value.secondatomID))
					{
						issues.Add(new Issue() { Code = "100.40" });
						return true;
					}
				}
			}

			return false;
		}
	}
}
