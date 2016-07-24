using RSC.Compounds.Search;
using ChemSpider.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace RSC.Compounds.NMRFeatures.WebAPI
{
	public class NMRSearchController : ApiController
	{
		private readonly INMRFeaturesSearch search;

		public NMRSearchController(INMRFeaturesSearch search)
		{
			if (search == null)
			{
				throw new ArgumentNullException("search");
			}

			this.search = search;
		}

		/// <summary>
		/// Run NMR features search
		/// </summary>
		/// <param name="searchOptions"></param>
		/// <param name="commonOptions"></param>
		/// <param name="scopeOptions"></param>
		/// <param name="resultOptions"></param>
		/// <returns>Request ID</returns>
		[Route("api/searches/nmrfeatures")]
		[HttpGet]
		public IHttpActionResult NMRFeaturesSearch(
			[FromUri] NMRFeaturesSearchOptions searchOptions,
			[FromUri] CommonSearchOptions commonOptions,
			[FromUri] SearchScopeOptions scopeOptions,
			[FromUri] SearchResultOptions resultOptions)
		{
			string rid = SearchUtility.RunAsyncSearch(search.Instance, searchOptions, commonOptions, scopeOptions, resultOptions).Rid;

			return Ok(rid);
		}
	}
}
