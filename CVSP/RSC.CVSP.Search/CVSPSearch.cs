using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Search
{
	public abstract class CVSPSearch
	{
		/// <summary>
		/// Run records' search
		/// </summary>
		/// <param name="options">General CVSP options for search</param>
		/// <param name="commonOptions">Common CVSP options</param>
		/// <param name="scopeOptions">Scope CVSP options</param>
		/// <param name="resultsOptions">Results CVSP options</param>
		/// <returns>List of records' GUIDs that match the search request</returns>
		public abstract IEnumerable<Guid> RecordsSearch(CVSPRecordsSearchOptions options, CVSPCommonSearchOptions commonOptions, CVSPSearchScopeOptions scopeOptions, CVSPSearchResultOptions resultsOptions);

		/// <summary>
		/// Run depositions' search
		/// </summary>
		/// <param name="options">General CVSP options for search</param>
		/// <param name="commonOptions">Common CVSP options</param>
		/// <param name="scopeOptions">Scope CVSP options</param>
		/// <param name="resultsOptions">Results CVSP options</param>
		/// <returns>List of depositions' GUIDs that match the search request</returns>
		public abstract IEnumerable<Guid> DepositionsSearch(CVSPDepositionsSearchOptions options, CVSPCommonSearchOptions commonOptions, CVSPSearchScopeOptions scopeOptions, CVSPSearchResultOptions resultsOptions);
	}
}
