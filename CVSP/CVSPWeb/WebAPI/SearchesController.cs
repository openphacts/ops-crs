using CVSPWeb.Models;
using RSC.CVSP.Search;
using RSC.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CVSPWeb.WebAPI
{
	[EnableCors("*", "*", "*")]
	public class SearchesController : ApiController
    {
		/// <summary>
		/// Run records search
		/// </summary>
		/// <param name="search"></param>
		/// <returns>Request ID</returns>
		[Route("api/searches/records")]
		[HttpPost]
		public IHttpActionResult RecordsSearch([FromBody] CVSPRecordsSearchModel search)
		{
			var rid = SearchUtility.RunSearchAsync(typeof(RecordsSearch), search.searchOptions, search.commonOptions, search.scopeOptions, search.resultOptions);

			return Ok(rid);
		}

		/// <summary>
		/// Returns the status of request
		/// </summary>
		/// <param name="rid">Request ID that was returned by the search procedure</param>
		/// <returns></returns>
		[Route("api/searches/{rid}/status")]
		[HttpGet]
		public IHttpActionResult SearchStatus(Guid rid)
		{
			RequestStatus status = RequestManager.Current.GetStatus(rid);

			return Ok(status);
		}

		/// <summary>
		/// Get search results by Request ID
		/// </summary>
		/// <param name="rid"></param>
		/// <param name="start"></param>
		/// <param name="count"></param>
		/// <returns>List of ChemSpider IDs</returns>
		[Route("api/searches/{rid}/result")]
		[HttpGet]
		public IHttpActionResult SearchResult(Guid rid, int start = 0, int count = 10)
		{
			var results = RequestManager.Current.GetResults(rid, start, count).ToList();

			return Ok(from g in results select (Guid)g);
		}
	}
}
