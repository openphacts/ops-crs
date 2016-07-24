using System;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Configuration;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using ChemSpider.Database;
using ChemSpider.Data.Database;

namespace ChemSpider.Molecules
{
    /// <summary>
    /// Set of methods for handling name and synonym lookup in ChemSpider.
    /// </summary>
    public class NameUtility
    {
        private static ChemSpiderDB s_csdb
        {
            get
            {
                return new ChemSpiderDB();
            }
        }

        static public List<int> getCSIDsForApprovedSynonym(string query)
        {
            string sql = @"select c.cmp_id from compounds c, compounds_synonyms cs, synonyms s
                        where c.deleted_yn = 0 and c.cmp_id = cs.cmp_id and cs.opinion = 'Y' and cs.approved_yn = 1
                            and cs.deleted_yn = 0 and s.deleted_yn = 0 and cs.syn_id = s.syn_id
                            and s.synonym= '" + DbUtil.prepare4sql(query) + @"' 
                         order by dbo.fSynonymMatchScore(c.cmp_id, cs.syn_id) desc";
            return s_csdb.DBU.m_fetchColumn<int>(sql);
        }

        static public List<int> getCSIDsForSynonym(string query)
        {
            string sql = @"select c.cmp_id from compounds c, compounds_synonyms cs, synonyms s
                        where c.deleted_yn = 0 and c.cmp_id = cs.cmp_id
                            and cs.deleted_yn = 0 and s.deleted_yn = 0 and cs.syn_id = s.syn_id
                            and s.synonym= '" + DbUtil.prepare4sql(query) + @"' 
                         order by dbo.fSynonymMatchScore(c.cmp_id, cs.syn_id) desc";
            return s_csdb.DBU.m_fetchColumn<int>(sql);
        }

        /// <summary>
        /// This returns the same name as the title that appears on
        /// RecordView.aspx.
        /// </summary>
        /// <param name="csid">ChemSpider Id.</param>
        /// <returns>The best name for a ChemSpider Id.</returns>
        public static string getBestNameForCSID(int csid)
        {
            return s_csdb.GetCompoundTitle(csid);
        }

        public enum SynonymsSelector
        {
            /// Returns all synonyms.
            All,
            /// Returns all validated synonyms.
            Validated,
            /// Returns all non-validated synonyms.
            NonValidated,
        }

        public enum ValidatedSynonymsSelector
        {
            /// Returns all validated synonyms.
            All,
            /// Returns only those validated synonyms we want to use for SEO in the description meta tag.
            SEO,
            /// Returns only those validated synonyms we want to use to pass to the RSC Publishing Platform.
            RSC,
        }

        /// <summary>
        /// Returns all synonyms for CSID.
        /// </summary>
        /// <param name="csid">The ChemSpider Id.</param>
        /// <param name="selector">The selection type enumeration.</param>
        /// <returns>Returns a list of Synonyms.</returns>
        public static List<string> getSynonymsForCSID(int csid, SynonymsSelector selector = SynonymsSelector.Validated)
        {
            string sel =
                selector == SynonymsSelector.All ? "" :
                selector == SynonymsSelector.Validated ? "and cs.opinion = 'Y' and cs.approved_yn = 1" :
                "and (cs.opinion is null or cs.opinion = 'Y' and cs.approved_yn = 0)";

            return s_csdb.DBU.m_fetchColumn<string>(String.Format(@"
				select s.synonym
				from compounds_synonyms cs join synonyms s on cs.syn_id = s.syn_id
				where cs.cmp_id = {0} {1} and cs.deleted_yn = 0 and s.deleted_yn = 0
				order by dbo.fSynonymMatchScore(cs.cmp_id, cs.syn_id) desc", csid, sel));
        }

        /// <summary>
        /// Filters the list of validated synonyms based on the various filter types.
        /// </summary>
        /// <param name="dict">A dictionary containing the synonyms for filtering.</param>
        /// <param name="selector">The enum describing the selection method.</param>
        /// <returns>A list containing the filtered synonyms.</returns>
        public static List<string> FilterValidatedSynonyms(Dictionary<string, int> dict, ValidatedSynonymsSelector selector = ValidatedSynonymsSelector.All)
        {
            switch (selector)
            {
                //Only include those with a ranking > 1 if we are getting seo synonyms.
                case ValidatedSynonymsSelector.SEO:
                    return stripFlags(dict.Where(n => n.Value > 1).ToDictionary(n => n.Key, n => n.Value).Keys.ToList());                    
                //Exclude EINECS (rank=2) for RSC and strip out any flags. Include those with no explicit flag set.
                case ValidatedSynonymsSelector.RSC:
                    return stripFlags(dict.Where(n => n.Value > 0 && n.Value != 2 /* 2=EINECS */).ToDictionary(n => n.Key, n => n.Value).Keys.ToList());
                //Otherwise return the entire list of validated synonyms with no extra filtering.
                default:
                    return new List<string>(dict.Keys);
            }
        }

        /// <summary>
        /// Get the Validated synonymns based on various business rules.
        /// Either:
        /// 
        /// i) All validated synonyms.
        /// ii) For SEO purposes so these will be displayed in the page meta description.
        /// iii) For sending to the RSC publishing platform and certain items are removed E.g. EINECS.
        /// 
        /// </summary>
        /// <param name="cmp_id">The compound id.</param>
        /// <param name="selector">The selection enum.</param>
        /// <returns>The filtered list of synonyms based on the selection enum.</returns>
        public static List<string> GetValidatedSynonyms(int cmp_id, ValidatedSynonymsSelector selector = ValidatedSynonymsSelector.All)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();

            //Exclude DBID and WLN. Exclude foreign names if we require a ranking. Exclude all synonyms less than 3 chars or greater than 50.
            //Give those with no ranking a value of 1 which is higher than those explicitly set to 0.
            DataTable rawList = s_csdb.DBU.FillDataTable(
                @"SELECT s.synonym, 
                    MAX (ISNULL(ssf.rank, 1)) ranking
                FROM compounds_synonyms cs 
                    JOIN synonyms s 
                        ON s.syn_id = cs.syn_id 
                    LEFT JOIN v_synonyms_synonyms_flags ssf 
                        ON s.syn_id = ssf.syn_id
                WHERE cs.cmp_id = @cmp_id
                    AND LEN(s.synonym) > 3 
                    AND LEN(s.synonym) < 50 
                    AND cs.approved_yn = 1 
                    AND opinion = 'Y' 
                    AND s.deleted_yn = 0
                    AND cs.deleted_yn = 0
                    AND NOT EXISTS (SELECT 1 FROM v_synonyms_synonyms_flags x
                                        WHERE x.syn_id = s.syn_id
                                            AND ( x.name IN ('DBID', 'WLN') ))
                    AND (@ranking = 0 OR lang_id1 = 'en')
                GROUP BY s.syn_id, s.synonym
            ORDER BY ranking DESC, LEN(synonym), synonym", new { cmp_id = cmp_id, ranking = (selector == ValidatedSynonymsSelector.SEO || selector == ValidatedSynonymsSelector.RSC) });

            foreach ( DataRow synonym in rawList.Rows ) {
                string syn = synonym[0].ToString();
                if ( !Regex.Match(syn, "^[0-9- ]+$").Success || RegistryNumbers.isCASNumber(syn) ) {
                    if ( syn.IndexOf("&") < 0 && !dict.ContainsKey(syn) ) {
                        dict.Add(syn, Convert.ToInt32(synonym[1]));
                    }
                }
            }
            
            //Return the list filtered using the selector.
            return FilterValidatedSynonyms(dict, selector);
        }

        /// <summary>
        /// Passed a list of synonyms and strips the flags using regular expression.
        /// </summary>
        /// <param name="synonyms">A list of strings containing synonyms.</param>
        /// <returns>A list of strings with the flags stripped.</returns>
        private static List<string> stripFlags(List<string> synonyms)
        {
            //return new List<string>(synonyms.Select(i => Regex.Replace(i.Key.ToString(), @" \[[A-Za-z]+\]$", "")));
            return synonyms.Select(i => Regex.Replace(i, @" \[[A-Za-z]+\]$", "")).ToList();
        }

        /// <summary>
        /// Returns all wikipedia urls for CSID as a string.
        /// </summary>
        /// <param name="csid">The ChemSpider Id.</param>
        /// <returns>Returns a list of Wikipedia urls.</returns>
        public static List<string> getWikipediaURLsForCSID(int csid)
        {
            return s_csdb.DBU.m_fetchColumn<string>(String.Format(@"
				select substances.ext_id
                from substances
                join [data_sources]
                on [substances].[dsn_id] = [data_sources].[dsn_id]
                where [data_sources].[name] ='Wikipedia' and substances.deleted_yn = 0 and substances.cmp_id = {0}", csid));
        }


    }
}