using System;
using com.ggasoftware.indigo;
using System.Collections.Generic;
using System.Linq;

using InChINet;
using RSC.Compounds;
using MoleculeObjects;

namespace RSC.CVSP.Compounds.Operations
{
	public class CalculateParents : OperationBase
	{
		private readonly IStandardizationModule standardizationModule = null;
		private readonly IStandardizationChargesModule standardizationChargesModule = null;
        static Indigo indigo;

        /// <summary>
        /// Apply this to all SMILES as different vendors may or may not append something in pipes after a space.
        /// </summary>
        public string FirstToken(string s)
        {
            return s.Split(new[] { ' ' }).First();
        }

		public CalculateParents(IStandardizationModule standardizationModule, IStandardizationChargesModule standardizationChargesModule)
		{
			if (standardizationModule == null)
				throw new ArgumentNullException("standardizationModule");

			if (standardizationChargesModule == null)
				throw new ArgumentNullException("standardizationChargesModule");

			this.standardizationModule = standardizationModule;
			this.standardizationChargesModule = standardizationChargesModule;

            indigo = GetIndigo();
		}

		public override void ProcessRecord(Record record, IDictionary<string, object> options = null)
		{
			var compound = record as CompoundRecord;

			if (compound == null)
				return;

			compound.AddParents(GenerateParents(record.Standardized));
		}

		public override IEnumerable<OperationInfo> GetOperations()
		{
			return new List<OperationInfo>() {
				new OperationInfo() {
					Id = "ParentsCalculation",
					Name = "Parents Calculation",
					Description = "Calculate compound parents"
				}
			};
		}

        public ICollection<Parent> GenerateChargeParents(string molfile)
        {
            var parents = new List<Parent>();
            string parentInchi, parentInchiKey, parentMolfile;

            //*************** CHARGE PARENT ***********
            parentMolfile = standardizationChargesModule.NeutralizeCharges(molfile,
                out parentInchi,
                out parentInchiKey,
                InChIFlags.CRS);
            if (!String.IsNullOrEmpty(parentMolfile) && !String.IsNullOrEmpty(parentInchi) && !String.IsNullOrEmpty(parentInchiKey))
            {
                string[] stdInChI = InChIUtils.mol2inchiinfo(parentMolfile, InChINet.InChIFlags.Standard);
                parents.Add(new Parent()
                {
                    Relationship = ParentChildRelationship.ChargeInsensitive,
                    MolFile = parentMolfile,
                    NonStdInChI = new InChI(parentInchi, parentInchiKey),
                    StdInChI = new InChI(stdInChI[0], stdInChI[1]),
                    Smiles = FirstToken(indigo.loadMolecule(parentMolfile).canonicalSmiles())
                });
            }
            return parents;
        }

        public ICollection<Parent> GenerateFragmentParents(string molfile)
        {
            var parents = new List<Parent>();
            IndigoObject obj = indigo.loadMolecule(molfile);
            Console.WriteLine(obj.countComponents());
            //*************** FRAGMENT PARENT ***********
            if (obj.countComponents() > 1)
            {
                foreach (IndigoObject fragment in obj.iterateComponents())
                {
                    IndigoObject ifragment = fragment.clone();
                    if (string.IsNullOrEmpty(ifragment.molfile()))
                        continue;

                    //add fragment as a parent
                    string[] nonstd_inchi = InChIUtils.mol2inchiinfo(ifragment.molfile(), InChINet.InChIFlags.CRS);
                    string[] std_inchi = InChINet.InChIUtils.mol2inchiinfo(ifragment.molfile(), InChINet.InChIFlags.Standard);

                    parents.Add(new Parent()
                    {
                        Relationship = ParentChildRelationship.Fragment,
                        MolFile = ifragment.molfile(),
                        NonStdInChI = new InChI(nonstd_inchi[0], nonstd_inchi[1]),
                        StdInChI = new InChI(std_inchi[0], std_inchi[1]),
                        Smiles = FirstToken(ifragment.canonicalSmiles())
                    });

                    //add parents of the fragment itself
                    parents.AddRange(GenerateParents(ifragment.molfile()));
                }
            }
            return parents;
        }

        public ICollection<Parent> GenerateIsotopeParents(string molfile)
        {
            var parents = new List<Parent>();
            //*************** ISOTOPE PARENT ***********
            string parentInchi, parentInchiKey;
            string parentMolfile = standardizationModule.ReplaceIsotopes(molfile,
                out parentInchi,
                out parentInchiKey,
                InChIFlags.CRS);
            if (!String.IsNullOrEmpty(parentMolfile) && !String.IsNullOrEmpty(parentInchi) && !String.IsNullOrEmpty(parentInchiKey))
            {
                string[] stdInChI = InChIUtils.mol2inchiinfo(parentMolfile, InChINet.InChIFlags.Standard);
                parents.Add(new Parent()
                {
                    Relationship = ParentChildRelationship.IsotopInsensitive,
                    MolFile = parentMolfile,
                    NonStdInChI = new InChI(parentInchi, parentInchiKey),
                    StdInChI = new InChI(stdInChI[0], stdInChI[1]),
                    Smiles = FirstToken(indigo.loadMolecule(parentMolfile).canonicalSmiles())
                });
            }
            return parents;
        }

        public ICollection<Parent> GenerateStereoParents(string molfile)
        {
            var parents = new List<Parent>();
            string parentInchi, parentInchiKey;
            //*************** STEREO PARENT ***********
            string parentMolfile = standardizationModule.RemoveStereo(molfile,
                out parentInchi,
                out parentInchiKey,
                InChIFlags.CRS);
            if (!String.IsNullOrEmpty(parentMolfile) && !String.IsNullOrEmpty(parentInchi) && !String.IsNullOrEmpty(parentInchiKey))
            {
                string[] stdInChI = InChIUtils.mol2inchiinfo(parentMolfile, InChINet.InChIFlags.Standard);
                parents.Add(new Parent()
                {
                    Relationship = ParentChildRelationship.StereoInsensitive,
                    MolFile = parentMolfile,
                    NonStdInChI = new InChI(parentInchi, parentInchiKey),
                    StdInChI = new InChI(stdInChI[0], stdInChI[1]),
                    Smiles = FirstToken(indigo.loadMolecule(parentMolfile).canonicalSmiles())
                });
            }
            return parents;
        }

        public ICollection<Parent> GenerateTautomerParents(string molfile)
        {
            var parents = new List<Parent>();
            //*************** TAUTOMER PARENT ***********
            string[] tautomericInchi = InChIUtils.mol2inchiinfo(molfile, InChINet.InChIFlags.CRS_Tautomer);
            string[] stdInChI = InChIUtils.mol2inchiinfo(molfile, InChINet.InChIFlags.Standard);
            if (IsMobileHydrogenPresent(tautomericInchi[0]))
            {
                parents.Add(new Parent()
                {
                    // 2015-09-18: on advice from Valery:
                    MolFile = null,
                    Relationship = ParentChildRelationship.TautomerInsensitive,
                    NonStdInChI = new InChI(tautomericInchi[0], tautomericInchi[1]),
                    TautomericInChI = new InChI(tautomericInchi[0], tautomericInchi[1]),
                    StdInChI = new InChI(stdInChI[0], stdInChI[1]),
                    Smiles = FirstToken(indigo.loadMolecule(molfile).canonicalSmiles())
                });
            }
            return parents;
        }

		public ICollection<Parent> GenerateParents(string molfile)
		{
            var parents = new List<Parent>();
            GenericMolecule gm = MoleculeFactory.FromMolV2000(molfile);
            string[] inchiInfo = InChIUtils.mol2inchiinfo(molfile, InChIFlags.CRS);
            string inchi = inchiInfo[0];
            string inchiKey = inchiInfo[1];
            if (String.IsNullOrEmpty(inchiKey)) return parents;
            
            parents.AddRange(GenerateFragmentParents(molfile));
            // inspect individual atoms because InChI only reflects overall charge
            if (gm.IndexedAtoms.Any(a => a.Value.Charge != 0)) parents.AddRange(GenerateChargeParents(molfile));
            if (inchi.Contains("/i")) parents.AddRange(GenerateIsotopeParents(molfile));
            if (inchi.Contains("/t")) parents.AddRange(GenerateStereoParents(molfile));
            if (IsMobileHydrogenPresent(inchi)) parents.AddRange(GenerateTautomerParents(molfile));
			//if no parents so far then skip the generation of a super parent
			if (!parents.Any())	return parents;
			parents.AddRange(GenerateSuperParents(molfile));
			return parents;
		}

        /// <summary>
        /// 2015-10-01: return empty list if charge neutralization fails
        /// </summary>
		public ICollection<Parent> GenerateSuperParents(string mol)
		{
            var parents = new List<Parent>();
            InChIFlags flags = InChIFlags.CRS_Tautomer;
            string inchi, inchiKey;
            string molecule = standardizationModule.RemoveStereo(mol, out inchi, out inchiKey, flags);
            molecule = inchi.Contains("/p") || inchi.Contains("/q")
               ? standardizationChargesModule.NeutralizeCharges(molecule, out inchi, out inchiKey, flags)
               : molecule;
            if (molecule == null) return parents;
			molecule = standardizationModule.ReplaceIsotopes(molecule, out inchi, out inchiKey, flags);

            //*************** SUPER PARENT ***********
            string superParentMolfile = molecule;
            string[] output_non_std_inchi = InChIUtils.mol2inchiinfo(superParentMolfile, InChIFlags.CRS);
            Parent superParent = new Parent()
            {
                Relationship = ParentChildRelationship.SuperInsensitive,
                Smiles = FirstToken(indigo.loadMolecule(superParentMolfile).canonicalSmiles())
            };
            if (!IsMobileHydrogenPresent(inchi))//happens when there were mobile hydrogens
            {
                superParent.MolFile = superParentMolfile;
                superParent.NonStdInChI = new InChI(output_non_std_inchi[0], output_non_std_inchi[1]);
                string[] stdInchi = InChIUtils.mol2inchiinfo(superParent.MolFile, InChIFlags.Standard);
                superParent.StdInChI = new InChI(stdInchi[0], stdInchi[1]);
            }
            else
            {
                string[] inputStdInChI = InChIUtils.mol2inchiinfo(mol, InChIFlags.Standard);
                superParent.NonStdInChI = new InChI(output_non_std_inchi[0], output_non_std_inchi[1]);
                superParent.StdInChI = new InChI(inputStdInChI[0], inputStdInChI[1]);
                superParent.MolFile = mol;
                superParent.TautomericInChI = new InChI(inchi, inchiKey);
            }
            parents.Add(superParent);
            return parents;
		}

		/// <summary>
		/// takes inchi string and checks for mobile hydrogens
		/// </summary>
		/// <param name="inchi"></param>
		/// <returns></returns>
		public bool IsMobileHydrogenPresent(string inchi)
		{
            return inchi.Contains("(H");
		}
	}
}
