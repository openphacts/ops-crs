using InChINet;
using RSC.Compounds.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChemSpider.Utilities;

namespace RSC.Compounds.EntityFramework
{
	public class EFExactStructureSearch : IExactStructureSearch
	{
		/// <summary>
		/// Run exact structure search
		/// </summary>
		/// <param name="options">General compounds options for search</param>
		/// <param name="commonOptions">Common options</param>
		/// <param name="scopeOptions">Scope options</param>
		/// <param name="resultsOptions">Results options</param>
		/// <returns>List of compounds' IDs that match the search request</returns>
		public IEnumerable<SearchResult> DoSearch(ExactStructureSearchOptions options, CompoundsCommonSearchOptions commonOptions, CompoundsSearchScopeOptions scopeOptions, CompoundsSearchResultOptions resultsOptions)
		{
			string mol = options.Molecule;
			if (!mol.Contains('\n'))
			{
				MoleculeObjects.Molecule molecule = MoleculeObjects.MoleculeFactory.FromSMILES(mol);
				mol = molecule.ct();

				//please leave this comments below
				// using MolUtils.SMILESToMol(mol) will cause some smiles to convert to molfiles with relative stereo, e.g. "OC[C@@H](O­)[C@@H](O)­C=O"
				//mol = MolUtils.SMILESToMol(mol); 
			}

			string[] inchi_info = InChIUtils.mol2inchiinfo(mol, InChIFlags.Standard);
			if (inchi_info != null && !String.IsNullOrEmpty(inchi_info[0]))
			{
				string[] inchi_layers = InChIUtils.getInChILayers(inchi_info[0]);
				if (inchi_layers == null)
					throw new StructureSearchException("Unable to parse InChI: {0}\nOriginal MOL: {1}", inchi_info[0], mol);

				using (CompoundsContext db = new CompoundsContext())
				{
					IQueryable<Guid> query = null;
					byte[] hash = null;

					switch (options.MatchType)
					{
						case ExactStructureSearchOptions.EMatchType.ExactMatch:
							string[] crs_inchi = InChIUtils.mol2inchiinfo(mol, InChINet.InChIFlags.CRS);

							var key = crs_inchi[1];

							query = db.Compounds
									.SearchScope(scopeOptions, db)
									.Join(db.InChIs, c => c.NonStandardInChIId, i => i.Id, (c, i) => new { c.Id, i.InChIKey })
									.Where(i => i.InChIKey == key)
									.Select(c => c.Id)
									.Distinct();

							//tables.Add("compounds_nonstd_inchi ni");
							//predicates.Add("c.csid = ni.csid");
							//string[] crs_inchi = InChIUtils.mol2inchiinfo(mol, InChINet.InChIFlags.CRS);
							//predicates.Add(String.Format("ni.non_std_inchi_key = '{0}'", crs_inchi[1]));
							break;
						case ExactStructureSearchOptions.EMatchType.AllTautomers:
							hash = inchi_layers[(int)InChILayers.CHSI].GetMD5Hash();

							query = db.Compounds
									.SearchScope(scopeOptions, db)
									.Join(db.InChI_MD5s, c => c.StandardInChIId, md5 => md5.Id, (c, md5) => new { c.Id, md5.InChI_CHSI_MD5 })
									.Where(i => i.InChI_CHSI_MD5 == hash)
									.Select(c => c.Id)
									.Distinct();

							//visual.Add("All Tautomers");
							//tables.Add("inchis_md5 imd5");
							//predicates.Add("c.inc_id = imd5.inc_id");
							//predicates.Add("imd5.inchi_chsi_md5 = HashBytes('md5', cast('" + inchi_layers[(int)InChILayers.CHSI] + "' as nvarchar(max)))");
							break;
						case ExactStructureSearchOptions.EMatchType.SameSkeletonIncludingH:
							hash = inchi_layers[(int)InChILayers.CHSI].GetMD5Hash();

							query = db.Compounds
									.SearchScope(scopeOptions, db)
									.Join(db.InChI_MD5s, c => c.StandardInChIId, md5 => md5.Id, (c, md5) => new { c.Id, md5.InChI_CH_MD5 })
									.Where(i => i.InChI_CH_MD5 == hash)
									.Select(c => c.Id)
									.Distinct();

							//visual.Add("Same Skeleton (Including H)");
							//tables.Add("inchis_md5 imd5");
							//predicates.Add("c.inc_id = imd5.inc_id");
							//predicates.Add("imd5.inchi_ch_md5 = HashBytes('md5', cast('" + inchi_layers[(int)InChILayers.CH] + "' as nvarchar(max)))");
							break;
						case ExactStructureSearchOptions.EMatchType.SameSkeletonExcludingH:
							hash = inchi_layers[(int)InChILayers.C].GetMD5Hash();

							query = db.Compounds
									.SearchScope(scopeOptions, db)
									.Join(db.InChI_MD5s, c => c.StandardInChIId, md5 => md5.Id, (c, md5) => new { c.Id, md5.InChI_C_MD5 })
									.Where(i => i.InChI_C_MD5 == hash)
									.Select(c => c.Id)
									.Distinct();

							//visual.Add("Same Skeleton (Excluding H)");
							//tables.Add("inchis_md5 imd5");
							//predicates.Add("c.inc_id = imd5.inc_id");
							//predicates.Add("imd5.inchi_c_md5 = HashBytes('md5', cast('" + inchi_layers[(int)InChILayers.C] + "' as nvarchar(max)))");
							break;
						case ExactStructureSearchOptions.EMatchType.AllIsomers:
							hash = inchi_layers[(int)InChILayers.MF].GetMD5Hash();

							query = db.Compounds
									.SearchScope(scopeOptions, db)
									.Join(db.InChI_MD5s, c => c.StandardInChIId, md5 => md5.Id, (c, md5) => new { c.Id, md5.InChI_MF_MD5 })
									.Where(i => i.InChI_MF_MD5 == hash)
									.Select(c => c.Id)
									.Distinct();

							//visual.Add("All Isomers");
							//tables.Add("inchis_md5 imd5");
							//predicates.Add("c.inc_id = imd5.inc_id");
							//predicates.Add("imd5.inchi_mf_md5 = HashBytes('md5', cast('" + inchi_layers[(int)InChILayers.MF] + "' as nvarchar(max)))");
							break;
					}

					return query.ToSearchResults();
				}
			}

			return new List<SearchResult>();
		}
	}
}
