using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChemSpider.Security;
using System.Data.SqlClient;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using System.Text.RegularExpressions;
using ChemSpider.Molecules;
using InChINet;

namespace ChemSpider.Search
{
    public abstract class CSBaseNameSearch : CSSearch
    {
        public new BaseNameSearchOptions Options
        {
            get { return base.Options as BaseNameSearchOptions; }
        }

        public override void SetOptions(SearchOptions options, CommonSearchOptions common, SearchScopeOptions scopeOptions, SearchResultOptions resultOptions)
        {
            if ( (options as BaseNameSearchOptions) == null )
                throw new ArgumentException("Options is null or of wrong type");

            base.SetOptions(options, common, scopeOptions, resultOptions);

            Description = NameSearchHelper.GetDescription(Options);
        }

        protected string GetPredicate(string column, bool fulltext)
        {
            return NameSearchHelper.GetPredicate(Options, column, fulltext);
        }
    }

    public class CSNameSearch : CSBaseNameSearch
    {
        public new NameSearchOptions Options
        {
            get { return base.Options as NameSearchOptions; }
        }

        public override void SetOptions(SearchOptions options, CommonSearchOptions common, SearchScopeOptions scopeOptions, SearchResultOptions resultOptions)
        {
            base.SetOptions(options, common, scopeOptions, resultOptions);
            Description = string.Format(Description, "NAME");
        }

        public override void Run(Sandbox sandbox, CSSearchResult result)
        {
            try {
                string clause = "and syn_id {0} in (select syn_id from compounds_synonyms where approved_yn = 1)";
                switch ( Options.ApprovedAssociationsClause ) {
                    case NameSearchOptions.EApprovedAssociationsClause.Exclude:
                        clause = string.Format(clause, "not");
                        break;
                    case NameSearchOptions.EApprovedAssociationsClause.Only:
                        clause = string.Format(clause, "");
                        break;
                    case NameSearchOptions.EApprovedAssociationsClause.Include:
                        clause = string.Empty;
                        break;
                    default:
                        throw new ArgumentException("Unrecognized clause type");

                }
                string sql = string.Format(
                    "select {0} syn_id from synonyms where deleted_yn = 0 and {1} " + clause,
                    ResultOptions == null || ResultOptions.Limit <= 0 ? string.Empty : "top " + ResultOptions.Limit, GetPredicate("synonym", true));
                using ( SqlConnection conn = new SqlConnection(ChemSpiderDB.RO_ConnectionString) ) {
                    List<int> list = new List<int>();
                    conn.ExecuteReader(sql, r => { list.Add((int)r[0]); });
                    result.Found = new ResultList(EResultObjectType.Synonym, list);
                }
                result.Description = Description;
                result.Status = ERequestStatus.ResultReady;
                result.Progress = 1;
                result.Message = "Finished";
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

    public class CSKeywordSearch : CSSqlSearch
    {
        public new KeywordSearchOptions Options
        {
            get { return base.Options as KeywordSearchOptions; }
        }

        protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
        {
            bool bAdded = false;
            if ( Options.QueryText != String.Empty ) {
                visual.Add("IDENTIFIER = '" + Options.QueryText + "'");

                if ( Options.QueryType == KeywordSearchOptions.EQueryType.InChI ) {
                    if ( Options.QueryText.StartsWith("InChI=", StringComparison.CurrentCultureIgnoreCase) ) {

                        predicates.Add("i5.cmp_id = c.cmp_id");
                        predicates.Add("i5.inchi_md5 = HashBytes('md5', '" + DbUtil.prepare4sql(Options.QueryText) + "')");

                        if ( Options.QueryText.StartsWith("InChI=1S", StringComparison.CurrentCultureIgnoreCase) ) {
                            tables.Add("inchis_std_md5 i5");
                        }
                        else {
                            tables.Add("inchis_md5 i5");
                        }
                    }
                    else {
                        string message;
                        string[] inchi_layers = InChIUtils.getInChILayers(ChemIdUtils.anyId2InChI(Options.QueryText, out message, InChIFlags.Default));
                        if ( inchi_layers == null )
                            predicates.Add("c.cmp_id in (select cs.cmp_id from compounds_synonyms cs, synonyms y where cs.deleted_yn <> 1 and y.deleted_yn <> 1 and cs.syn_id = y.syn_id and y.synonym = '" + DbUtil.prepare4sql(Options.QueryText) + "')");
                        else {
                            predicates.Add("i5.cmp_id = c.cmp_id");
                            predicates.Add("i5.inchi_chsi_md5 = HashBytes('md5', '" + DbUtil.prepare4sql(inchi_layers[1]) + "')");
                            tables.Add("inchis_md5 i5");
                        }
                    }
                }
                else if ( Options.QueryType == KeywordSearchOptions.EQueryType.InChIKey ) {
                    //InChiKey or Std InChIKey
                    if ( Options.QueryText.Length == 25 || Options.QueryText.Length == 27 ) {
                        predicates.Add("i5.cmp_id = c.cmp_id");
                        predicates.Add("i5.inchi_key = '" + DbUtil.prepare4sql(Options.QueryText) + "'");

                        tables.Add(InChIUtils.isStdInChIKey(Options.QueryText) ? "inchis_std i5" : "inchis_md5 i5");
                    }
                    /*else if ( QueryKeyword.Length == 14 ) {*/
                    else {
                        predicates.Add("i5.cmp_id = c.cmp_id");
                        predicates.Add("i5.inchi_key_a = '" + DbUtil.prepare4sql(Options.QueryText) + "'");
                        tables.Add("inchis_std_md5 i5");
                    }
                    //Jon Steele - No point throwing this exception as it is not currently caught anywhere.
                    /*
                    else {
                    
                        //throw new FormatException("Supplied identifier does not look like InChIKey");
                    }*/
                }
                else if ( Options.QueryType == KeywordSearchOptions.EQueryType.Synonym ) {
                    predicates.Add("c.cmp_id in (select cs.cmp_id from compounds_synonyms cs, synonyms y where cs.deleted_yn <> 1 and y.deleted_yn <> 1 and cs.syn_id = y.syn_id and y.synonym = '" + DbUtil.prepare4sql(Options.QueryText) + "')");
                }
                else if ( Options.QueryType == KeywordSearchOptions.EQueryType.Substring ) {
                    predicates.Add("c.cmp_id in (select cs.cmp_id from compounds_synonyms cs, synonyms y where cs.deleted_yn <> 1 and y.deleted_yn <> 1 and cs.syn_id = y.syn_id and contains(y.synonym, '\"" + DbUtil.prepare4sql(Options.QueryText) + "\"'))");
                }
                else if ( Options.QueryType == KeywordSearchOptions.EQueryType.Approximate ) {
                    Regex re = new Regex(@"\s+|-");
                    string[] tokens = re.Split(DbUtil.prepare4sql(Options.QueryText));
                    string pred = String.Empty;
                    foreach ( string token in tokens )
                        pred += " NEAR \"" + token + "\"";
                    predicates.Add("c.cmp_id in (select cs.cmp_id from compounds_synonyms cs, synonyms y where cs.deleted_yn <> 1 and y.deleted_yn <> 1 and cs.syn_id = y.syn_id and contains(y.synonym, '" + pred.Substring(5) + "'))");
                }

                bAdded = true;
            }

            return bAdded;
        }
    }

    class NameSearchHelper
    {
        public static string GetDescription(BaseNameSearchOptions options)
        {
            string op;
            switch ( options.MatchType ) {
                case IdentifierSearchOptions.EMatchType.Approximate:
                    op = "~";
                    break;
                case IdentifierSearchOptions.EMatchType.Exact:
                    op = "=";
                    break;
                case IdentifierSearchOptions.EMatchType.Regex:
                    op = "=~";
                    break;
                case IdentifierSearchOptions.EMatchType.Substring:
                    op = "CONTAINS";
                    break;
                default:
                    throw new ArgumentException("Unsupported comparison operation");
            }
            return string.Format("{{0}} {0} '{1}'", op, options.QueryText);
        }

        public static string GetPredicate(BaseNameSearchOptions options, string column, bool fulltext)
        {
            string predicate;
            string query = DbUtil.prepare4sql(options.QueryText);
            string not = string.Empty;
            if ( query.StartsWith("!") ) {
                query = query.Substring(1);
                not = " NOT ";
            }
            if ( options.MatchType == IdentifierSearchOptions.EMatchType.Exact ) {
                predicate = string.Format("{2}{0} = '{1}'", column, query, not);
            }
            else if ( options.MatchType == BaseNameSearchOptions.EMatchType.Substring ) {
                if ( fulltext )
                    predicate = string.Format("{2}contains({0}, '\"{1}\"')", column, query, not);
                else
                    predicate = string.Format("{0} {2}LIKE '%{1}%'", column, query, not); // other columns are not full-text indexed
            }
            else if ( options.MatchType == BaseNameSearchOptions.EMatchType.Regex ) {
                predicate = string.Format("{2}(dbo.IsMatchRegex({0}, '{1}') = 1)", column, query, not);
            }
            else if ( options.MatchType == BaseNameSearchOptions.EMatchType.Approximate ) {
                Regex re = new Regex(@"\s+|-");
                string[] tokens = re.Split(query);
                if ( fulltext ) {
                    string pred = String.Empty;
                    foreach ( string token in tokens )
                        pred += " NEAR \"" + token + "\"";
                    predicate = not + "contains(synonym, '" + pred.Substring(5) + "')";
                }
                else {
                    StringBuilder p = new StringBuilder();
                    foreach ( string token in tokens ) {
                        p.AppendFormat(string.Format("OR {0} LIKE '%{1}%'", column, token));
                    }
                    predicate = not + "(" + p.ToString().Substring(3) + ")";
                }
            }
            else
                throw new ArgumentException("Unsupported comparison operation");
            return predicate;
        }
    }

    public class CSIdentifierSearch : CSSqlSearch
    {
        public new IdentifierSearchOptions Options
        {
            get { return base.Options as IdentifierSearchOptions; }
        }

        internal override ResultList GetResults(SqlCommand cmd)
        {
            ResultList list;
            if ( Options.IdentifierType == IdentifierSearchOptions.EIdentifierType.Name )
                list = new ResultList(EResultObjectType.SynonymCompound, null);
            else
                list = new ResultList(EResultObjectType.Compound, null);
            using ( SqlDataReader r = cmd.ExecuteReader() ) {
                while ( r.Read() ) {
                    int cmp_id = (int)r[0];
                    if ( Options.IdentifierType == IdentifierSearchOptions.EIdentifierType.Name ) {
                        int syn_id = (int)r[1];
                        list.Add(cmp_id, syn_id);
                    }
                    else
                        list.Add(cmp_id);
                }
            }
            return list;
        }

        protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
        {
            if ( Options.IdentifierType == IdentifierSearchOptions.EIdentifierType.Name )
                columns.Add(Uniq("y++.syn_id"));
            orderby.Add("c.cmp_id");
            string descr = NameSearchHelper.GetDescription(Options);
            switch ( Options.IdentifierType ) {
                case IdentifierSearchOptions.EIdentifierType.InChI:
                    tables.Add(Uniq("inchis i++"));
                    predicates.Add(Uniq("c.cmp_id = i++.cmp_id"));
                    predicates.Add("c.deleted_yn <> 1");
                    predicates.Add(NameSearchHelper.GetPredicate(Options, Uniq("i++.inchi"), false));
                    visual.Add(string.Format(descr, "INCHI"));
                    break;
                case IdentifierSearchOptions.EIdentifierType.SMILES:
                    predicates.Add(NameSearchHelper.GetPredicate(Options, "SMILES", false));
                    visual.Add(string.Format(descr, "SMILES"));
                    break;
                case IdentifierSearchOptions.EIdentifierType.Name:
                    tables.Add(Uniq("compounds_synonyms cs++"));
                    tables.Add(Uniq("synonyms y++"));
                    predicates.Add(Uniq("y++.syn_id = cs++.syn_id"));
                    predicates.Add(Uniq("c.cmp_id = cs++.cmp_id"));
                    predicates.Add(Uniq("y++.deleted_yn <> 1"));
                    predicates.Add(Uniq("cs++.deleted_yn <> 1"));
                    predicates.Add(NameSearchHelper.GetPredicate(Options, Uniq("y++.synonym"), true));
                    visual.Add(string.Format(descr, "NAME"));

                    switch ( Options.ApprovedAssociationsClause ) {
                        case NameSearchOptions.EApprovedAssociationsClause.Exclude:
                            predicates.Add("cs++.approved_yn = 0");
                            visual.Add("[!approved]");
                            break;
                        case NameSearchOptions.EApprovedAssociationsClause.Only:
                            predicates.Add("cs++.approved_yn = 1");
                            visual.Add("[approved]");
                            break;
                        case NameSearchOptions.EApprovedAssociationsClause.Include:
                            break;
                        default:
                            throw new ArgumentException("Unrecognized clause type");
                    }

                    foreach ( string f in Options.SetFlags ) {
                        predicates.Add(string.Format("cs++.{0} = 1", f));
                        visual.Add(string.Format("[{0}]", f));
                    }
                    foreach ( string f in Options.UnsetFlags ) {
                        predicates.Add(string.Format("cs++.{0} = 0", f));
                        visual.Add(string.Format("[!{0}]", f));
                    }
                    break;
                default:
                    throw new ArgumentException("Unsupported identifier type");
            }
            return true;
        }
    }
}
