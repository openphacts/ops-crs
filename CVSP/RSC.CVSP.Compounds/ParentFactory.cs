using RSC.Compounds;
using com.ggasoftware.indigo;
using InChINet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
	public static class ParentFactory
	{
		static Indigo indigo;
		static ParentFactory()
		{
			indigo = new Indigo();
			indigo.setOption("ignore-stereochemistry-errors", "true");
			indigo.setOption("ignore-noncritical-query-features", "true");
			indigo.setOption("timeout", "60000");
		}

		public static ICollection<Parent> GenerateParents(CtMonad ctm)
		{
			var parents = new List<Parent>();

			List<string> nonStdInchiKeys = new List<string>();
			string non_std_input_inchi_key = InChIUtils.mol2InChIKey(ctm.ConnectionTable, InChINet.InChIFlags.CRS);
			if (String.IsNullOrEmpty(non_std_input_inchi_key))
				return parents;

			nonStdInchiKeys.Add(non_std_input_inchi_key);
			string parentInchi, parentInchiKey, parentMolfile;

			/*************** CHARGE PARENT ***********/
            var chargeneutral = StandardizationModulesCharges.NeutralizeCharges(ctm, InChIFlags.CRS);
            parentMolfile = chargeneutral.Item1.ConnectionTable;
            parentInchi = chargeneutral.Item2;
            parentInchiKey = chargeneutral.Item3;
			if (!String.IsNullOrEmpty(parentMolfile) && !String.IsNullOrEmpty(parentInchi) && !String.IsNullOrEmpty(parentInchiKey)
				&& !nonStdInchiKeys.Contains(parentInchiKey))
			{
				string[] stdInChI = InChIUtils.mol2inchiinfo(parentMolfile, InChINet.InChIFlags.Standard);
				parents.Add(new Parent()
				{
					Relationship = ParentChildRelationship.ChargeInsensitive,
					MolFile = parentMolfile,
					NonStdInChI = new InChI(parentInchi, parentInchiKey),
					StdInChI = new InChI(stdInChI[0], stdInChI[1]),
					Smiles = indigo.loadMolecule(parentMolfile).canonicalSmiles()
				});
				nonStdInchiKeys.Add(parentInchiKey);
			}

			/*************** ISOTOPE PARENT ***********/

			parentMolfile = StandardizationModules.ReplaceIsotopes(ctm,
				out parentInchi,
				out parentInchiKey,
				InChIFlags.CRS).ConnectionTable;
			if (!String.IsNullOrEmpty(parentMolfile) && !String.IsNullOrEmpty(parentInchi) && !String.IsNullOrEmpty(parentInchiKey)
			&& !nonStdInchiKeys.Contains(parentInchiKey))
			{
				string[] stdInChI = InChIUtils.mol2inchiinfo(parentMolfile, InChINet.InChIFlags.Standard);
				parents.Add(new Parent()
				{
					Relationship = ParentChildRelationship.IsotopInsensitive,
					MolFile = parentMolfile,
					NonStdInChI = new InChI(parentInchi, parentInchiKey),
					StdInChI = new InChI(stdInChI[0], stdInChI[1]),
					Smiles = indigo.loadMolecule(parentMolfile).canonicalSmiles()
				});
				nonStdInchiKeys.Add(parentInchiKey);
			}

			/*************** STEREO PARENT ***********/
			parentMolfile = StandardizationModulesStereo.RemoveStereo(ctm,
				out parentInchi,
				out parentInchiKey,
				InChIFlags.CRS).ConnectionTable;
			if (!String.IsNullOrEmpty(parentMolfile) && !String.IsNullOrEmpty(parentInchi) && !String.IsNullOrEmpty(parentInchiKey)
			&& !nonStdInchiKeys.Contains(parentInchiKey))
			{
				string[] stdInChI = InChIUtils.mol2inchiinfo(parentMolfile, InChINet.InChIFlags.Standard);
				parents.Add(new Parent()
				{
					Relationship = ParentChildRelationship.StereoInsensitive,
					MolFile = parentMolfile,
					NonStdInChI = new InChI(parentInchi, parentInchiKey),
					StdInChI = new InChI(stdInChI[0], stdInChI[1]),
					Smiles = indigo.loadMolecule(parentMolfile).canonicalSmiles()
				});
				nonStdInchiKeys.Add(parentInchiKey);
			}

			/*************** TAUTOMER PARENT ***********/
			string[] tautomericInchi = InChIUtils.mol2inchiinfo(ctm.ConnectionTable, InChINet.InChIFlags.CRS_Tautomer);
			if (InChIAnalyzer.MobileHydrogenPresent(parentInchi) && !nonStdInchiKeys.Contains(tautomericInchi[1]))
			{
				parents.Add(new Parent()
				{
					Relationship = ParentChildRelationship.TautomerInsensitive,
					TautomericInChI = new InChI(tautomericInchi[0], tautomericInchi[1])
				});
			}
			nonStdInchiKeys.Add(tautomericInchi[1]);

			//if no parents so far then skip the generation of a super parent
			if (!parents.Any())
				return parents;

			/*************** SUPER PARENT ***********/
			Parent superParent = GenerateSuperParent(ctm, true, InChIFlags.CRS_Tautomer);
			parents.Add(superParent);

			return parents;
		}

		public static Parent GenerateSuperParent(CtMonad ctm, bool enableUseOfTautomerParent, InChIFlags flags)
		{
            string inchi, inchiKey;
			var molecule = StandardizationModulesStereo.RemoveStereo(ctm, out inchi, out inchiKey, flags);
			var neutralized = StandardizationModulesCharges.NeutralizeCharges(molecule, flags);
			molecule = StandardizationModules.ReplaceIsotopes(neutralized.Item1, out inchi, out inchiKey, flags);
            var nonstdinchi = InChIUtils.mol2inchiinfo(molecule.ConnectionTable, InChIFlags.CRS);
            var stdinchi = InChIUtils.mol2inchiinfo(molecule.ConnectionTable, InChIFlags.Standard);
            return new Parent()
            {
                MolFile = molecule.ConnectionTable,
                NonStdInChI = new InChI(nonstdinchi[0], nonstdinchi[1]),
                StdInChI = new InChI(stdinchi[0], stdinchi[1]),
                Smiles = indigo.loadMolecule(molecule.ConnectionTable).canonicalSmiles(),
                Relationship = ParentChildRelationship.SuperInsensitive,
                TautomericInChI = InChIAnalyzer.MobileHydrogenPresent(inchi) ? new InChI(nonstdinchi[0], nonstdinchi[1]) : null
            };
		}
	}
}
