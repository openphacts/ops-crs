using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RSC.Compounds.Search;
using InChINet;
using ChemSpider.Molecules;
using ChemSpider.Utilities;

namespace RSC.Compounds.EntityFramework
{
	public class EFSimpleSearch : ISimpleSearch
	{
		/// <summary>
		/// Run simple compounds' search
		/// </summary>
		/// <param name="options">General compounds options for search</param>
		/// <param name="commonOptions">Common options</param>
		/// <param name="scopeOptions">Scope options</param>
		/// <param name="resultsOptions">Results options</param>
		/// <returns>List of compounds' IDs that match the search request</returns>
		public IEnumerable<SearchResult> DoSearch(SimpleSearchOptions options, CompoundsCommonSearchOptions commonOptions, CompoundsSearchScopeOptions scopeOptions, CompoundsSearchResultOptions resultsOptions)
		{
			using (CompoundsContext db = new CompoundsContext())
			{
				//	find by approved synonym...
				var query = db.Compounds
							.SearchScope(scopeOptions, db)
							.Join(db.CompoundSynonyms, c => c.Id, cs => cs.CompoundId, (c, cs) => new { cs })
							.Join(db.Synonyms, css => css.cs.SynonymId, s => s.Id, (css, s) => new { css.cs, s })
							.Where(cs => cs.cs.SynonymState == CompoundSynonymState.eApproved && cs.s.Synonym == options.QueryText)
							.Select(cs => cs.cs.CompoundId)
							.Distinct();

				if (query.Any())
					return query.ToSearchResults();

				//	find by any synonym...
				query = db.Compounds
						.SearchScope(scopeOptions, db)
						.Join(db.CompoundSynonyms, c => c.Id, cs => cs.CompoundId, (c, cs) => new { cs })
						.Join(db.Synonyms, css => css.cs.SynonymId, s => s.Id, (css, s) => new { css.cs, s })
						.Where(cs => cs.s.Synonym == options.QueryText)
						.Select(cs => cs.cs.CompoundId)
						.Distinct();

				if (query.Any())
					return query.ToSearchResults();

				// InChIKey
				string inchi_key = options.QueryText;
				if (options.QueryText.StartsWith("InChIKey=", StringComparison.OrdinalIgnoreCase))
					inchi_key = options.QueryText.Substring(9);

				// Maybe InChIKey
				if (inchi_key.Length == 27 && InChIUtils.isValidInChIKey(inchi_key))
				{
					query = db.Compounds
							.SearchScope(scopeOptions, db)
							.Join(db.InChIs, c => c.StandardInChIId, i => i.Id, (c, i) => new { c.Id, i.InChIKey })
							.Where(i => i.InChIKey == inchi_key)
							.Select(c => c.Id)
							.Distinct();

					if (query.Any())
						return query.ToSearchResults();
				}

				// Part of InChIKey
				if (inchi_key.Length == 14)
				{
					query = db.Compounds
							.SearchScope(scopeOptions, db)
							.Join(db.InChI_MD5s, c => c.StandardInChIId, md5 => md5.Id, (c, md5) => new { c.Id, md5.InChIKey_A })
							.Where(i => i.InChIKey_A == inchi_key)
							.Select(c => c.Id)
							.Distinct();

					if (query.Any())
						return query.ToSearchResults();
				}

				// Try to get InChI to search by
				string[] inchi_layers = null;
				if (options.QueryText.StartsWith("InChI="))
					inchi_layers = InChIUtils.getInChILayers(options.QueryText);
				else if (InChIUtils.isValidInChI(options.QueryText))
				{
					string message;
					string inchi = ChemIdUtils.anyId2InChI(options.QueryText, out message, InChIFlags.v104);
					if (!String.IsNullOrEmpty(inchi))
						inchi_layers = InChIUtils.getInChILayers(inchi);
				}

				// We have InChI - let's use it
				if (inchi_layers != null)
				{
					// Standard InChI
					byte[] hash = inchi_layers[(int)InChILayers.ALL].GetMD5Hash();
					query = db.Compounds
							.SearchScope(scopeOptions, db)
							.Join(db.InChI_MD5s, c => c.StandardInChIId, md5 => md5.Id, (c, md5) => new { c.Id, md5.InChI_MD5 })
							.Where(i => i.InChI_MD5 == hash)
							.Select(c => c.Id)
							.Distinct();

					if (query.Any())
						return query.ToSearchResults();

					hash = inchi_layers[(int)InChILayers.CHSI].GetMD5Hash();
					query = db.Compounds
							.SearchScope(scopeOptions, db)
							.Join(db.InChI_MD5s, c => c.StandardInChIId, md5 => md5.Id, (c, md5) => new { c.Id, md5.InChI_CHSI_MD5 })
							.Where(i => i.InChI_CHSI_MD5 == hash)
							.Select(c => c.Id)
							.Distinct();

					if (query.Any())
						return query.ToSearchResults();

					hash = inchi_layers[(int)InChILayers.CH].GetMD5Hash();
					query = db.Compounds
							.SearchScope(scopeOptions, db)
							.Join(db.InChI_MD5s, c => c.StandardInChIId, md5 => md5.Id, (c, md5) => new { c.Id, md5.InChI_CH_MD5 })
							.Where(i => i.InChI_CH_MD5 == hash)
							.Select(c => c.Id)
							.Distinct();

					if (query.Any())
						return query.ToSearchResults();

					hash = inchi_layers[(int)InChILayers.C].GetMD5Hash();
					query = db.Compounds
							.SearchScope(scopeOptions, db)
							.Join(db.InChI_MD5s, c => c.StandardInChIId, md5 => md5.Id, (c, md5) => new { c.Id, md5.InChI_C_MD5 })
							.Where(i => i.InChI_C_MD5 == hash)
							.Select(c => c.Id)
							.Distinct();

					if (query.Any())
						return query.ToSearchResults();
				}
			}

			return new List<SearchResult>();
		}
	}
}
