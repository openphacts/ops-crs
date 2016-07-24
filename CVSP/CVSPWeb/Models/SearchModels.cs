using RSC.CVSP.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CVSPWeb.Models
{
	public class SearchModel
	{
		public CVSPCommonSearchOptions commonOptions { get; set; }
		public CVSPSearchScopeOptions scopeOptions { get; set; }
		public CVSPSearchResultOptions resultOptions { get; set; }
	}

	public class CVSPRecordsSearchModel : SearchModel
	{
		public CVSPRecordsSearchOptions searchOptions { get; set; }
	}

	public class CVSPJobsSearchModel : SearchModel
	{
		public CVSPJobsSearchOptions searchOptions { get; set; }
	}
}