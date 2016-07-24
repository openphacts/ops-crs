using ChemSpider.Search;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ChemSpider.Compounds;
using ChemSpider.Compounds.Search;
using ChemSpider.Compounds.Database;

namespace OpenPHACTSWeb.WebAPI.Controllers
{
/*
    /// <summary>
    /// General API for running searches on compounds database
    /// </summary>
    public class SearchesController : ApiController
    {
        protected CompoundProvider compoundProvider = new CompoundProvider();

        /// <summary>
        /// Run a simple search which tries to interpret a query string as anything it can search by (Synonym, SMILES, InChI, ChemSpider ID etc.)
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <param name="commonOptions"></param>
        /// <param name="scopeOptions"></param>
        /// <param name="resultOptions"></param>
        /// <returns>Request ID</returns>
        [Route("api/searches/simple")]
        [HttpGet]
        public string SimpleSearch(
            [FromUri] SimpleSearchOptions searchOptions,
            [FromUri] CommonSearchOptions commonOptions,
            [FromUri] SearchScopeOptions scopeOptions,
            [FromUri] SearchResultOptions resultOptions)
        {
            return SearchUtility.RunSearch(CSCSearchFactory.GetSimpleSearch(), searchOptions, commonOptions, scopeOptions, resultOptions).Rid;
        }

        /// <summary>
        /// Run identical structure search
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <param name="commonOptions"></param>
        /// <param name="scopeOptions"></param>
        /// <param name="resultOptions"></param>
        /// <returns>Request ID</returns>
        [Route("api/searches/exact")]
        [HttpGet]
        public string ExactStructureSearch(
            [FromUri] ExactStructureSearchOptions searchOptions,
            [FromUri] CommonSearchOptions commonOptions,
            [FromUri] SearchScopeOptions scopeOptions,
            [FromUri] SearchResultOptions resultOptions)
        {
            return SearchUtility.RunSearch(CSCSearchFactory.GetExactStructureSearch(), searchOptions, commonOptions, scopeOptions, resultOptions).Rid;
        }

        /// <summary>
        /// Run substructure search
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <param name="commonOptions"></param>
        /// <param name="scopeOptions"></param>
        /// <param name="resultOptions"></param>
        /// <returns>Request ID</returns>
        [Route("api/searches/substructure")]
        [HttpGet]
        public string SubstructureSearch(
            [FromUri] SubstructureSearchOptions searchOptions,
            [FromUri] CommonSearchOptions commonOptions,
            [FromUri] SearchScopeOptions scopeOptions,
            [FromUri] SearchResultOptions resultOptions)
        {
            return SearchUtility.RunSearch(CSCSearchFactory.GetSubstructureSearch(), searchOptions, commonOptions, scopeOptions, resultOptions).Rid;
        }

        /// <summary>
        /// Run structure similarity search
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <param name="commonOptions"></param>
        /// <param name="scopeOptions"></param>
        /// <param name="resultOptions"></param>
        /// <returns>Request ID</returns>
        [Route("api/searches/similarity")]
        [HttpGet]
        public string SimilarityStructureSearch(
            [FromUri] SimilaritySearchOptions searchOptions,
            [FromUri] CommonSearchOptions commonOptions,
            [FromUri] SearchScopeOptions scopeOptions,
            [FromUri] SearchResultOptions resultOptions)
        {
            return SearchUtility.RunSearch(CSCSearchFactory.GetSimilarityStructureSearch(), searchOptions, commonOptions, scopeOptions, resultOptions).Rid;
        }

        /// <summary>
        /// Run advanced search as a union of all other search types.
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <param name="commonOptions"></param>
        /// <param name="resultOptions"></param>
        /// <returns>Request ID</returns>
        [Route("api/searches/advanced")]
        [HttpGet]
        public string AdvancedSearch(
            [FromUri] AdvancedSearchOptions searchOptions,
            [FromUri] CommonSearchOptions commonOptions,
            [FromUri] SearchResultOptions resultOptions)
        {
            return SearchUtility.RunSearch(CSCSearchFactory.GetAdvancedSearch(), searchOptions, commonOptions, null, resultOptions).Rid;
        }

        /// <summary>
        /// Returns the status of request
        /// </summary>
        /// <param name="rid">Request ID that was returned by the search procedure</param>
        /// <returns></returns>
        [Route("api/searches/status/{rid}")]
        [HttpGet]
        public RequestStatus SearchStatus(string rid)
        {
            RequestStatus status = ChemSpider.Search.Request.getRequestStatus(rid);

            return status;
        }

        /// <summary>
        /// Get search results by Request ID
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns>List of ChemSpider IDs</returns>
        [Route("api/searches/result/{rid}")]
        [HttpGet]
        public IEnumerable<int> SearchResult(string rid, int start = 0, int count = 0)
        {
            CSRequestSearchResult result = new CSRequestSearchResult(rid);
            List<int> found = result.Found.ToList();
            return found.Skip(start).Take(count <= 0 ? found.Count - start : count).ToList();
        }

        /// <summary>
        /// Get two columns (CSID and Relevance) search results by Request ID
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [Route("api/searches/resultwithrelevance/{rid}")]
        [HttpGet]
        public IEnumerable<ResultRecord> SearchResultWithRelevance(string rid, int start = 0, int count = 0)
        {
            CSRequestSearchResult result = new CSRequestSearchResult(rid);
            List<ResultRecord> res = result.Found.GetResultRecords();
            return res.Skip(start).Take(count <= 0 ? res.Count - start : count).ToList();
        }

        /// <summary>
        /// Get two columns (CSID and Relevance) search results by Request ID
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [Route("api/searches/resultasrecords/{rid}")]
        [HttpGet]
        public IEnumerable<Compound> SearchResultAsCompounds(string rid, int start = 0, int count = 0)
        {
            CSRequestSearchResult result = new CSRequestSearchResult(rid);
            IEnumerable<int> found = result.Found.ToList();

            List<Compound> compounds = new List<Compound>();

            foreach (var id in found.Skip(start).Take(count <= 0 ? found.Count() - start : count))
            {
                Compound c = compoundProvider.GetCompound(id);
                compounds.Add(c);
            }

            return compounds;
        }
    }
*/
}
