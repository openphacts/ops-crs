using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Search.Core
{
	public interface ISqlSearchProvider
	{
		string GetConnectionString();
		void GetCommonSqlParts(CommonSearchOptions options, List<string> predicates, List<string> tables, List<string> orderby, List<string> visual);
		void GetScopeSqlParts(SearchScopeOptions options, List<string> predicates, List<string> tables, List<string> orderby, List<string> visual);
		string GetResultSqlCommand(SearchResultOptions options, List<string> predicates, List<string> tables, List<string> columns, List<string> orderby);
	}
}
