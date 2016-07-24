using RSC.Compounds.NMRFeatures;
using ChemSpider.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RSC.Compounds.NMRFeatures.WebAPI
{
	public class NMRController : ApiController
	{
		private readonly INMRFeaturesService service;

		public NMRController(INMRFeaturesService service)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}

			this.service = service;
		}

		// GET api/nmr/features/all
		/// <summary>
		/// Returns list of all NMR features registered in the system
		/// </summary>
		/// <returns>List of NMR features</returns>
		[Route("api/nmrfeatures")]
		public IHttpActionResult GetNMRFeatures()
		{
			var features = service.GetAllFeatures();

			return Ok(features);
		}
	}
}
