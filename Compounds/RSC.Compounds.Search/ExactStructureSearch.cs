using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RSC.Compounds.Search
{
	public class ExactStructureSearch : RSC.Search.Search
	{
		private IExactStructureSearch search;

		public ExactStructureSearch()
		{
			search = ServiceLocator.Current.GetService(typeof(IExactStructureSearch)) as IExactStructureSearch;

			if (search == null)
				throw new NullReferenceException("Exact structure search is not configured properly");
		}

		public override IEnumerable<object> DoSearch()
		{
			var res = search.DoSearch(Options as ExactStructureSearchOptions, CommonOptions as CompoundsCommonSearchOptions, ScopeOptions as CompoundsSearchScopeOptions, ResultOptions as CompoundsSearchResultOptions);

			return res.Cast<object>();
		}
	}
}
