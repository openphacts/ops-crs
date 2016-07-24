using RSC.Compounds.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.EntityFramework
{
	public static class SearchResultExtensions
	{
		public static IEnumerable<SearchResult> ToSearchResults(this IQueryable<Guid> guids)
		{
			return guids.Select(g => new SearchResult() { Id = g }).ToList();
		}
	}
}
