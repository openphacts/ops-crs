using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RSC.Compounds.Search
{
	public class SimilarityStructureSearch : RSC.Search.Search
	{
		private ISimilarityStructureSearch search;

		public SimilarityStructureSearch()
		{
			search = ServiceLocator.Current.GetService(typeof(ISimilarityStructureSearch)) as ISimilarityStructureSearch;

			if (search == null)
				throw new NullReferenceException("Similarity structure search is not configured properly");
		}

		public override IEnumerable<object> DoSearch()
		{
			var res = search.DoSearch(Options as SimilarityStructureSearchOptions, CommonOptions as CompoundsCommonSearchOptions, ScopeOptions as CompoundsSearchScopeOptions, ResultOptions as CompoundsSearchResultOptions);

			return res.Cast<object>();
		}
	}
}
