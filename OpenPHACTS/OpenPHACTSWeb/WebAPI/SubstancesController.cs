using System.Collections.Generic;
using System.Web.Http;
using ChemSpider.ObjectModel;
using ChemSpider.Compounds;
using ChemSpider.Compounds.Database;

namespace OpenPHACTSWeb.WebAPI
{
/*
    public class SubstancesController : ApiController
    {
        protected SubstanceProvider substanceProvider = new SubstanceProvider();

        // GET api/Substances
        /// <summary>
        /// Returns list of substances
        /// </summary>
        /// <param name="start">Index where to start returning substances</param>
        /// <param name="count">Number of returned substances</param>
        /// <returns>List of substances</returns>
        [Route("api/substances")]
        public IHttpActionResult GetSubstances(int start = 0, int count = 10)
        {
            var substances = substanceProvider.GetSubstances(start, count);

            return Ok(substances);
        }

        // GET api/substances/5
        /// <summary>
        /// Returns substance information by ID
        /// </summary>
        /// <param name="id">Internal substance ID</param>
        /// <returns>Substance object</returns>
        [Route("api/substances/{id}")]
        public IHttpActionResult Get(int id)
        {
            Substance substance = substanceProvider.GetSubstance(id);

            return Ok(substance);
        }

        // GET api/substances/5/issues
        /// <summary>
        /// Returns list of substance's issues
        /// </summary>
        /// <param name="id">Internal substance ID</param>
        /// <returns>List of issues</returns>
        [Route("api/substances/{id}/issues")]
        public IHttpActionResult GetIssues(int id)
        {
            IEnumerable<Issue> issues = substanceProvider.GetIssues(id);

            return Ok(issues);
        }

        // GET api/substances/5/synonyms
        /// <summary>
        /// Returns list of substance's synonyms
        /// </summary>
        /// <param name="id">Internal substance ID</param>
        /// <returns>List of synonyms</returns>
        [Route("api/substances/{id}/synonyms")]
        public IHttpActionResult GetSynonyms(int id)
        {
            IEnumerable<string> synonyms = substanceProvider.GetSynonyms(id);

            return Ok(synonyms);
        }
    }
*/
}
