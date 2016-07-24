using Microsoft.Practices.ServiceLocation;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace RSC.CVSP.Search
{
	public class RecordsSearch : RSC.Search.Search
	{
		private CVSPSearch search;

		public RecordsSearch()
		{
			search = ServiceLocator.Current.GetService(typeof(CVSPSearch)) as CVSPSearch;

			if (search == null)
				throw new NullReferenceException("Records search is not configured properly");
		}

		public override IEnumerable<object> DoSearch()
		{
			var res = search.RecordsSearch(Options as CVSPRecordsSearchOptions, CommonOptions as CVSPCommonSearchOptions, ScopeOptions as CVSPSearchScopeOptions, ResultOptions as CVSPSearchResultOptions);

			return res.Cast<object>();
		}
	}
}
