using System.Collections.Generic;
using System.Web.Http;
using ChemSpider.Molecules;
using ChemSpider.Compounds;
using ChemSpider.Compounds.Database;

namespace OpenPHACTSWeb.WebAPI.Controllers
{
/*
	public class CompoundsController : ApiController
	{
		protected CompoundProvider compoundProvider = new CompoundProvider();

		// GET api/compounds
		/// <summary>
		/// Returns list of compounds
		/// </summary>
		/// <param name="start">Index where to start returning compounds</param>
		/// <param name="count">Number of returned compounds</param>
		/// <returns>List of compounds</returns>
		[Route("api/compounds")]
		public IHttpActionResult GetCompounds(int start = 0, int count = 10)
		{
			var compounds = compoundProvider.GetCompounds(start, count);

			return Ok(compounds);
		}

		// GET api/compoundsid
		/// <summary>
		/// Returns list of compounds' IDs
		/// </summary>
		/// <param name="start">Index where to start returning compounds</param>
		/// <param name="count">Number of returned compounds</param>
		/// <returns>List of compounds's IDs</returns>
		[Route("api/compoundsid")]
		public IHttpActionResult GetCompoundsID(int start = 0, int count = 10)
		{
			var ids = compoundProvider.GetCompoundsID(start, count);

			return Ok(ids);
		}

		// GET api/compounds/5
		/// <summary>
		/// Returns compound information by ID
		/// </summary>
		/// <param name="id">Internal compound ID</param>
		/// <param name="peer_id">Id of related compound that should be used to narrow the information</param>
		/// <returns>Compound object</returns>
		[Route("api/compounds/{id}")]
		public IHttpActionResult Get(int id, int? peer_id = null)
		{
			var compound = compoundProvider.GetCompound(id, peer_id);

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
		public IHttpActionResult GetList([FromUri] int[] id)
		{
			List<Compound> compounds = new List<Compound>();

			foreach (int i in id)
			{
				var c = compoundProvider.GetCompound(i);

				compounds.Add(c);
			}

			return Ok(compounds);
		}

		// GET api/compounds/5/fragments
		/// <summary>
		/// Returns list of compound fragments
		/// </summary>
		/// <param name="id">Internal compound ID</param>
		/// <returns>Array of compounds</returns>
		[AcceptVerbs("GET", "POST")]
		[Route("api/compounds/{id}/fragments")]
		public IHttpActionResult Fragments(int id)
		{
			IEnumerable<Compound> fragments = compoundProvider.GetFragments(id);

			return Ok(fragments);
		}

		// GET api/compounds/5/parents
		/// <summary>
		/// Returns list of compound parents
		/// </summary>
		/// <param name="id">Internal compound ID</param>
		/// <returns>List of compounds</returns>
		[AcceptVerbs("GET", "POST")]
		[Route("api/compounds/{id}/parents")]
		public IHttpActionResult Parents(int id, int? peer_id = null)
		{
			IEnumerable<Compound> parents = compoundProvider.GetParents(id);

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
		public IHttpActionResult Children(int id)
		{
			IEnumerable<Compound> children = compoundProvider.GetChildren(id);

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
		public IHttpActionResult Substances(int id)
		{
			IEnumerable<Substance> substances = compoundProvider.GetSubstances(id);

			return Ok(substances);
		}

		/// <summary>
		/// Convert Name, SMILES, InChI or internal compound ID into different formats
		/// </summary>
		/// <param name="convertOptions"></param>
		/// <returns></returns>
		[AcceptVerbs("GET")]
		[Route("api/compounds/ConvertTo")]
		public IHttpActionResult ConvertTo([FromUri] ChemIdUtils.ConvertOptions convertOptions)
		{
			ChemSpider.Parts.Client.JSONClient parts = new ChemSpider.Parts.Client.JSONClient() { RedirectCookies = false };

			var res = parts.ConvertTo(convertOptions);

			return Ok(res);
		}

		/// <summary>
		/// Returns MOL file for the specified compound
		/// </summary>
		/// <param name="id">Internal compound ID</param>
		/// <param name="dimension"></param>
		/// <param name="calc"></param>
		/// <param name="striph"></param>
		/// <returns>MOL file</returns>
		[AcceptVerbs("GET")]
		[Route("api/compounds/{id}/mol")]
		public IHttpActionResult GetMol(int id, string dimension = "2d")
		{
			Compound compound = compoundProvider.GetCompound(id);

			string mol = string.Empty;

			if (dimension == "3d")
			{
				mol = ExtUtilsNet.Balloon.opt3d(compound.MOL);

				if (string.IsNullOrEmpty(mol))
					mol = compound.MOL;
			}
			else
			{
				mol = compound.MOL;
			}

			return Ok(mol);
		}
	}
*/
}