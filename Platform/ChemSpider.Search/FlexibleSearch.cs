using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace ChemSpider.Search
{
    public class CSFlexibleSearch : CSSqlSearch
    {
        List<CSSearch> m_Searches = new List<CSSearch>();

        public new FlexibleSearchOptions Options
        {
            get { return base.Options as FlexibleSearchOptions; }
        }

        internal override ResultList GetResults(SqlCommand cmd)
        {
            if ( m_Searches[0] is CSSqlSearch )
                return (m_Searches[0] as CSSqlSearch).GetResults(cmd);
            return base.GetResults(cmd);
        }

        public override void SetOptions(SearchOptions options, CommonSearchOptions common, SearchScopeOptions scopeOptions, SearchResultOptions resultOptions)
        {
            FlexibleSearchOptions o = options as FlexibleSearchOptions;

            foreach (SearchOptionsPair p in o.InnerSearches)
            {
                p.Search.SetOptions(p.Options, common, scopeOptions, resultOptions);
                m_Searches.Add(p.Search);
            }

            base.SetOptions(options, common, scopeOptions, resultOptions);
        }

        protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
        {
            bool bAdded = false;
            foreach ( CSSearch s in m_Searches ) {
                if ( s is CSSqlSearch )
                    bAdded = (s as CSSqlSearch).GetSqlAndSubstitute(predicates, tables, orderby, visual, columns);
            }
            return bAdded;
        }
    }
}
