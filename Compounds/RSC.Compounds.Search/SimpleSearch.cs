using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RSC.Compounds.Search
{
	public class SimpleSearch : RSC.Search.Search
	{
		private ISimpleSearch search;

		public SimpleSearch()
		{
			search = ServiceLocator.Current.GetService(typeof(ISimpleSearch)) as ISimpleSearch;

			if (search == null)
				throw new NullReferenceException("Simple search is not configured properly");
		}

		public override IEnumerable<object> DoSearch()
		{
			var res = search.DoSearch(Options as SimpleSearchOptions, CommonOptions as CompoundsCommonSearchOptions, ScopeOptions as CompoundsSearchScopeOptions, ResultOptions as CompoundsSearchResultOptions);

			return res.Cast<object>();
		}
	}
}
