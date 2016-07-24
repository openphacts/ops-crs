using RSC.Compounds.NMRFeatures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace RSC.Compounds.NMRFeatures.WebAPI
{
	[KnownType(typeof(CompoundNMRFeature))]
	public class NMRCompoundsController : ApiController
	{
		private readonly INMRFeaturesService service;

		public NMRCompoundsController(INMRFeaturesService service)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}

			this.service = service;
		}

		// GET api/compounds/5/nmrfeatures
		/// <summary>
		/// Returns list of compound fragments
		/// </summary>
		/// <param name="id">Internal compound ID</param>
		/// <returns>Array of compounds</returns>
		[AcceptVerbs("GET", "POST")]
		[Route("api/compounds/{id}/nmrfeatures")]
		public IHttpActionResult NMRFeatures(int id)
		{
			var features = service.GetCompoundFeatures(id).ToList();

			return Ok(features);
		}
	}
}
