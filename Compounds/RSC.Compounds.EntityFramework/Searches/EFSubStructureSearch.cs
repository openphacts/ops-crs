using RSC.Compounds.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.EntityFramework
{
	public class EFSubStructureSearch : ISubStructureSearch
	{
		/// <summary>
		/// Run sub-structure search
		/// </summary>
		/// <param name="options">Sub-structure search options for search</param>
		/// <param name="commonOptions">Common options</param>
		/// <param name="scopeOptions">Scope options</param>
		/// <param name="resultsOptions">Results options</param>
		/// <returns>List of compounds' IDs that match the search request</returns>
		public IEnumerable<SearchResult> DoSearch(SubStructureSearchOptions options, CompoundsCommonSearchOptions commonOptions, CompoundsSearchScopeOptions scopeOptions, CompoundsSearchResultOptions resultsOptions)
		{
			using (CompoundsContext db = new CompoundsContext())
			{
				string parameters = resultsOptions != null && resultsOptions.Limit > 0 ? string.Format("TOP {0};", resultsOptions.Limit) : "";

				if (options.MolType == SubStructureSearchOptions.EMolType.SMILES)
				{
					var res = db.Database.SqlQuery<StructureSearchResult>("exec SearchByMolSub {0}, {1}", options.Molecule, parameters).ToList();

					return res;
				}
				else if (options.MolType == SubStructureSearchOptions.EMolType.SMARTS)
				{
					var res = db.Database.SqlQuery<StructureSearchResult>("exec SearchBySMARTSSub {0}, {1}", options.Molecule, parameters).ToList();

					return res;
				}
			}

			return new List<StructureSearchResult>();
		}
	}
}
