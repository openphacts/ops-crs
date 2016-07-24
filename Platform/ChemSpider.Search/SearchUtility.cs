using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Configuration;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using ChemSpider.Utilities;
using ChemSpider.Search;
using ChemSpider;
using ChemSpider.Security;
using ChemSpider.Molecules;
using InChINet;

// TODO: many utilities in this file are redundant and have to be replaced with CSSearch derivatives

namespace ChemSpider.Search
{
    public class AsyncSearchParam<T>
    {
        public T ctl;
        public Request req;
        private ManualResetEvent evt;
        public AsyncSearchParam(T ctl, Request req)
        {
            this.ctl = ctl;
            this.req = req;
            evt = new ManualResetEvent(false);
        }

        public T Control
        {
            get { return ctl; }
        }

        public Request Request
        {
            get { return req; }
        }

        public void Wait(TimeSpan ts)
        {
            evt.WaitOne(ts, false);
        }

        public void Set()
        {
            evt.Set();
        }
    }

    /// <summary>
    /// Summary description for SearchUtility
    /// </summary>
    public class SearchUtility
    {
        private static ChemSpiderDB s_csdb
        {
            get
            {
                return new ChemSpiderDB();
            }
        }

        static public List<int> SimpleSearch(string query, out string message, bool bAllowVagueSearch = true)
        {
            SearchParameters sp = new SearchParameters { allowVagueSearch = bAllowVagueSearch };
            var req = new CSRequestSearchResult();
            sp.req = req;
            if ( query.StartsWith("\"") && query.EndsWith("\"") )
                query = query.Substring(1, query.Length - 2);
            sp.query = query;

            SessionInfo si = new SessionInfo();

            if ( HttpContext.Current != null ) {
                HttpRequest httpReq = HttpContext.Current.Request;
                si.cookie = httpReq.Cookies["CSSESSID"] != null ? httpReq.Cookies["CSSESSID"].Value : null;
                si.address = httpReq.UserHostName;
                si.referrer = httpReq.UrlReferrer != null ? httpReq.UrlReferrer.AbsoluteUri : null;
                sp.allowVagueSearch = false;
            }

            SimpleSearch(sp, si);

            message = sp.req.Message;

            if ( sp.req.Found == null )
                return null;
            else
                return ( sp.req.Found as ResultList ).ToList();
        }

        static public void SimpleSearch(SearchParameters sp, SessionInfo si)
        {
            using ( SqlConnection conn = new SqlConnection(ChemSpiderDB.RO_ConnectionString) ) {
                conn.Open();

                if ( sp.query == String.Empty ) {
                    sp.req.Message = "Query is not specified";
                    sp.req.Status = ERequestStatus.Failed;
                    return;
                }

                sp.req.Description = sp.query;
                sp.req.Timeout = sp.timeout;

                if ( sp.req.Rid == null )
                    registerTransaction(si, sp.req.Request);

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.ConnectionTimeout;

                try {
                    CSSimpleSearch search = new CSSimpleSearch();
                    search.SetOptions(new SimpleSearchOptions() { QueryText = sp.query, AllowVagueSearch = sp.allowVagueSearch }, null, null, new SearchResultOptions());
                    search.Run(sp.sandbox, sp.req);
                    sp.req.Status = ERequestStatus.ResultReady;
                    sp.req.Progress = 1;
                }
                catch ( Exception ex ) {
                    sp.req.Status = ERequestStatus.Failed;
                    sp.req.Message = ex.Message;
                }

                sp.req.Request.saveToTransaction();
            }
        }

        private static void registerTransaction(SessionInfo sp, Request req)
        {
            req.registerTransaction(sp.cookie, sp.address, sp.referrer, "S");
        }

        public class SearchParameters
        {
            public CSRequestSearchResult req;
            public string query;
            public Sandbox sandbox;
            public TimeSpan timeout;
            public bool allowVagueSearch = true;
            public int limit;
        }

        public class SessionInfo
        {
            public string cookie, address, referrer;
        }

        static public CSSearchResult RunAsyncSearch(CSSearch search, SearchOptions options, CommonSearchOptions commonOptions, SearchScopeOptions scopeOptions, SearchResultOptions resultOptions)
        {
            return RunSearch(new CSAsyncSearch(search), options, commonOptions, scopeOptions, resultOptions);
        }

        static public CSSearchResult RunSearch(CSSearch search, SearchOptions options, CommonSearchOptions commonOptions, SearchScopeOptions scopeOptions, SearchResultOptions resultOptions)
        {
            CSRequestSearchResult req = new CSRequestSearchResult(new Request());

            search.SetOptions(options, commonOptions, scopeOptions, resultOptions ?? new SearchResultOptions());
            req.Description = search.Description;
            req.Request.registerTransaction("W");

            try {
                search.Run(null, req);
            }
            catch ( Exception ex ) {
                req.Status = ERequestStatus.Failed;
                req.Message = ex.Message;
                req.Update();
            }

            return req;
        }

        public static void asynchSqlSearch(Request req, HttpRequest httpReq)
        {
            req.registerTransaction(
                httpReq.Cookies["CSSESSID"] != null ? httpReq.Cookies["CSSESSID"].Value : null,
                httpReq.UserHostName,
                httpReq.UrlReferrer != null ? httpReq.UrlReferrer.AbsoluteUri : null, "A");

            try {
                AsyncSearchParam<object> ac = new AsyncSearchParam<object>(null, req);

                if ( ThreadPool.QueueUserWorkItem(new WaitCallback(async_sql_search), ac) ) {
                    req.Status = ERequestStatus.Processing;
                }
                else {
                    req.Status = ERequestStatus.Failed;
                    req.Message = "No query specified";
                }

                if ( req.Status == ERequestStatus.Processing )
                    ac.Wait(TimeSpan.FromSeconds(20.0));
            }
            catch ( Exception ex ) {
                req.Status = ERequestStatus.Failed;
                req.Message = ex.Message;
            }
            finally {
                req.saveToTransaction();
            }
        }

        private static void async_sql_search(Object o)
        {
            Request req = ( o as AsyncSearchParam<object> ).Request;
            try {
                using ( SqlConnection conn = new SqlConnection(ChemSpiderDB.RO_ConnectionString) ) {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(req.Sql, conn);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = req.Timeout.Seconds;
                    req.Found = new ResultList(EResultObjectType.Compound, DbUtil.fetchColumn(cmd));
                    req.Status = ERequestStatus.ResultReady;
                    req.Progress = 1;
                }
            }
            catch ( Exception ex ) {
                req.Status = ERequestStatus.Failed;
                req.Message = ex.Message;
            }
            finally {
                req.saveToTransaction();
                ( o as AsyncSearchParam<object> ).Set();
            }
        }

        public enum ExactSearchOptions
        {
            eExactMatch,
            eAllTautomers,
            eSameSkeletonAndH,
            eSameSkeleton,
            eAllIsomers
        }

        public static List<int> SearchExact(string mol_or_smiles, ExactSearchOptions options)
        {
            string mol = mol_or_smiles;
            if ( !mol.Contains("\n") )
                mol = MolUtils.SMILESToMol(mol);
            string inchi = InChIUtils.mol2InChI(mol, InChIFlags.Default);
            if ( String.IsNullOrEmpty(inchi) )
                throw new ChemSpiderException("Unable to generate InChI: {0}", mol_or_smiles);

            return SearchExactInChI(inchi, options);
        }

        public static List<int> SearchExactInDataSource(string mol, ExactSearchOptions options, string[] datasources)
        {
            string inchi = InChIUtils.mol2InChI(mol, InChIFlags.Default);
            if ( String.IsNullOrEmpty(inchi) )
                throw new ChemSpiderException("Unable to generate InChI. Original MOL: {0}", mol);

            return SearchExactInChIInDataSource(inchi, options, datasources);
        }

        public static List<int> SearchExactSMILES(string smiles, ExactSearchOptions options)
        {
            string mol = MolUtils.SMILESToMol(smiles);
            string inchi = InChIUtils.mol2InChI(mol, InChIFlags.Default);
            return SearchExactInChI(inchi, options);
        }

        public static List<int> SearchExactInChI(string inchi, ExactSearchOptions options)
        {
            string[] inchi_layers = InChIUtils.getInChILayers(inchi);
            if ( inchi_layers == null )
                throw new ChemSpiderException("Unable to parse InChI: {0}\n", inchi);

            string sql = null;
            switch ( options ) {
                case ExactSearchOptions.eExactMatch:
                    sql = String.Format("select cmp_id from inchis_md5 where inchi_md5 = HashBytes('md5', '{0}')", DbUtil.prepare4sql(inchi_layers[0]));
                    break;
                case ExactSearchOptions.eAllTautomers:
                    sql = String.Format("select cmp_id from inchis_md5 where inchi_chsi_md5 = HashBytes('md5', '{0}')", DbUtil.prepare4sql(inchi_layers[4]));
                    break;
                case ExactSearchOptions.eSameSkeletonAndH:
                    sql = String.Format("select cmp_id from inchis_md5 where inchi_ch_md5 = HashBytes('md5', '{0}')", DbUtil.prepare4sql(inchi_layers[1]));
                    break;
                case ExactSearchOptions.eSameSkeleton:
                    sql = String.Format("select cmp_id from inchis_md5 where inchi_c_md5 = HashBytes('md5', '{0}')", DbUtil.prepare4sql(inchi_layers[2]));
                    break;
                case ExactSearchOptions.eAllIsomers:
                    sql = String.Format("select cmp_id from inchis where inchi_mf = '{0}'", DbUtil.prepare4sql(inchi_layers[3]));
                    break;
            }

            return s_csdb.DBU.m_fetchColumn<int>(sql);
        }

        public static List<int> SearchExactInChIInDataSource(string inchi, ExactSearchOptions options, string[] datasources)
        {
            string[] inchi_layers = InChIUtils.getInChILayers(inchi);
            if ( inchi_layers == null )
                throw new ChemSpiderException("Unable to parse InChI: {0}\n", inchi);

            string sql = null;
            switch ( options ) {
                case ExactSearchOptions.eExactMatch:
                    sql = String.Format("select i5.cmp_id from inchis_md5 i5 join substances s on i5.cmp_id = s.cmp_id where i5.inchi_md5 = HashBytes('md5', '{0}')", DbUtil.prepare4sql(inchi_layers[0]));
                    break;
                case ExactSearchOptions.eAllTautomers:
                    sql = String.Format("select i5.cmp_id from inchis_md5 i5 join substances s on i5.cmp_id = s.cmp_id where i5.inchi_chsi_md5 = HashBytes('md5', '{0}')", DbUtil.prepare4sql(inchi_layers[4]));
                    break;
                case ExactSearchOptions.eSameSkeletonAndH:
                    sql = String.Format("select i5.cmp_id from inchis_md5 i5 join substances s on i5.cmp_id = s.cmp_id where i5.inchi_ch_md5 = HashBytes('md5', '{0}')", DbUtil.prepare4sql(inchi_layers[1]));
                    break;
                case ExactSearchOptions.eSameSkeleton:
                    sql = String.Format("select i5.cmp_id from inchis_md5 i5 join substances s on i5.cmp_id = s.cmp_id where i5.inchi_c_md5 = HashBytes('md5', '{0}')", DbUtil.prepare4sql(inchi_layers[2]));
                    break;
                case ExactSearchOptions.eAllIsomers:
                    sql = String.Format("select i.cmp_id from inchis i join substances s on i.cmp_id = s.cmp_id where i.inchi_mf = '{0}'", DbUtil.prepare4sql(inchi_layers[3]));
                    break;
            }

            if ( datasources != null && datasources.Length > 0 ) {
                StringBuilder sb = new StringBuilder();
                foreach ( string ds in datasources )
                    sb.AppendFormat(", '{0}'", ds);

                sql = String.Format("{0} and s.dsn_id in (select dsn_id from v_data_sources where name in ({1}))", sql, sb.ToString().Substring(2));
            }

            return s_csdb.DBU.m_fetchColumn<int>(sql);
        }

        public struct SynonymInfo
        {
            public string synonym;
            public bool approved;
            public string language;
        }

        /// <summary>
        /// Search by InChI, SMILES or MOL
        /// </summary>
        /// <param name="mol">InChI, SMILES or MOL</param>
        /// <param name="bUseSkeletonApproximation">Return exact match or skeleton match would be enough</param>
        /// <returns></returns>
        public static List<SynonymInfo> getSynonymsInfo(string mol, bool bUseSkeletonApproximation)
        {
            List<int> csids = null;
            if ( !mol.Contains("\n") && mol.StartsWith("InChI=", StringComparison.OrdinalIgnoreCase) ) {
                csids = SearchUtility.SearchExactInChI(mol, SearchUtility.ExactSearchOptions.eExactMatch);
                if ( csids.Count == 0 )
                    csids = SearchUtility.SearchExactInChI(mol, SearchUtility.ExactSearchOptions.eAllTautomers);
                if ( csids.Count == 0 && bUseSkeletonApproximation )
                    csids = SearchUtility.SearchExactInChI(mol, SearchUtility.ExactSearchOptions.eSameSkeletonAndH);
            }
            else {
                if ( !mol.Contains("\n") )
                    mol = MolUtils.SMILESToMol(mol);

                if ( !String.IsNullOrEmpty(mol) ) {
                    csids = SearchUtility.SearchExact(mol, SearchUtility.ExactSearchOptions.eExactMatch);
                    if ( csids.Count == 0 )
                        csids = SearchUtility.SearchExact(mol, SearchUtility.ExactSearchOptions.eAllTautomers);
                    if ( csids.Count == 0 && bUseSkeletonApproximation )
                        csids = SearchUtility.SearchExact(mol, SearchUtility.ExactSearchOptions.eSameSkeletonAndH);
                }
            }

            List<SynonymInfo> result = new List<SynonymInfo>();
            if ( csids != null && csids.Count != 0 )
                addSynonymsForCSIDs(result, csids);

            return result;
        }

        public static List<string> getSynonyms(string mol, bool bUseSkeletonApproximation)
        {
            List<SynonymInfo> infos = getSynonymsInfo(mol, bUseSkeletonApproximation);
            List<string> synonyms = new List<string>();
            foreach ( SynonymInfo info in infos )
                synonyms.Add(info.synonym);
            return synonyms;
        }

        /// <summary>
        /// Appends approved synonyms for each csid in the List&lt;int&gt; csids to the end of result.
        /// </summary>
        private static void addSynonymsForCSIDs(List<SynonymInfo> result, List<int> csids)
        {
            StringBuilder sb = new StringBuilder();
            foreach ( int csid in csids )
                sb.AppendFormat(",{0}", csid);
            List<SynonymInfo> synonyms_info = new List<SynonymInfo>();
            DataTable synonyms = s_csdb.DBU.m_fillDataTable(String.Format(@"
			select s.synonym, cs.opinion, cs.approved_yn, cast(s.lang_id1 as varchar(10)) as language
			from compounds_synonyms cs join synonyms s on cs.syn_id = s.syn_id
			where cs.cmp_id in ({0}) and cs.deleted_yn = 0 and s.deleted_yn = 0
			order by dbo.fSynonymMatchScore(cs.cmp_id, cs.syn_id) desc", sb.ToString().Substring(1)));  // and cs.opinion = 'Y' and cs.approved_yn = 1 as per Christine's request
            foreach ( DataRow row in synonyms.Rows ) {
                synonyms_info.Add(
                    new SynonymInfo {
                        synonym = row["synonym"] as string,
                        approved = bool.Equals(row["approved_yn"], true) && String.Equals(row["opinion"], "Y"),
                        language = row["language"] as string,
                    });
            }
            result.AddRange(synonyms_info);
        }

        // From inchis.org

        public static List<int> Search(string query, out string message)
        {
            message = null;

            if ( query == null )
                return null;

            query = query.Trim();

            if ( String.IsNullOrEmpty(query) )
                return null;

            if ( query.StartsWith("info:inchi/", StringComparison.OrdinalIgnoreCase) )
                query = query.Substring(11);

            if ( query.StartsWith("InChIKey=", StringComparison.OrdinalIgnoreCase) )
                query = query.Substring(9);

            // This may be InChIKey
            if ( query.Length >= 14 && query.Length <= 27 ) {
                List<int> result = search_inchi_key(query, out message);
                if ( result != null )
                    return result;
            }

            // This is InChI, so return whatever we find
            if ( query.StartsWith("InChI=", StringComparison.OrdinalIgnoreCase) )
                return search_inchi(query, out message);

            // Try to convert query to InChI
            string mol = MolUtils.SMILESToMol(query);
            if ( !String.IsNullOrEmpty(mol) ) {
                string inchi = InChIUtils.mol2InChI(mol, InChIFlags.Standard);
                return search_inchi(inchi, out message);
            }

            return SimpleSearch(query, out message);
        }

        private static List<int> search_inchi_key(string query, out string message)
        {
            message = null;
            List<int> result = null;

            if ( InChIUtils.isValidInChIKey(query) ) {
                string sql = String.Format("select distinct c.cmp_id from compounds c join inchis_102b_std i on c.cmp_id = i.cmp_id where i.inchi_key = '{0}'", DbUtil.prepare4sql(query));
                result = s_csdb.DBU.m_fetchColumn<int>(sql);
                if ( result.Count != 0 ) {
                    message = "Found by InChIKey";
                    return result;
                }

                sql = String.Format("select distinct c.cmp_id from compounds c join inchis_md5 i on c.cmp_id = i.cmp_id where i.inchi_key = '{0}'", DbUtil.prepare4sql(query));
                result = s_csdb.DBU.m_fetchColumn<int>(sql);
                if ( result.Count != 0 ) {
                    message = "Found by InChIKey";
                    return result;
                }
            }

            // Maybe std inchi
            if ( query.Length == 27 ) {
                string sql = String.Format("select distinct c.cmp_id from compounds c join inchis_std i on c.cmp_id = i.cmp_id where i.inchi_key = '{0}'", DbUtil.prepare4sql(query));
                result = s_csdb.DBU.m_fetchColumn<int>(sql);
                if ( result.Count != 0 ) {
                    message = "Found by Standard InChIKey";
                    return result;
                }
            }

            // Part of InChIKey
            if ( query.Length == 14 ) {
                string sql = String.Format("select distinct c.cmp_id from compounds c join inchis_102b_std_md5 i5 on c.cmp_id = i5.cmp_id where i5.inchi_key_a = '{0}'", DbUtil.prepare4sql(query));
                result = s_csdb.DBU.m_fetchColumn<int>(sql);
                if ( result.Count != 0 ) {
                    message = "Found by part of InChIKey";
                    return result;
                }

                sql = String.Format("select distinct c.cmp_id from compounds c join inchis_std_md5 i5 on c.cmp_id = i5.cmp_id where i5.inchi_key_a = '{0}'", DbUtil.prepare4sql(query));
                result = s_csdb.DBU.m_fetchColumn<int>(sql);
                if ( result.Count != 0 ) {
                    message = "Found by part of Standard InChIKey";
                    return result;
                }
            }

            return null;
        }

        private static List<int> search_inchi(string query, out string message)
        {
            message = null;

            if ( !query.StartsWith("InChI=", StringComparison.OrdinalIgnoreCase) )
                return null;

            // We have InChI - let's use it
            string[] inchi_layers = InChIUtils.getInChILayers(query);
            if ( inchi_layers == null )
                return null;

            string[] tables = { "inchis_102b_std_md5", "inchis_std_md5" };
            foreach ( string table in tables ) {
                List<int> result = search_inchi(inchi_layers, table, out message);
                if ( result != null && result.Count > 0 )
                    return result;
            }

            return null;
        }

        private static List<int> search_inchi(string[] inchi_layers, string table_name, out string message)
        {
            message = null;

            // Non-standard InChI
            string sql = String.Format("select cmp_id from {0} where inchi_md5 = HashBytes('md5', '{1}')", table_name, DbUtil.prepare4sql(inchi_layers[(int)InChILayers.ALL]));
            List<int> result = s_csdb.DBU.m_fetchColumn<int>(sql);
            if ( result.Count != 0 ) {
                message = "Found by InChI (full match)";
                return result;
            }

            sql = String.Format("select cmp_id from {0} where inchi_chsi_md5 = HashBytes('md5', '{1}')", table_name, DbUtil.prepare4sql(inchi_layers[(int)InChILayers.CHSI]));
            result = s_csdb.DBU.m_fetchColumn<int>(sql);
            if ( result.Count != 0 ) {
                message = "Found by InChI (tautomer/stereo mismatch)";
                return result;
            }

            sql = String.Format("select cmp_id from {0} where inchi_ch_md5 = HashBytes('md5', '{1}')", table_name, DbUtil.prepare4sql(inchi_layers[(int)InChILayers.CH]));
            result = s_csdb.DBU.m_fetchColumn<int>(sql);
            if ( result.Count != 0 ) {
                message = "Found by InChI (connectivity match)";
                return result;
            }

            sql = String.Format("select cmp_id from {0} where inchi_c_md5 = HashBytes('md5', '{1}')", table_name, DbUtil.prepare4sql(inchi_layers[(int)InChILayers.C]));
            result = s_csdb.DBU.m_fetchColumn<int>(sql);
            if ( result.Count != 0 ) {
                message = "Found by InChI (skeleton match)";
                return result;
            }

            return null;
        }
    }

}
