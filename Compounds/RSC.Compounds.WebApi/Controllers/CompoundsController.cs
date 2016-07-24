using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Cors;
using System;
using System.Linq;

using RSC.Properties;

namespace RSC.Compounds.WebApi
{
	[EnableCors("*", "*", "*")]
	public class CompoundsController : ApiController
	{
		private readonly CompoundStore compoundStoreOld;
		private readonly IPropertyStore propertyStore;
		private readonly ICompoundStore compoundStore;

		public CompoundsController(CompoundStore compoundStoreOld, IPropertyStore propertyStore, ICompoundStore compoundStore)
		{
			if (compoundStoreOld == null)
			{
				throw new ArgumentNullException("compoundStoreOld");
			}

			if (compoundStore == null)
			{
				throw new ArgumentNullException("compoundStore");
			}

			if (propertyStore == null)
			{
				throw new ArgumentNullException("propertyStore");
			}

			this.compoundStoreOld = compoundStoreOld;
			this.compoundStore = compoundStore;
			this.propertyStore = propertyStore;
		}

		// GET api/compounds/count
		/// <summary>
		/// Returns total number of compounds registered in the system
		/// </summary>
		/// <returns>Total number of compounds</returns>
		[Route("api/compounds/count")]
		public IHttpActionResult GetCompoundsCount()
		{
			var count = compoundStoreOld.GetCompoundsCount();

			return Ok(count);
		}

		// GET api/compoundsid
		/// <summary>
		/// Returns list of compounds' IDs
		/// </summary>
		/// <param name="start">Index where to start returning compounds</param>
		/// <param name="count">Number of returned compounds</param>
		/// <returns>List of compounds's IDs</returns>
		[Route("api/compounds/ids")]
		public IHttpActionResult GetCompoundsIDs(int start = 0, int count = -1)
		{
			var ids = compoundStoreOld.GetCompoundIds(start, count);

			return Ok(ids);
		}

		// GET api/compounds/5
		/// <summary>
		/// Returns compound information by ID
		/// </summary>
		/// <param name="id">Internal compound ID</param>
		/// <returns>Compound object</returns>
		[Route("api/compounds/{id}")]
		public IHttpActionResult Get(Guid id)
		{
			var compound = compoundStoreOld.GetCompound(id);

			if (compound == null)
				return NotFound();

			return Ok(compound);
		}

		// GET api/compounds/list?id=1&id=2
		/// <summary>
		/// Returns array of compounds by array of IDs
		/// </summary>
		/// <param name="id">Array of internal compound IDs</param>
		/// <returns>Array of compounds</returns>
		[Route("api/compounds/list")]
		[HttpGet]
		public IHttpActionResult GetListGET([FromUri] Guid[] id, [FromUri] string filter = null)
		{
			var compounds = compoundStore.GetCompounds(id, filter == null ? null : filter.Split('|'));

			return Ok(compounds);
		}

		// POST api/records/list
		/// <summary>
		/// Returns list of compounds by array of IDs
		/// </summary>
		/// <param name="id">List of internal compound IDs</param>
		/// <returns>List of compounds</returns>
		[Route("api/compounds/list")]
		[HttpPost]
		public IHttpActionResult GetListPOST([FromBody] Guid[] id, [FromUri] string filter = null)
		{
			var res = compoundStore.GetCompounds(id, filter == null ? null : filter.Split('|'));

			return Ok(res);
		}

		// GET api/compounds/5/parents
		/// <summary>
		/// Returns list of compound parents
		/// </summary>
		/// <param name="id">Internal compound ID</param>
		/// <returns>List of compounds</returns>
		[AcceptVerbs("GET", "POST")]
		[Route("api/compounds/{id}/parents")]
		public IHttpActionResult Parents(Guid id)
		{
			var parents = compoundStore.GetParents(id);

			return Ok(parents);
		}

		// GET api/compounds/5/children
		/// <summary>
		/// Returns list of compound children
		/// </summary>
		/// <param name="id">Internal compound ID</param>
		/// <returns>List of compounds</returns>
		[AcceptVerbs("GET", "POST")]
		[Route("api/compounds/{id}/children")]
		public IHttpActionResult Children(Guid id)
		{
			var children = compoundStore.GetChildren(id);

			return Ok(children);
		}

		// GET api/compounds/5/substances
		/// <summary>
		/// Returns list of substances where this compound came from
		/// </summary>
		/// <param name="id">Internal compound ID</param>
		/// <returns>List of substances</returns>
		[AcceptVerbs("GET", "POST")]
		[Route("api/compounds/{id}/substances")]
		public IHttpActionResult Substances(Guid id)
		{
			var substances = compoundStoreOld.GetSubstanceIds(id);

			return Ok(substances);
		}

		// GET api/compounds/5/properties
		/// <summary>
		/// Returns list of compound's properties
		/// </summary>
		/// <param name="compoundId">Compound ID</param>
		/// <param name="start">Index where to start returning properties</param>
		/// <param name="count">Number of returned properties</param>
		/// <returns>List of properties</returns>
		[AcceptVerbs("GET", "POST")]
		[Route("api/compounds/{id}/properties")]
		public IHttpActionResult GetProperties(Guid id, int start = 0, int count = -1)
		{
			var propertyIds = compoundStore.GetCompoundProperties(id);

			if (propertyIds == null)
				return Ok(new Property[] { });

			if (start > 0)
				propertyIds = propertyIds.Skip(start);

			if (count > 0)
				propertyIds = propertyIds.Take(count);

			var properties = propertyStore.GetProperties(propertyIds);

			return Ok(properties);
		}

		// GET api/compounds/c4eb4deb-36a5-4864-9b0e-7aeb4e7a994f/synonyms
		/// <summary>
		/// Returns list of compound's synonyms
		/// </summary>
		/// <param name="compoundId">Compound Id</param>
		/// <param name="start">Index where to start returning synonyms</param>
		/// <param name="count">Number of returned synonyms</param>
		/// <returns>List of synonyms</returns>
		[AcceptVerbs("GET", "POST")]
		[Route("api/compounds/{id}/synonyms")]
		public IHttpActionResult GetSynonyms(Guid id, int start = 0, int count = -1)
		{
			var synonyms = compoundStore.GetCompoundSynonyms(id);

			if (synonyms == null)
				return Ok(new Synonym[] { });

			if (start > 0)
				synonyms = synonyms.Skip(start);

			if (count > 0)
				synonyms = synonyms.Take(count);

			return Ok(synonyms);
		}

		// GET api/compounds/5/datasources/count
		/// <summary>
		/// Returns number of datasources where this compound's registered
		/// </summary>
		/// <param name="id">Internal compound ID</param>
		/// <returns>Number of datasources where this compound's registered</returns>
		[AcceptVerbs("GET", "POST")]
		[Route("api/compounds/{id}/datasources/count")]
		public IHttpActionResult Datasources(Guid id)
		{
			var count = compoundStoreOld.GetDatasourcesCount(id);

			return Ok(count);
		}

		// GET api/compounds/5/datasources
		/// <summary>
		/// Returns list of datasources where this compound came from
		/// </summary>
		/// <param name="id">Internal compound ID</param>
		/// <param name="start">Index where to start returning datasources</param>
		/// <param name="count">Number of returned datasources</param>
		/// <returns>List of datasources' GUIDs</returns>
		[AcceptVerbs("GET", "POST")]
		[Route("api/compounds/{id}/datasources")]
		public IHttpActionResult Datasources(Guid id, int start = 0, int count = -1)
		{
			IEnumerable<Guid> guids = compoundStoreOld.GetDatasourceIds(id);

			return Ok(guids);
		}

		// GET api/compounds/5/similarities
		/// <summary>
		/// Returns list of similar compounds
		/// </summary>
		/// <param name="id">Internal compound ID</param>
		/// <param name="threshold">The lower limit of the desired similarity</param>
		/// <returns>List of compounds</returns>
		//[AcceptVerbs("GET", "POST")]
		//[Route("api/compounds/{id}/similarities")]
		//public IHttpActionResult Similarities(int id, double threshold = 0.95)
		//{
		//	IEnumerable<Similarity> similarities = compoundsService.GetSimilarities(id, threshold);

		//	return Ok(similarities);
		//}

		/// <summary>
		/// Returns MOL file for the specified compound
		/// </summary>
		/// <param name="id">Internal compound ID</param>
		/// <param name="dimension"></param>
		/// <returns>MOL file</returns>
		[AcceptVerbs("GET")]
		[Route("api/compounds/{id}/mol")]
		public HttpResponseMessage GetMol(Guid id, string dimension = "2d")
		{
			var compound = compoundStoreOld.GetCompound(id);

			string mol = string.Empty;

			if (dimension == "3d")
			{
				mol = ExtUtilsNet.Balloon.opt3d(compound.Mol);

				if (string.IsNullOrEmpty(mol))
					mol = compound.Mol;
			}
			else
			{
				mol = compound.Mol;
			}

			if (string.IsNullOrEmpty(mol))
				return new HttpResponseMessage(HttpStatusCode.NotFound);

			HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(mol),
			};

			result.Content.Headers.ContentType = new MediaTypeHeaderValue("chemical/x-mdl-molfile");
			result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = string.Format("{0}.mol", id) };

			return result;
		}
	}
}