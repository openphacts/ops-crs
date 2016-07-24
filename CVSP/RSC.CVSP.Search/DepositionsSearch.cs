using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Search
{
	public class DepositionsSearch : RSC.Search.Search
	{
		private CVSPSearch search;

		public DepositionsSearch()
		{
			search = ServiceLocator.Current.GetService(typeof(CVSPSearch)) as CVSPSearch;

			if (search == null)
				throw new NullReferenceException("Depositions search is not configured properly");
		}

		public override IEnumerable<object> DoSearch()
		{
			var res = search.DepositionsSearch(Options as CVSPDepositionsSearchOptions, CommonOptions as CVSPCommonSearchOptions, ScopeOptions as CVSPSearchScopeOptions, ResultOptions as CVSPSearchResultOptions);

			return res.Cast<object>();
		}
	}
}
