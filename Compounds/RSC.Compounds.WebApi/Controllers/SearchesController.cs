using ChemSpider.Molecules;
using CompoundsWeb.Models;
using RSC.Compounds.Search;
using RSC.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace RSC.Compounds.WebApi
{
	/// <summary>
	/// General API for running searches on compounds database
	/// </summary>
	[EnableCors("*", "*", "*")]
	public class SearchesController : ApiController
	{
		public SearchesController()
		{
		}

		/// <summary>
		/// Run a simple search which tries to interpret a query string as anything it can search by (Synonym, SMILES, InChI, ChemSpider ID etc.)
		/// </summary>
		/// <param name="search"></param>
		/// <returns>Request Id</returns>
		[Route("api/searches/simple")]
		[HttpPost]
		public IHttpActionResult SimpleSearch([FromBody] SimpleSearchModel search)
		{
			ChemSpider.Parts.Client.JSONClient parts = new ChemSpider.Parts.Client.JSONClient() { RedirectCookies = false };

			//  convert name into InChI and try to search InChI in DB...
			ChemIdUtils.N2SResult res = parts.ConvertTo(new ChemIdUtils.ConvertOptions { Direction = ChemIdUtils.ConvertOptions.EDirection.Term2Mol, Text = search.searchOptions.QueryText });
			if (res.confidence == 100)
			{
				//search.searchOptions.QueryText = InChINet.InChIUtils.mol2inchiinfo(res.mol, InChINet.InChIFlags.Standard)[0];
				search.searchOptions.QueryText = InChINet.InChIUtils.mol2InChIKey(res.mol, InChINet.InChIFlags.Standard);
			}

			var rid = SearchUtility.RunSearchAsync(typeof(SimpleSearch), search.searchOptions, search.commonOptions, search.scopeOptions, search.resultOptions);

			return Ok(rid);
		}

		/// <summary>
		/// Run identical structure search
		/// </summary>
		/// <param name="search"></param>
		/// <returns>Request Id</returns>
		[Route("api/searches/exact")]
		[HttpPost]
		public IHttpActionResult ExactStructureSearch([FromBody] ExactStructureSearchModel search)
		{
			var rid = SearchUtility.RunSearchAsync(typeof(ExactStructureSearch), search.searchOptions, search.commonOptions, search.scopeOptions, search.resultOptions);

			return Ok(rid);
		}

		/// <summary>
		/// Run substructure search
		/// </summary>
		/// <param name="search"></param>
		/// <returns>Request Id</returns>
		[Route("api/searches/substructure")]
		[HttpPost]
		public IHttpActionResult SubstructureSearch([FromBody] SubstructureSearchModel search)
		{
			var rid = SearchUtility.RunSearchAsync(typeof(SubStructureSearch), search.searchOptions, search.commonOptions, search.scopeOptions, search.resultOptions);

			return Ok(rid);
		}

		/// <summary>
		/// Run structure similarity search
		/// </summary>
		/// <param name="search"></param>
		/// <returns>Request Id</returns>
		[Route("api/searches/similarity")]
		[HttpPost]
		public IHttpActionResult SimilarityStructureSearch([FromBody] SimilarityStructureSearchModel search)
		{
			var rid = SearchUtility.RunSearchAsync(typeof(SimilarityStructureSearch), search.searchOptions, search.commonOptions, search.scopeOptions, search.resultOptions);

			return Ok(rid);
		}
/*
		/// <summary>
		/// Run advanced search as a union of all other search types.
		/// </summary>
		/// <param name="search"></param>
		/// <returns>Request Id</returns>
		[Route("api/searches/advanced")]
		[HttpPost]
		public IHttpActionResult AdvancedSearch([FromBody] AdvancedSearchModel search)
		{
			return Ok(true);
		}
*/
		/// <summary>
		/// Returns the status of request
		/// </summary>
		/// <param name="rid">Request Id that was returned by the search procedure</param>
		/// <returns></returns>
		[Route("api/searches/status/{rid}")]
		[HttpGet]
		public IHttpActionResult SearchStatus(Guid rid)
		{
			var status = RequestManager.Current.GetStatus(rid);

			return Ok(status);
		}

		/// <summary>
		/// Get search results by request Id
		/// </summary>
		/// <param name="rid">Request Id that was returned by the search procedure</param>
		/// <param name="start"></param>
		/// <param name="count"></param>
		/// <returns>List of compound Ids</returns>
		[Route("api/searches/result/{rid}")]
		[HttpGet]
		public IHttpActionResult SearchResult(Guid rid, int start = 0, int count = 10)
		{
			var request = RequestManager.Current.GetRequest(rid);

			if (request == null)
				return NotFound();

			var results = RequestManager.Current.GetResults(rid, start, count);

			return Ok(results.Cast<SearchResult>().Select(r => r.Id));
		}

		/// <summary>
		/// Get search results with info by request Id
		/// </summary>
		/// <param name="rid"></param>
		/// <param name="start"></param>
		/// <param name="count"></param>
		/// <returns>List of compound Ids with some detailed information (similarity etc.)</returns>
		[Route("api/searches/result-with-info/{rid}")]
		[HttpGet]
		public IHttpActionResult SearchResultWithInfo(Guid rid, int start = 0, int count = 10)
		{
			var request = RequestManager.Current.GetRequest(rid);

			if (request == null)
				return NotFound();

			var results = RequestManager.Current.GetResults(rid, start, count).ToList();

			return Ok(results);
		}
		/*
				/// <summary>
				/// Get search results as records
				/// </summary>
				/// <param name="rid">Search request ID</param>
				/// <param name="start">Index where to start returning search results</param>
				/// <param name="count">Number of returned results</param>
				/// <returns>List of compounds</returns>
				[Route("api/searches/resultasrecords/{rid}")]
				[HttpGet]
				public IHttpActionResult SearchResultAsRecords(string rid, int start = 0, int count = 10)
				{
					return Ok(true);
				}
		*/
	}
}
