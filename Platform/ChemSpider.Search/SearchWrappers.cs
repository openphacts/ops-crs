using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using ChemSpider.Data.Database;
using System.Data.SqlClient;
using System.Collections.Specialized;
using ChemSpider.Security;
using ChemSpider.Utilities;

namespace ChemSpider.Search
{
    public class CSAsyncSearch : CSSearch
    {
        CSSearch m_Search;

        public CSAsyncSearch(CSSearch search)
        {
            m_Search = search;
        }

        public CSSearch Search
        {
            get { return m_Search; }
        }

        public override void SetOptions(SearchOptions options, CommonSearchOptions common, SearchScopeOptions scopeOptions, SearchResultOptions resultOptions)
        {
            m_Search.SetOptions(options, common, scopeOptions, resultOptions);
        }

        public override string Description
        {
            get { return m_Search.Description; }
        }
        public override void Run(Sandbox sandbox, CSSearchResult result)
        {
            Action<Sandbox, CSSearchResult> run = new Action<Sandbox, CSSearchResult>(m_Search.Run);
            run.BeginInvoke(sandbox, result, null, null);
        }

    }
    /*
    public class CSBpfSearch : CSSearch
    {
        CSSearch m_Search;

        public string SearchJobName { get; set; }

        public CSBpfSearch(CSSearch search)
        {
            m_Search = search;
        }

        public override void SetOptions(SearchOptions options, CommonSearchOptions common, SearchScopeOptions scopeOptions, SearchResultOptions resultOptions)
        {
            m_Search.SetOptions(options, common, scopeOptions, resultOptions);
        }

        public override string Description
        {
            get { return m_Search.Description; }
        }

        public override void Run(Sandbox sandbox, CSSearchResult result)
        {
            string sOptions = XamlUtils.Object2Xaml(m_Search.Options);
            string sCommonOptions = XamlUtils.Object2Xaml(m_Search.CommonOptions);
            string sScopeOptions = XamlUtils.Object2Xaml(m_Search.ScopeOptions);
            string sResultOptions = XamlUtils.Object2Xaml(m_Search.ResultOptions);

            string sSandbox = XamlUtils.Object2Xaml(sandbox);
            string sSearchHandler = m_Search.GetType().FullName;

            string j = SearchJobName;
            if ( string.IsNullOrEmpty(j) ) {
                NameValueCollection searchSettings = (NameValueCollection)System.Configuration.ConfigurationManager.GetSection("Search/Settings");
                if ( searchSettings == null )
                    throw new Exception("Configuration section Searcrh/Settings is missing");

                j = searchSettings["BpfSearchJobName"];
                if ( string.IsNullOrEmpty(j) )
                    throw new Exception("Configuration parameter BpfSearchJobName is missing");
            }
            Dictionary<string, object> args = new Dictionary<string, object>();
            args["Options"] = sOptions;
            args["CommonOptions"] = sCommonOptions;
            args["ScopeOptions"] = sScopeOptions;
            args["ResultOptions"] = sResultOptions;

            args["Sandbox"] = sSandbox;
            args["SearchHandler"] = sSearchHandler;
            args["Rid"] = result.Rid;
            Guid handle = BpfServices.BeginExecuteJob(j, args);
            int r = BpfServices.EndExecuteJob(handle, -1);
            if ( r < 0 ) {
                throw new BpfException(string.Format("Background job execution failed with code {0}", r));
            }
        }
    }*/

    public class CSSearchFactory
    {
        public static bool Wrap { get { return m_WrapperMap != null; } }
        static NameValueCollection m_WrapperMap;

        static CSSearchFactory()
        {
            m_WrapperMap = (NameValueCollection)System.Configuration.ConfigurationManager.GetSection("Search/Wrappers");
        }

        public static CSSearch CreateWrappedInstance(Type type)
        {
            var s = Activator.CreateInstance(type) as CSSearch;
            return Wrap ? WrapInstance(s) : s;
        }

        public static CSSearch WrapInstance(CSSearch search)
        {
            if ( m_WrapperMap == null )
                return search;
            string wrapperTypeName = m_WrapperMap.Get(search.GetType().FullName);
            if ( !string.IsNullOrEmpty(wrapperTypeName) )
                search = Activator.CreateInstance(Type.GetType(wrapperTypeName), search) as CSSearch;

            return search;
        }

        public static CSSearch GetPredictedPropertiesSearch()
        {
            return CreateWrappedInstance(typeof(CSPredictedPropertiesSearch));
        }

        public static CSSearch GetElementsSearch()
        {
            return CreateWrappedInstance(typeof(CSElementsSearch));
        }

        public static CSSearch GetDataSourceSearch()
        {
            return CreateWrappedInstance(typeof(CSDataSourceSearch));
        }

        public static CSSearch GetIntrinsicPropertiesSearch()
        {
            return CreateWrappedInstance(typeof(CSIntrinsicPropertiesSearch));
        }

        public static CSSearch GetSimilarityStructureSearch()
        {
            return CreateWrappedInstance(typeof(CSSimilarityStructureSearch));
        }

        public static CSSearch GetSubstructureSearch()
        {
            return CreateWrappedInstance(typeof(CSSubstructureSearch));
        }

        public static CSSearch GetExactStructureSearch()
        {
            return CreateWrappedInstance(typeof(CSExactStructureSearch));
        }

        public static CSSearch GetSimpleSearch()
        {
            return CreateWrappedInstance(typeof(CSSimpleSearch));
        }

        public static CSSearch GetLassoSearch()
        {
            return CreateWrappedInstance(typeof(CSLassoSearch));
        }

        public static CSSearch GetKeywordSearch()
        {
            return CreateWrappedInstance(typeof(CSKeywordSearch));
        }

        public static CSSearch GetIdentifierSearch()
        {
            return CreateWrappedInstance(typeof(CSIdentifierSearch));
        }

        public static CSSearch GetNameSearch()
        {
            return CreateWrappedInstance(typeof(CSNameSearch));
        }

        public static CSSearch GetAdvancedSearch()
        {
            CSAdvancedSearch result = new CSAdvancedSearch();
            result.DataSourceSearch = new CSDataSourceSearch();
            result.ElementsSearch = new CSElementsSearch();
            result.IntrinsicPropertiesSearch = new CSIntrinsicPropertiesSearch();
            result.KeywordSearch = new CSKeywordSearch();
            result.LassoSearch = new CSLassoSearch();
            result.PredictedPropertiesSearch = new CSPredictedPropertiesSearch();
            result.StructureSearch = new CSExactStructureSearch();
            result.SuppInfoSearch = new CSSuppInfoSearch();
            result.CmpIdListSearch = new CSCmpIdListSearch();
            return WrapInstance(result);
        }

        public static CSSearch GetFlexibleSearch()
        {
            return CreateWrappedInstance(typeof(CSFlexibleSearch));
        }

        public static CSSearch GetSuppInfoSearch()
        {
            return CreateWrappedInstance(typeof(CSSuppInfoSearch));
        }
    }
}
