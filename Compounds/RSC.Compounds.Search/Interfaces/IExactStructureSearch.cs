using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.Search
{
	public interface IExactStructureSearch
	{
		/// <summary>
		/// Run exact structure search
		/// </summary>
		/// <param name="options">Exact stracture search options for search</param>
		/// <param name="commonOptions">Common options</param>
		/// <param name="scopeOptions">Scope options</param>
		/// <param name="resultsOptions">Results options</param>
		/// <returns>List of compounds' IDs that match the search request</returns>
		IEnumerable<SearchResult> DoSearch(ExactStructureSearchOptions options, CompoundsCommonSearchOptions commonOptions, CompoundsSearchScopeOptions scopeOptions, CompoundsSearchResultOptions resultsOptions);
	}
}
