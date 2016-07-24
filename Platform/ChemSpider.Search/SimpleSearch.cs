using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using ChemSpider.Security;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using ChemSpider.Molecules;
using InChINet;

namespace ChemSpider.Search
{
    public class CSSimpleSearch : CSSqlSearch
    {
        public new SimpleSearchOptions Options
        {
            get { return base.Options as SimpleSearchOptions; }
        }

        protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
        {
            // to generate appropriate Description only
            visual.Add(Options.QueryText);

            return true;
        }

        public override void SetOptions(SearchOptions options, CommonSearchOptions common, SearchScopeOptions scopeOptions, SearchResultOptions resultOptions)
        {
            if ( options as SimpleSearchOptions == null )
                throw new ArgumentException("Options is null or of wrong type");

            base.SetOptions(options, common, scopeOptions, resultOptions);
        }

        protected bool TrySearch(CSSearchResult res, SqlConnection conn, Sandbox sandbox, List<string> tables, List<string> predicates, List<string> orderby, bool isDistinct, ESimpleSearchMatchType matchType)
        {
            //Build the SQL Statement - don't add any limit or ordering yet.
            res.Count = conn.ExecuteScalar<int>(ComposeSQL(sandbox, -1, tables, predicates, orderby, true, isDistinct));

            //If the count is greater than 0 then we have our results so run the proper SQL.
            if (res.Count > 0)
            {
                res.Found = new ResultList(EResultObjectType.Compound, conn.FetchColumn<int>(ComposeSQL(sandbox, ResultOptions.Limit, tables, predicates, ResultOptions.SortOrder.Count > 0 ? ResultOptions.SortOrder : orderby, false, isDistinct)));
                res.Message = SearchResultStatement.SearchResultToDescription(matchType);
                res.ResultMatchType = matchType;
                return true;
            }

            return false;
        }

        protected virtual void getStandardOptions(Sandbox sandbox, ref List<string> tables, ref List<string> predicates, ref List<string> orderby)
        {
            //Get any common options from the base class
            predicates = m_Predicates.ToList();
            tables = m_Tables.ToList();
            orderby = m_Orderby.ToList();

            //Add the standard tables.
            tables.Add("compounds c");
            predicates.Add("c.deleted_yn = 0");

            //Add the sandbox.
            if (sandbox != null)
            {
                tables.Add(" JOIN compounds_datasources cds ON cds.cmp_id = c.cmp_id ");
                predicates.Add(String.Format("cds.dsn_id = {0}", sandbox.DsnId));
            }
        }

        protected virtual void getInChIOptions(Sandbox sandbox, ref List<string> tables, ref List<string> predicates, ref List<string> orderby, bool bStandardInChI)
        {
            //Add the standard tables and sandbox.
            getStandardOptions(sandbox, ref tables, ref predicates, ref orderby);
            //Add the InChI Options.
            tables.Add(" JOIN " + (bStandardInChI ? "inchis_std_md5 i5" : "inchis_md5 i5") + " ON i5.cmp_id = c.cmp_id ");
        }

        protected string AppendList(List<string> list, string separator)
        {
            if (list.Count > 0)
            {
                //return String.Join(separator, list.Distinct());
                StringBuilder joined = new StringBuilder();
                joined.Append(String.Join(separator, list.Where(s => !s.Contains(" JOIN ")).Distinct()));
                joined.Append(String.Join(string.Empty, list.Where(s => s.Contains(" JOIN ")).Distinct()));
                return joined.ToString();
            }
            else
                return string.Empty;
        }

        protected virtual void GetOrderBy(ref List<string> orderby, ref List<string> cols, ref List<string> tables, ref List<string> predicates)
        {
            //Add default of number of data sources.
            if (orderby.Count == 0)
                orderby.Add("n_ds_total desc");

            //Only use the first orderby in the list, strip the ASC/DESC
            string orderby_col = orderby.First().ToString().ToLower().Replace(" asc", string.Empty).Replace(" desc", string.Empty);

            switch (orderby_col)
            {
                case "n_ds_total":
                    cols.Add(String.Format("ds.ds_count {0}", orderby_col));
                    tables.Add(" JOIN v_compound_ds_count ds ON ds.cmp_id = c.cmp_id ");
                    break;
                case "molecular_weight":
                    cols.Add("c.molecular_weight");
                    break;
                case "n_references":
                    cols.Add(String.Format("ref.ref_count {0}", orderby_col));
                    tables.Add(" JOIN v_compound_ref_count ref ON ref.cmp_id = c.cmp_id ");
                    break;
                case "n_pubmed_hits":
                    cols.Add(String.Format("pubmed.n_hits {0}", orderby_col));
                    tables.Add(" LEFT JOIN compounds_hits pubmed ON pubmed.cmp_id = c.cmp_id AND pubmed.src_id = 1 ");
                    break;
                case "n_rsc_hits":
                    cols.Add(String.Format("rsc.n_hits {0}", orderby_col));
                    tables.Add(" LEFT JOIN compounds_hits rsc ON rsc.cmp_id = c.cmp_id AND rsc.src_id = 2 ");
                    break;
                case "synonym_match_score":
                    cols.Add(String.Format("dbo.fSynonymMatchScore(c.cmp_id, cs.syn_id) {0}", orderby_col));
                    break;
            }
        }

        protected virtual string ComposeSQL(Sandbox sandbox, int limit, List<string> tables, List<string> predicates, List<string> orderby, bool isCountSQL, bool isDistinct)
        {
            StringBuilder sql = new StringBuilder("SELECT ");

            if (!isCountSQL)
            {
                List<string> cols = new List<string>();
                cols.Add("c.cmp_id");
                GetOrderBy(ref orderby, ref cols, ref tables, ref predicates);
                
                if (isDistinct)
                    sql.Append("DISTINCT ");
                if (limit > 0)
                    sql.Append(String.Format("TOP {0} ", limit));
                sql.Append(String.Format("{0}", AppendList(cols, ",")));
            }
            else
            {
                if(isDistinct)
                    sql.Append("COUNT(DISTINCT c.cmp_id)");
                else
                    sql.Append("COUNT(c.cmp_id)");
            }

            sql.Append(" FROM ");
            sql.Append(AppendList(tables, ","));

            sql.Append(" WHERE ");
            sql.Append(AppendList(predicates, " AND "));

            if (!isCountSQL && orderby.Count > 0)
            {
                sql.Append(" ORDER BY ");
                sql.Append(AppendList(orderby, ","));
            }

            return sql.ToString();
        }

        protected virtual bool TrySearches(string query, Sandbox sandbox, CSSearchResult res, bool allowVagueSearch)
        {
            List<string> predicates = new List<string>();
            List<string> tables = new List<string>();
            List<string> orderby = new List<string>();

            using ( SqlConnection conn = new SqlConnection(ChemSpiderDB.RO_ConnectionString) ) {

                // CSID search
                try {
                    IEnumerable<int> csids = query.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s.Trim()));
                    if ( csids.Count() > 0 ) {
                        getStandardOptions(sandbox, ref tables, ref predicates, ref orderby);
                        predicates.Add(String.Format("c.cmp_id in ({0})", String.Join(",", csids)));
                        if ( TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.CSID) )
                            return true;
                    }
                }
                catch { }

                if ( query.StartsWith("CSID", StringComparison.OrdinalIgnoreCase) ) {
                    int csid = 0;
                    if ( query.StartsWith("CSID", StringComparison.OrdinalIgnoreCase) )
                        csid = int.Parse(query.Substring(4));

                    getStandardOptions(sandbox, ref tables, ref predicates, ref orderby);
                    predicates.Add(String.Format("c.cmp_id = {0}", csid));
                    if ( TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.CSID) )
                        return true;
                }

                //// Sandbox full fetch (within data source only - data_collections not covered - where are data_collections used in search?)
                if ( sandbox != null && query.Equals("all", StringComparison.InvariantCultureIgnoreCase) ) {
                    getStandardOptions(sandbox, ref tables, ref predicates, ref orderby);
                    if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.None))
                        return true;
                }

                // Approved synonym search
                getStandardOptions(sandbox, ref tables, ref predicates, ref orderby);
                tables.Add(" JOIN compounds_synonyms cs ON cs.cmp_id = c.cmp_id ");
                tables.Add(" JOIN synonyms y ON y.syn_id = cs.syn_id ");
                predicates.Add("cs.opinion <> 'N'");
                predicates.Add("cs.approved_yn = 1");
                predicates.Add("cs.deleted_yn <> 1");           
                predicates.Add("y.deleted_yn <> 1");
                predicates.Add("y.synonym = '" + DbUtil.prepare4sql(query) + "'");
                orderby.Add("synonym_match_score DESC");
                if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.Synonym))
                    return true;

                // Unapproved synonym search
                getStandardOptions(sandbox, ref tables, ref predicates, ref orderby);
                tables.Add(" JOIN compounds_synonyms cs ON cs.cmp_id = c.cmp_id ");
                tables.Add(" JOIN synonyms y ON y.syn_id = cs.syn_id ");
                predicates.Add("(cs.opinion IS NULL OR cs.opinion <> 'N')");
                predicates.Add("cs.approved_yn = 0");
                predicates.Add("cs.deleted_yn <> 1");
                predicates.Add("y.deleted_yn <> 1");
                predicates.Add("y.synonym = '" + DbUtil.prepare4sql(query) + "'");
                orderby.Add("synonym_match_score DESC");
                
                if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.NonApprovedSynonym))
                    return true;

                // InChIKey
                string inchi_key = query;
                if ( query.StartsWith("InChIKey=", StringComparison.OrdinalIgnoreCase) )
                    inchi_key = query.Substring(9);
                if ( inchi_key.Length == 25 ) {
                    getStandardOptions(sandbox, ref tables, ref predicates, ref orderby);
                    tables.Add(" JOIN inchis_md5 i5 ON i5.cmp_id = c.cmp_id ");
                    predicates.Add("i5.inchi_key = '" + DbUtil.prepare4sql(inchi_key) + "'");
                    if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.InChIKey))
                        return true;
                }

                // Maybe InChIKey
                if ( inchi_key.Length == 27 && InChIUtils.isValidInChIKey(inchi_key) ) {
                    getStandardOptions(sandbox, ref tables, ref predicates, ref orderby);
                    tables.Add(" JOIN inchis_std i ON i.cmp_id = c.cmp_id ");
                    predicates.Add("i.inchi_key = '" + DbUtil.prepare4sql(inchi_key) + "'");
                    if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.InChIKey))
                        return true;
                }

                // Part of InChIKey
                if ( inchi_key.Length == 14 ) {
                    getStandardOptions(sandbox, ref tables, ref predicates, ref orderby);
                    tables.Add(" JOIN inchis_md5 i5 ON i5.cmp_id = c.cmp_id ");
                    predicates.Add("i5.inchi_key_1 = '" + DbUtil.prepare4sql(inchi_key) + "'");
                    if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.InChIKeySkeleton))
                        return true;
                }

                // Part of InChIKey
                if ( inchi_key.Length == 25 || inchi_key.Length == 27 && InChIUtils.isValidInChIKey(inchi_key) ) {
                    getStandardOptions(sandbox, ref tables, ref predicates, ref orderby);
                    tables.Add(" JOIN inchis_md5 i5 ON i5.cmp_id = c.cmp_id ");
                    predicates.Add("i5.inchi_key_1 = '" + DbUtil.prepare4sql(inchi_key.Substring(0, 14)) + "'");
                    if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.InChIKeySkeleton))
                        return true;
                }

                // Try to get InChI to search by
                string[] inchi_layers = null;
                if ( query.StartsWith("InChI=") )
                    inchi_layers = InChIUtils.getInChILayers(query);
                else {
                    string message;

                    //Do not call this if it is a registry number i.e. CAS number - Bots frequently crawl the site looking
                    //for registry numbers - so it is best to not call this if it is a registry number.
                    if(!Regex.IsMatch(query, @"^\d+\-\d+\-\d+$"))
                    {
                        string inchi = ChemIdUtils.anyId2InChI(query, out message, InChIFlags.Default);
                        if ( !String.IsNullOrEmpty(inchi) )
                            inchi_layers = InChIUtils.getInChILayers(inchi);
                    }
                }

                // We have InChI - let's use it
                if ( inchi_layers != null ) {
                    
                    // Non-standard InChI
                    getInChIOptions(sandbox, ref tables, ref predicates, ref orderby, false);
                    predicates.Add("i5.inchi_md5 = HashBytes('md5', '" + DbUtil.prepare4sql(inchi_layers[0]) + "')");
                    if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.ConvertedToInChI))
                        return true;

                    getInChIOptions(sandbox, ref tables, ref predicates, ref orderby, false);
                    predicates.Add("i5.inchi_chsi_md5 = HashBytes('md5', '" + DbUtil.prepare4sql(inchi_layers[4]) + "')");
                    if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.ConvertedToInChITautomerStereoMismatch))
                        return true;

                    getInChIOptions(sandbox, ref tables, ref predicates, ref orderby, false);
                    predicates.Add("i5.inchi_ch_md5 = HashBytes('md5', '" + DbUtil.prepare4sql(inchi_layers[1]) + "')");
                    if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.ConvertedToInChIConnectivityMatch))
                        return true;

                    getInChIOptions(sandbox, ref tables, ref predicates, ref orderby, false);
                    predicates.Add("i5.inchi_c_md5 = HashBytes('md5', '" + DbUtil.prepare4sql(inchi_layers[2]) + "')");
                    if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.ConvertedToInChISkeletonMatch))
                        return true;

                    // Standard InChI
                    getInChIOptions(sandbox, ref tables, ref predicates, ref orderby, true);
                    predicates.Add("i5.inchi_md5 = HashBytes('md5', '" + DbUtil.prepare4sql(inchi_layers[0]) + "')");
                    if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.ConvertedToInChI))
                        return true;

                    getInChIOptions(sandbox, ref tables, ref predicates, ref orderby, true);
                    predicates.Add("i5.inchi_chsi_md5 = HashBytes('md5', '" + DbUtil.prepare4sql(inchi_layers[4]) + "')");
                    if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.ConvertedToInChITautomerStereoMismatch))
                        return true;

                    getInChIOptions(sandbox, ref tables, ref predicates, ref orderby, true);
                    predicates.Add("i5.inchi_ch_md5 = HashBytes('md5', '" + DbUtil.prepare4sql(inchi_layers[1]) + "')");
                    if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.ConvertedToInChIConnectivityMatch))
                        return true;

                    getInChIOptions(sandbox, ref tables, ref predicates, ref orderby, true);
                    predicates.Add("i5.inchi_c_md5 = HashBytes('md5', '" + DbUtil.prepare4sql(inchi_layers[2]) + "')");
                    if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.ConvertedToInChISkeletonMatch))
                        return true;
                }

                // Molecular Formula
                if ( res.FoundCount == 0 ) {
                    string mf = MolecularFormula.prepareMF(query);
                    if ( mf != String.Empty ) {
                        getStandardOptions(sandbox, ref tables, ref predicates, ref orderby);
                        predicates.Add("c.PUBCHEM_OPENEYE_MF = '" + DbUtil.prepare4sql(mf) + "'");
                        if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.MolecularFormula))
                            return true;
                    }
                }

                // Molecular Formula input as a range. E.g. C(12-14) H(5-8) F(0) S(2-4)
                if (res.FoundCount == 0)
                {
                    //Extract the elements from the Range MF.
                    List<Tuple<string, short, short?>> rangeMf = MolecularFormula.prepareRangeMF(query);
                    if (rangeMf != null)
                    {
                        getStandardOptions(sandbox, ref tables, ref predicates, ref orderby);
                        //Add a predicate to join to compound elements and include the extracted range mf as where clause.
                        tables.Add(" JOIN compounds_elements ce ON ce.cmp_id = c.cmp_id ");
                        foreach (Tuple<string, short, short?> i in rangeMf)
                        {
                            if (i.Item3 != null)
                                //We have a low and high value for the element.
                                predicates.Add(String.Format("(ce.el_{0} >= {1} and ce.el_{0} <= {2})", i.Item1, i.Item2, i.Item3));
                            else
                                //We only have a low value so must be an exact match.
                                predicates.Add(String.Format("(ce.el_{0} = {1})", i.Item1, i.Item2));
                        }
                        if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.MolecularFormula))
                            return true;
                    }
                }

                // Queries below would fail with double quotes inside anyway
                if (allowVagueSearch && !query.Contains("\""))
                {
                    getStandardOptions(sandbox, ref tables, ref predicates, ref orderby);
                    tables.Add(" JOIN compounds_synonyms cs ON cs.cmp_id = c.cmp_id ");
                    tables.Add(" JOIN synonyms y ON y.syn_id = cs.syn_id ");
                    predicates.Add("(cs.opinion is null or cs.opinion <> 'N' or cs.approved_yn = 0)");
                    predicates.Add("cs.deleted_yn <> 1");
                    predicates.Add("y.deleted_yn <> 1");
                    predicates.Add("contains(y.synonym, '" + "\"" + DbUtil.prepare4sqlRemoveMarkup(query) + "\"')");
                    if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.FullTextSynonym))
                        return true;

                    //Split into Tokens.
                    Regex re = new Regex(@"\s+");
                    string[] tokens = re.Split(DbUtil.prepare4sql(query));
                    if (tokens.Length > 1)
                    {
                        string predicate = String.Empty;
                        foreach (string token in tokens)
                        {
                            if (!String.IsNullOrEmpty(token))
                                predicate += ",'" + token + "'";
                        }

                        getStandardOptions(sandbox, ref tables, ref predicates, ref orderby);
                        tables.Add(" JOIN compounds_synonyms cs ON cs.cmp_id = c.cmp_id ");
                        tables.Add(" JOIN synonyms y ON y.syn_id = cs.syn_id ");
                        predicates.Add("(cs.opinion is null or cs.opinion <> 'N' or cs.approved_yn = 0)");
                        predicates.Add("cs.deleted_yn <> 1");
                        predicates.Add("y.deleted_yn <> 1");
                        predicates.Add("y.synonym in (" + predicate.Substring(1) + ")");
                        if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.TokenizedSynonym))
                            return true;
                    }

                    //Split into Tokens and remove markup, use NEAR full text predicate.
                    Regex re2 = new Regex(@"\s+|-");
                    string[] tokens2 = re.Split(DbUtil.prepare4sqlRemoveMarkup(query));
                    if (tokens2.Length > 1)
                    {
                        string predicate = String.Empty;
                        foreach (string token in tokens2)
                        {
                            if (!String.IsNullOrEmpty(token))
                                predicate += " NEAR \"" + token + "\"";
                        }

                        getStandardOptions(sandbox, ref tables, ref predicates, ref orderby);
                        tables.Add(" JOIN compounds_synonyms cs ON cs.cmp_id = c.cmp_id ");
                        tables.Add(" JOIN synonyms y ON y.syn_id = cs.syn_id ");
                        predicates.Add("(cs.opinion is null or cs.opinion <> 'N' or cs.approved_yn = 0)");
                        predicates.Add("cs.deleted_yn <> 1");
                        predicates.Add("y.deleted_yn <> 1");
                        predicates.Add("contains(y.synonym, '" + predicate.Substring(5) + "')");
                        if (TrySearch(res, conn, sandbox, tables, predicates, orderby, true, ESimpleSearchMatchType.TokenizedFullTextNearSynonym))
                            return true;
                    }
                }
            }

            //Nothing found.
            return false;
        }

        public override void Run(Sandbox sandbox, CSSearchResult result)
        {
            string query = Options.QueryText;
            if ( query.StartsWith("\"") && query.EndsWith("\"") )
                query = query.Substring(1, query.Length - 2);
            try {
                if (!TrySearches(query, sandbox, result, Options.AllowVagueSearch))
                    result.Found = new ResultList();
                result.Progress = 1;
                result.Status = ERequestStatus.ResultReady;
            }
            catch ( Exception ex ) {
                result.Status = ERequestStatus.Failed;
                result.Message = ex.Message;
            }
            finally {
                result.Update();
            }
        }
    }
}
