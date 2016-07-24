using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using com.ggasoftware.indigo;
using MoleculeObjects;
using OpenEyeNet;
using InChINet;

//using ChemSpider.Toolkit.ChemAxon;

namespace RSC.CVSP.Compounds
{
    public class StandardizationStereoModule : IStandardizationStereoModule
    {
        private static Indigo s_indigo;

		private readonly IValidationModule validationModule = null;

		public StandardizationStereoModule(IValidationModule validationModule)
        {
			if (validationModule == null)
				throw new ArgumentNullException("validationModule");

			this.validationModule = validationModule;

			s_indigo = new Indigo();
            s_indigo.setOption("ignore-stereochemistry-errors", "true");
            s_indigo.setOption("unique-dearomatization", "false");
            s_indigo.setOption("ignore-noncritical-query-features", "false");
            s_indigo.setOption("timeout", "600000");
        }

        public string AddChiralFlag(string mol)
        {
            string newsdf = String.Empty;
            using (StringReader reader = new StringReader(mol))
            {
                string line;
                int countline = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    countline++;
                    // Do something with the line whe it is count line
                    if (countline == 4)
                    {
                        StringBuilder builder = new StringBuilder(line);
                        builder[14] = '1';
                        line = builder.ToString();
                    }
                    newsdf += line + Environment.NewLine;
                }
            }
            return newsdf;
        }

        /// <summary>
        /// this does it willy-nilly
        /// </summary>
        public string RemoveChiralFlag(string mol)
        {
            var lines = mol.SplitOnNewLines();
            var header = lines.Take(3);
            string countline = lines.Skip(3).First();
            var tail = lines.Skip(4);
            string newcountline = String.Concat(countline.Take(14).Concat(new[] { '0' }).Concat(countline.Skip(15)));
            var newlines = header.Concat(new string[] { newcountline }).Concat(tail);
            return String.Join(Environment.NewLine, header.Concat(new string[] { newcountline }).Concat(tail));
        }

        /// <summary>
        /// Converts bond stereo on double bonds from CisTransEither to None.
        /// </summary>
        public string ConvertEitherBondsToDefined(string molfile)
        {
            if (validationModule.ContainsEitherBond(molfile))
            {
                GenericMolecule gm = MoleculeFactory.FromMolFile(molfile,ChemicalFormat.V2000,null);
                foreach(KeyValuePair<int,Bond> b in gm.IndexedBonds)
                {
                    if(b.Value.order == BondOrder.Double && b.Value.bondStereo == BondStereo.CisTransEither)
                        b.Value.bondStereo = BondStereo.None;
                }
                if (!validationModule.ContainsEitherBond(gm.ToString()))
                    return gm.ToString();
            }
            return molfile;
        }

        /// <summary>
        /// This converts double bonds with a given regiochemistry to double bonds without, willy-nilly.
        /// </summary>
        public string ConvertDoubleBondsToEither(string molfile)
        {
            IndigoObject obj = s_indigo.loadMolecule(molfile);
            string smiles = obj.canonicalSmiles();
            string eitherBond_smiles = smiles.Replace("/", "").Replace("\\", "");
            IndigoObject new_obj = s_indigo.loadMolecule(eitherBond_smiles);
            new_obj.markEitherCisTrans();
            new_obj.layout();

            if (!obj.canonicalSmiles().Equals(eitherBond_smiles))
                return new_obj.molfile();
            else return molfile;
        }

        public string RemoveAlleneStereo(string molfile)
        {
            IndigoObject obj = s_indigo.loadMolecule(molfile);
            string input_smiles = obj.canonicalSmiles();
            obj.clearAlleneCenters();
            if (!obj.canonicalSmiles().Equals(input_smiles))
                return obj.molfile();
            else return molfile;
        }

        public string RemoveSP3Stereo(string molfile)
        {
            IndigoObject obj = s_indigo.loadMolecule(molfile);
            string input_smiles = obj.canonicalSmiles();
            obj.clearStereocenters();
            if (!input_smiles.Equals(obj.canonicalSmiles()))
                return obj.molfile();
            else return molfile;
        }

        public string ConvertUpOrDownBondsAdjacentToDoubleBondToNoStereoSingleBondsWithCrossedDoubleBond(string mol)
        {
            IndigoObject obj = s_indigo.loadMolecule(mol);
            string smiles_before = obj.smiles();
            bool isStereoReset = false;
            List<int> stereoCenters = new List<int>();
            foreach (IndigoObject stereoCenterAtom in obj.iterateStereocenters())
                stereoCenters.Add(stereoCenterAtom.index());

			//for checking that double bond does not belong to allene
			List<int> alleneCenters = new List<int>();
			foreach (IndigoObject stereoCenterAtom in obj.iterateAlleneCenters())
				alleneCenters.Add(stereoCenterAtom.index());
			

            foreach (IndigoObject bond in obj.iterateBonds())
            {

                if (bond.bondOrder() == 2)
                {
                    IndigoObject bond_source_atom = bond.source();
                    IndigoObject bond_destination_atom = bond.destination();
					
					//if double bond belongs to allene - skip
					if (alleneCenters.Contains(bond_source_atom.index()) || alleneCenters.Contains(bond_destination_atom.index()))
						continue;

                    foreach (IndigoObject source_neigh in bond_source_atom.iterateNeighbors())
                    {
                        if (source_neigh.bond().bondOrder() == 1 && (source_neigh.bond().bondStereo() == Indigo.UP || source_neigh.bond().bondStereo() == Indigo.DOWN)
                            && !stereoCenters.Contains(source_neigh.index()))
                        {
                            bond.resetStereo();
                            source_neigh.bond().resetStereo();
                            isStereoReset = true;
                        }
                    }
                    foreach (IndigoObject destination_neigh in bond_destination_atom.iterateNeighbors())
                    {
                        if (destination_neigh.bond().bondOrder() == 1 && (destination_neigh.bond().bondStereo() == Indigo.UP || destination_neigh.bond().bondStereo() == Indigo.DOWN)
                            && !stereoCenters.Contains(destination_neigh.index()))
                        {
                            bond.resetStereo();
                            destination_neigh.bond().resetStereo();
                            isStereoReset = true;
                        }
                    }
                }
            }

            if (isStereoReset)
                return obj.molfile();
            else return mol;
        }

        public string ConvertDoubleBondWithAttachedEitherSingleBondStereoToEitherDoubleBond(string mol)
        {
            IndigoObject obj = s_indigo.loadMolecule(mol);
            string smiles_before = obj.smiles();
            bool isStereoReset = false;
            foreach (IndigoObject bond in obj.iterateBonds())
            {

                if (bond.bondOrder() == 2)
                {
                    IndigoObject bond_source_atom = bond.source();
                    IndigoObject bond_destination_atom = bond.destination();
                    foreach (IndigoObject source_neigh in bond_source_atom.iterateNeighbors())
                    {
                        if (source_neigh.bond().bondOrder() == 1 && source_neigh.bond().bondStereo() == Indigo.EITHER)
                        {
                            bond.resetStereo();
                            source_neigh.bond().resetStereo();
                            isStereoReset = true;
                        }
                    }
                    foreach (IndigoObject destination_neigh in bond_destination_atom.iterateNeighbors())
                    {
                        if (destination_neigh.bond().bondOrder() == 1 && destination_neigh.bond().bondStereo() == Indigo.EITHER)
                        {
                            bond.resetStereo();
                            destination_neigh.bond().resetStereo();
                            isStereoReset = true;
                        }
                    }
                }
            }
            if (isStereoReset)
            {
                string smiles_after = obj.smiles();
                return obj.molfile();
            }
            else return mol;
        }

		/// <summary>
        /// no stereo bonds (up or down) but chiral flag is set
        /// in such cases OE Layout puts chiral flag for some reason
        /// </summary>
        /// <param name="molfile"></param>
        /// <returns></returns>
        public string ClearChiralFlagOnFlatStructure(string molfile)
        {
            IndigoObject mol_obj = s_indigo.loadMolecule(molfile);
            if (mol_obj.countStereocenters() == 0)
            {
                return RemoveChiralFlag(molfile);
            }
            return molfile;
        }

        /// <summary>
        ///  to clear stereocenters that are not real stereocenters like in `CC[C@@H](CN)CC`
        /// </summary>
        /// <param name="mol"></param>
        public string ResetSymmetricStereoCenters(string mol)
        {
            IndigoObject obj = s_indigo.loadMolecule(mol);
            int symmetricStereoCount = obj.resetSymmetricStereocenters();
            if (symmetricStereoCount > 0)
            {
                Trace.TraceInformation(symmetricStereoCount + " symmetric stereo center(s) removed");
                return obj.molfile();
            }
            return mol;
        }

        /// <summary>
        ///  to clear stereocenters that are not real stereocenters like in `CC[C@@H](CN)CC`
        /// </summary>
        /// <param name="mol"></param>
        public string ResetSymmetricCisTrans(string mol)
        {
            IndigoObject obj = s_indigo.loadMolecule(mol);
            int SymmetricCisTransCount = obj.resetSymmetricCisTrans();
            if (SymmetricCisTransCount > 0)
            {
                Trace.TraceInformation(SymmetricCisTransCount + " symmetric Cis/Trans bonds reset");
                return obj.molfile();
            }
            return mol;
        }
    }
}
