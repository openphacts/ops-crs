using InChINet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using ChemSpider.Molecules;
using ChemSpider.Utilities;
using RSC.Compounds.Search;

namespace RSC.Compounds.EntityFramework
{
	public class EFCompoundConvert : ICompoundConvert
	{
		/// <summary>
		/// Convert InChI to Compound ID
		/// </summary>
		/// <param name="inchi">Standard on Non standard InChI</param>
		/// <returns>Internal compound ID</returns>
		public IEnumerable<Guid> InChIToCompoundId(string inchi)
		{
			using (var db = new CompoundsContext())
			{
				string[] inchi_layers = null;
				if (inchi.StartsWith("InChI="))
					inchi_layers = InChIUtils.getInChILayers(inchi);
				else if (InChIUtils.isValidInChI(inchi))
				{
					string message;
					inchi = ChemIdUtils.anyId2InChI(inchi, out message, InChIFlags.v104);
					if (!String.IsNullOrEmpty(inchi))
						inchi_layers = InChIUtils.getInChILayers(inchi);
				}

				// We have InChI - let's use it
				if (inchi_layers != null)
				{
					// Standard InChI
					byte[] hash = inchi_layers[(int)InChILayers.ALL].GetMD5Hash();
					return db.Compounds
						.SearchScope(new CompoundsSearchScopeOptions() { RealOnly = true }, db)
						.Join(db.InChI_MD5s, c => c.StandardInChIId, md5 => md5.Id, (c, md5) => new { c.Id, md5.InChI_MD5 })
						.Where(i => i.InChI_MD5 == hash)
						.Select(c => c.Id)
						.Distinct()
						.ToList();
				}
			}

			return new List<Guid>();
		}

		/// <summary>
		/// Convert InChIKey to Compound ID
		/// </summary>
		/// <param name="inchikey">Standard on Non standard InChIKey</param>
		/// <returns>Internal compound ID</returns>
		public IEnumerable<Guid> InChIKeyToCompoundId(string inchikey)
		{
			using (var db = new CompoundsContext())
			{
				if (inchikey.StartsWith("InChIKey=", StringComparison.OrdinalIgnoreCase))
					inchikey = inchikey.Substring(9);

				if (inchikey.Length == 27 && InChIUtils.isValidInChIKey(inchikey))
				{
					return db.Compounds
						.SearchScope(new CompoundsSearchScopeOptions() { RealOnly = true }, db)
						.Join(db.InChIs, c => c.StandardInChIId, i => i.Id, (c, i) => new { c.Id, i.InChIKey })
						.Where(i => i.InChIKey == inchikey)
						.Select(c => c.Id)
						.Distinct()
						.ToList();
				}
			}

			return new List<Guid>();
		}

		/// <summary>
		/// Convert SMILES to Compound ID
		/// </summary>
		/// <param name="smiles">SMILES</param>
		/// <returns>Internal compound ID</returns>
		public IEnumerable<Guid> SMILESToCompoundId(string smiles)
		{
			MoleculeObjects.Molecule molecule = MoleculeObjects.MoleculeFactory.FromSMILES(smiles);

			var compoundId = MolToCompoundId(molecule.ct());
			if (compoundId != null)
				return new List<Guid>() { (Guid)compoundId };

			return new List<Guid>();
		}

		/// <summary>
		/// Convert MOL to Compound ID
		/// </summary>
		/// <param name="mol">MOL</param>
		/// <returns>Internal compound ID</returns>
		private Guid? MolToCompoundId(string mol)
		{
			var inchikey = InChIUtils.mol2InChIKey(mol, InChIFlags.CRS);

			var ids = InChIKeyToCompoundId(inchikey);

			if (ids.Any())
				return ids.First();

			return null;
		}

		/// <summary>
		/// Convert list of external references to compound Ids
		/// </summary>
		/// <param name="references">List of external references</param>
		/// <param name="uri">Exterenal reference Uri</param>
		/// <returns>Dictionary of external reference and compound Id</returns>
		public IDictionary<string, Guid> ExternalReferencesToCompoundIds(IEnumerable<string> references, string uri)
		{
			using (var db = new CompoundsContext())
			{
				return db.ExternalReferences
					.AsNoTracking()
					.Include(er => er.Type)
					.Where(er => er.Type.UriSpace == uri && references.Contains(er.Value))
					.ToDictionary(er => er.Value, er => er.CompoundId);
			}
		}
	}
}
