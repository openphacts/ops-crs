using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.Search
{
	public interface ISubStructureSearch
	{
		/// <summary>
		/// Run sub-structure search
		/// </summary>
		/// <param name="options">Sub-structure search options for search</param>
		/// <param name="commonOptions">Common options</param>
		/// <param name="scopeOptions">Scope options</param>
		/// <param name="resultsOptions">Results options</param>
		/// <returns>List of compounds' IDs that match the search request</returns>
		IEnumerable<SearchResult> DoSearch(SubStructureSearchOptions options, CompoundsCommonSearchOptions commonOptions, CompoundsSearchScopeOptions scopeOptions, CompoundsSearchResultOptions resultsOptions);
	}
}
