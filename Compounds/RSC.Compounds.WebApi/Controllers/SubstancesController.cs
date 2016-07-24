using System.Collections.Generic;
using System.Web.Http;
using ChemSpider.ObjectModel;
using System;
using System.Web.Http.Cors;

namespace RSC.Compounds.WebApi
{
	[EnableCors("*", "*", "*")]
	public class SubstancesController : ApiController
	{
		private readonly SubstanceStore substanceStore;

		public SubstancesController(SubstanceStore substanceStore)
		{
			if (substanceStore == null)
			{
				throw new ArgumentNullException("substanceStore");
			}

			this.substanceStore = substanceStore;
		}

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
			var substances = substanceStore.GetSubstances(start, count);

			return Ok(substances);
		}

		// GET api/substances/5
		/// <summary>
		/// Returns substance information by ID
		/// </summary>
		/// <param name="id">Internal substance ID</param>
		/// <returns>Substance object</returns>
		[Route("api/substances/{id}")]
		public IHttpActionResult Get(Guid id)
		{
			var substance = substanceStore.GetSubstance(id);

			if (substance == null)
				return NotFound();

			return Ok(substance);
		}

		// GET api/substances/list?id=1&id=2
		/// <summary>
		/// Returns array of substances by array of IDs
		/// </summary>
		/// <param name="id">Array of internal substance IDs</param>
		/// <returns>Array of substances</returns>
		[Route("api/substances/list")]
		[HttpGet]
		public IHttpActionResult GetList([FromUri] Guid[] id)
		{
			var substances = substanceStore.GetSubstances(id);

			return Ok(substances);
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
			//IEnumerable<Issue> issues = substancesService.GetIssues(id);

			//return Ok(issues);

			return Ok(new List<Issue>());
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
			//IEnumerable<string> synonyms = substancesService.GetSynonyms(id);
			//return Ok(synonyms);

			return Ok(new List<string>());
		}
	}
}
