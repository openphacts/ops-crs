using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RSC.Compounds.Search
{
	public class SubStructureSearch : RSC.Search.Search
	{
		private ISubStructureSearch search;

		public SubStructureSearch()
		{
			search = ServiceLocator.Current.GetService(typeof(ISubStructureSearch)) as ISubStructureSearch;

			if (search == null)
				throw new NullReferenceException("Sub structure search is not configured properly");
		}

		public override IEnumerable<object> DoSearch()
		{
			var res = search.DoSearch(Options as SubStructureSearchOptions, CommonOptions as CompoundsCommonSearchOptions, ScopeOptions as CompoundsSearchScopeOptions, ResultOptions as CompoundsSearchResultOptions);

			return res.Cast<object>();
		}
	}
}
