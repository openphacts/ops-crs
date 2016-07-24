using com.ggasoftware.indigo;
using InChINet;
using MoleculeObjects;
using OpenEyeNet;
using RSC.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RSC.CVSP.Compounds
{
	public class StandardizationModule : IStandardizationModule
	{
		private List<string> s_metalSymbols;

		private OpenEyeUtility s_utility = OpenEyeUtility.GetInstance();
		private Indigo s_indigo = new Indigo();
        private Reactor s_reactor = new Reactor();

		private readonly IValidationModule validationModule = null;
		private readonly IValidationStereoModule validationStereoModule = null;
		private readonly IStandardizationStereoModule standardizationStereoModule = null;

		public StandardizationModule(IValidationModule validationModule, IValidationStereoModule validationStereoModule, IStandardizationStereoModule standardizationStereoModule)
		{
			if (validationModule == null)
				throw new ArgumentNullException("validationModule");

			if (validationStereoModule == null)
				throw new ArgumentNullException("validationStereoModule");

			if (standardizationStereoModule == null)
				throw new ArgumentNullException("standardizationStereoModule");

			this.validationModule = validationModule;
			this.validationStereoModule = validationStereoModule;
			this.standardizationStereoModule = standardizationStereoModule;

			s_metalSymbols = new List<string>();
			s_metalSymbols.AddRange(AtomicProperties.Group1Atoms);
			s_metalSymbols.AddRange(AtomicProperties.Group2Atoms);
			s_metalSymbols.AddRange(AtomicProperties.TransitionMetalsLessHg);
			s_metalSymbols.AddRange(AtomicProperties.Lanthanides);
			s_metalSymbols.AddRange(AtomicProperties.Actinides);
			s_metalSymbols.AddRange(AtomicProperties.pBlockMetals);

			s_indigo.setOption("ignore-stereochemistry-errors", "true");
			s_indigo.setOption("unique-dearomatization", "false");
			s_indigo.setOption("ignore-noncritical-query-features", "true");
			s_indigo.setOption("timeout", "600000");
			s_indigo.setOption("molfile-saving-mode", "auto");//for trying to save in v2000
		}

        public string ApplyReactions(string molfile, IList<string> reactions)
        {
            lock (s_indigo)
            {
                IndigoObject input_obj = s_indigo.loadMolecule(molfile);

                string input_smiles, output_smiles;
                foreach (string reaction in reactions)
                {
                    input_smiles = input_obj.canonicalSmiles();
                    IndigoObject reaction_obj = s_indigo.loadReactionSmarts(reaction);
                    s_indigo.transform(reaction_obj, input_obj);
                    output_smiles = input_obj.canonicalSmiles();
                }
                return input_obj.molfile();
            }
        }

		public string TreatAmmonia(string molFile)
		{
			List<string> reactions = new List<string>();
			reactions.Add("[N:1]([H:2])([H:3])[H:4].[H:5][F,Cl,Br,I:6]>>[N+:1]([H:2])([H:3])([H:4])[H:5].[F-,Cl-,Br-,I-:6]");//NH3 + HCl-> NH4+ + Cl-
			reactions.Add("[N+0;H3:1].[C:3](=[O:4])[O:5][H:6]>>[N+1;H4:1].[C:3](=[O:4])[O-:5]");//carbonic acid + ammonia into salt
			reactions.Add("[H:2][N+:1]([C,H:3])([C:4])[C:5].[F-,Cl-,Br-,I-:6]>>[N:1]([C,H:3])([C:4])[C:5].[H:2][F,Cl,Br,I:6]");//threeethylamine hydrochloride
			reactions.Add("[H:1][N+:3]=[C:2].[O:8][S:4]([O-:7])(=[O:5])=[O:6]>>[H:1][O:7][S:4]([O:8])(=[O:5])=[O:6].[C:2]=[N:3]");//aminoguanidine sulfate
			return ApplyReactions(molFile, reactions);
		}

		public string RemoveStereo(string molFile, out string newInchi, out string newInchiKey, InChIFlags flags, IList<Issue> issueCollection = null)
		{
			newInchi = String.Empty;
			newInchiKey = String.Empty;
			try
			{
				IndigoObject nostereo = s_indigo.loadMolecule(molFile);

				//nostereo.markEitherCisTrans();
				nostereo.clearStereocenters();
				nostereo.clearAlleneCenters();
				//replacing stereo on double bonds to either type
				nostereo.clearCisTrans();
				nostereo.markEitherCisTrans();
				//fold hydrogens
				string newmol = FoldNonStereoHydrogens(nostereo.molfile());
				string[] output_stereo_non_std_inchi = InChIUtils.mol2inchiinfo(newmol, flags);
				newInchi = output_stereo_non_std_inchi[0];
				newInchiKey = output_stereo_non_std_inchi[1];
				return newmol;
			}
			catch (Exception ex)
			{
				issueCollection.Add(new Issue
				{
					Code = "400.7",
					AuxInfo = ex.StackTrace,
					Message = ex.Message
				});
				Trace.TraceError("Stereo removal failed:" + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
				return molFile;
			}
		}

		public string RemoveAmbiguousSp3Stereo(string molFile)
		{
			IndigoObject obj = s_indigo.loadMolecule(molFile);
			foreach (IndigoObject bond in obj.iterateBonds())
			{
				if (bond.bondOrder() == 1 && (bond.bondStereo() == Indigo.UP || bond.bondStereo() == Indigo.DOWN))
				{
					IndigoObject source = bond.source();
					IndigoObject destination = bond.destination();
					int countNeighbors = 0;
					foreach (IndigoObject neighbor in source.iterateNeighbors())
						countNeighbors++;
					if (destination.symbol() == "H" && countNeighbors < 4)
						bond.resetStereo();
					else if (countNeighbors < 3)
						bond.resetStereo();
				}
			}
			return obj.molfile();
		}

		public string FoldNonStereoHydrogens(string molFile)
		{
			IndigoObject obj = s_indigo.loadMolecule(molFile);
			string input_smiles = obj.canonicalSmiles();
			Dictionary<int, IndigoObject> H_atoms = new Dictionary<int, IndigoObject>();
			foreach (IndigoObject bond in obj.iterateBonds())
			{
				IndigoObject H_atom = null;
				IndigoObject H_connected_atom = null;
				string source = bond.source().symbol();
				string destination = bond.destination().symbol();
				//bond.source().isotope() == 1: so it wouldn't affect Deuterium and Tritium
				if (bond.source().symbol().Equals("H") && (bond.source().isotope() == 1 || bond.source().isotope() == 0) && bond.source().charge() == 0)
				{
					H_atom = bond.source();
					H_connected_atom = bond.destination();
				}
				else if (bond.destination().symbol().Equals("H") && (bond.destination().isotope() == 1 || bond.destination().isotope() == 0) && bond.destination().charge() == 0)
				{
					H_atom = bond.destination();
					H_connected_atom = bond.source();
				}
				if (H_atom != null && H_connected_atom != null)
				{
					//iterate neighbors ot see if they are in double bond
					//if they are then check that that neighbor is not connected to other hydrogen
					foreach (IndigoObject neighb in H_connected_atom.iterateNeighbors())
					{
						//if 2 hydrogens are connected to same H_connected_atom then can fold (even if H bonds are stereo)
						if (neighb.index() != H_atom.index() && neighb.symbol() == "H")
						{
							if (!H_atoms.ContainsKey(H_atom.index()))
								H_atoms.Add(H_atom.index(), H_atom);
							if (!H_atoms.ContainsKey(neighb.index()))
								H_atoms.Add(neighb.index(), neighb);
							continue;
						}
						//if for example H-X- matched- remove H
						if (neighb.index() != H_atom.index() && neighb.symbol() != "H" && neighb.bond().bondOrder() == 1 && bond.bondStereo() != Indigo.UP && bond.bondStereo() != Indigo.DOWN)
						{
							if (!H_atoms.ContainsKey(H_atom.index()))
								H_atoms.Add(H_atom.index(), H_atom);
							continue;
						}
					}
				}
			}
			//remove H
			foreach (KeyValuePair<int, IndigoObject> kv in H_atoms)
				kv.Value.remove();
			if (H_atoms.Count > 0)
			{
				Trace.TraceInformation("Non stereo hydrogens were folded");
			}
			return obj.molfile();
		}

		public string Kekulize(string molFile)
		{
			if (validationModule.ContainsNotKekulizedAromaticRings(molFile))
			{
				IndigoObject obj = s_indigo.loadMolecule(molFile);
				obj.dearomatize();
				return obj.molfile();
			}
			return molFile;
		}

		public string Aromatize(string molFile)
		{
			IndigoObject layout_obj = s_indigo.loadMolecule(molFile);
			string input_smiles = layout_obj.canonicalSmiles();
			layout_obj.aromatize();
			string output_smiles = layout_obj.canonicalSmiles();
			if (!input_smiles.Equals(output_smiles))
				return layout_obj.molfile();
			return molFile;
		}

		public bool ShouldRunTautomerCanonicalizer(string molFile, int tautomerCanonicalizationAtomLimit)
		{
			GenericMolecule gm = MoleculeFactory.FromMolV2000(molFile);
            return gm.IndexedAtoms.Count <= tautomerCanonicalizationAtomLimit;
		}

        /// <summary>
        /// Also produces new InChI and new InChIKey, which seems odd.
        /// </summary>
		public string ReplaceIsotopes(string molFile, out string newInchi, out string newInchiKey, InChIFlags flags)
		{
			newInchi = String.Empty;
			newInchiKey = String.Empty;
			try
			{
				IndigoObject molecule = s_indigo.loadMolecule(molFile);
				string[] input_isotopeInchi = InChIUtils.mol2inchiinfo(molFile, flags);

				//molecule.markEitherCisTrans();
				foreach (IndigoObject atom in molecule.iterateAtoms())
					atom.resetIsotope();
				//molecule.dearomatize();
				string[] output_isotopeInchi = InChIUtils.mol2inchiinfo(molecule.molfile(), flags);

				newInchi = output_isotopeInchi[0];
				newInchiKey = output_isotopeInchi[1];
				if (!input_isotopeInchi[1].Equals(output_isotopeInchi[1]))
					return molecule.molfile();
				else return molFile;

			}
			catch (Exception ex)
			{
				Trace.TraceError("Isotope-unsensitive parent generation failed: " + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
				return molFile;
			}

		}

		/// <summary>
		///returns molfile if vendor is ChemAxon and InChIs with flag InChIFlags.CRS_Tautomer;
		/// if vendor is Indigo - returns null (molfile) and InChIs with flag InChIFlags.CRS
		/// </summary>
		/// <param name="molFile"></param>
		/// <param name="isCRSDeposition"></param>
		/// <param name="issueCollection"></param>
		/// <param name="TransformationCollection"></param>
		/// <returns></returns>
		public string CanonicalizeTautomer(string molFile, bool isCRSDeposition,
			Resources.Vendor vendor,
			out string newInchi,
			out string newInchiKey,
			InChIFlags flags,
			IList<Issue> issueCollection = null)
		{
			newInchi = String.Empty;
			newInchiKey = String.Empty;
			if (issueCollection == null)
				issueCollection = new List<Issue>();
			try
			{
				if (vendor == Resources.Vendor.ChemAxon)
				{
                    throw new Exception("not supporting ChemAxon tautomer canonicalization");
					//string input_smiles = mol_obj.canonicalSmiles();
					string errors = String.Empty, warnings = String.Empty;
					string output_molfile = null;	// Links to ChemAxon severed - ChemAxon.CanonicalizeTautomer(molFile, isCRSDeposition, ref errors, ref warnings);

					if (!String.IsNullOrEmpty(errors))
					{
						issueCollection.Add(new Issue
						{
							Code = "400.8",
							Message = errors
						});
					}

					if (!String.IsNullOrEmpty(warnings))
					{
						issueCollection.Add(new Issue
						{
							Code = "400.9",
							Message = warnings
						});
					}

					if (!String.IsNullOrEmpty(output_molfile))
					{
						string fixed_mol = standardizationStereoModule.ConvertDoubleBondWithAttachedEitherSingleBondStereoToEitherDoubleBond(output_molfile);
						string[] output_non_std_inchi = InChIUtils.mol2inchiinfo(fixed_mol, flags);
						newInchi = output_non_std_inchi[0];
						newInchiKey = output_non_std_inchi[1];
						return fixed_mol;
					}

					return molFile;
				}
				else return null;
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.Message + Environment.NewLine + ex.StackTrace);
				issueCollection.Add(new Issue { 
					Code = "400.8",
					Message = ex.Message,
					AuxInfo = ex.StackTrace
				});
				Trace.TraceWarning("Tautomer-canonicalization failed:" + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
				return molFile;
			}
		}

		public string Layout(string molFile, Resources.Vendor vendor, out bool hasStandardInchiChanged, IList<Issue> issueCollection = null)
		{
			try
			{
				hasStandardInchiChanged = false;
				IndigoObject input = s_indigo.loadMolecule(molFile);
				//temporary path while OE fixed their bug with clean function that converts relative stereo to absolute
				if (validationStereoModule.ContainsRelativeStereo(molFile))
				{
					Trace.TraceInformation("Relative stereo detected: switching from OE layout to Indigo as OE convertes them to absolute stereo");
					vendor = Resources.Vendor.Indigo;
				}

				string mol_out = String.Empty;
				if (vendor == Resources.Vendor.Indigo)
				{
					Trace.TraceInformation("Performing Indigo layout");
					IndigoObject layout_obj = s_indigo.loadMolecule(molFile);
					layout_obj.layout();
					mol_out = layout_obj.molfile();
				}
				else if (vendor == Resources.Vendor.OpenEye)
				{
					Trace.TraceInformation("Performing OE layout");
					try
					{
						mol_out = s_utility.Clean(molFile, false);
						//sometimes OE sets chiral flag on when no up  or down onds are present
						//clear possible chiral flag
						mol_out = standardizationStereoModule.ClearChiralFlagOnFlatStructure(mol_out);
					}
					catch (Exception ex)
					{
						Trace.TraceError(ex.Message + "\n" + ex.StackTrace);
						Trace.TraceInformation(ex.Message + "\n" + ex.StackTrace);
						vendor = Resources.Vendor.Indigo;
						Trace.TraceInformation("OE layout exception.. switching to Indigo layout");
						IndigoObject layout = input.clone();
						layout.layout();
						mol_out = layout.molfile();
					}
				}
				string input_inchi = InChIUtils.mol2InChI(molFile, InChIFlags.CRS);
				string output_inchi = InChIUtils.mol2InChI(mol_out, InChIFlags.CRS);
				if (!input_inchi.Equals(output_inchi))
				{
					Trace.TraceError(vendor.ToString() + " layout affected InChI: " + input_inchi + " >> " + output_inchi);

					hasStandardInchiChanged = true;
				}
				return mol_out;
			}
			catch (IndigoException ex)
			{
				Trace.TraceInformation("Layout : " + ex.Message + Environment.NewLine + ex.StackTrace);
				Trace.TraceError("Layout : " + ex.Message + Environment.NewLine + ex.StackTrace);
				Issue i = ex.ParseIndigoException();
				if (!issueCollection.Contains(i))
					issueCollection.Add(i);
				if (i.Code == "200.18")
					issueCollection.Add(new Issue { Code = "400.4" });

				hasStandardInchiChanged = false;
				return molFile;
			}
		}

		public string StandardizeByInChIRules(string input_mol)
		{
			lock (s_indigo)
			{
				IndigoObject input = s_indigo.loadMolecule(input_mol);
				//IndigoObject output = input.clone();
				string input_smiles = input.canonicalSmiles();
				IndigoInchi i_inchi = new IndigoInchi(s_indigo);
				s_indigo.setOption("inchi-options", "");

				string input_inchi = i_inchi.getInchi(input);
				//string input_inchi_key = i_inchi.getInchiKey(input_inchi);

				IndigoObject output = i_inchi.loadMolecule(input_inchi);

				string output_smiles = output.canonicalSmiles();

				//string output_inchi = i_inchi.getInchi(output);
				//string output_inchi_key = i_inchi.getInchiKey(output_inchi);
				if (!output_smiles.Equals(input_smiles))
					return output.molfile();
				return input_mol;
			}
		}

		public string FoldAllHydrogens(string molfile)
		{
			lock (s_indigo)
			{
				IndigoObject obj = s_indigo.loadMolecule(molfile);
				obj.foldHydrogens();
                return obj.molfile();
			}
		}
	}
}
