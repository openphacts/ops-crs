using RSC.Properties;
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
	public class PropertiesController : ApiController
    {
		// GET api/properties/units
		/// <summary>
		/// Returns list of registered units in the system
		/// </summary>
		/// <returns>List of units</returns>
		[Route("api/properties/units")]
		public IHttpActionResult GetUnitsList()
		{
			return Ok(PropertyManager.Current.UnitsList());
		}

		// GET api/properties/definitions
		/// <summary>
		/// Returns list of registered property definitions in the system
		/// </summary>
		/// <returns>List of property definitions</returns>
		[Route("api/properties/definitions")]
		public IHttpActionResult GetPropertyDefinitionsList()
		{
			return Ok(PropertyManager.Current.PropertyDefinitionsList());
		}

		// GET api/properties/software
		/// <summary>
		/// Returns list of software registered in the system
		/// </summary>
		/// <returns>List of software</returns>
		[Route("api/properties/software")]
		public IHttpActionResult GetSoftwareList()
		{
			return Ok(PropertyManager.Current.SoftwareList());
		}

		// GET api/properties/provenances
		/// <summary>
		/// Returns list of provenances registered in the system
		/// </summary>
		/// <returns>List of provenances</returns>
		[Route("api/properties/provenances")]
		public IHttpActionResult GetProvenancesList()
		{
			return Ok(PropertyManager.Current.ProvenancesList());
		}

		// GET api/properties/provenances/48868D82-0A9C-4D49-9994-1C4A33808B9F
		/// <summary>
		/// Returns provenance details
		/// </summary>
		/// <returns>Provenance instance</returns>
		[Route("api/properties/provenances/{id}")]
		public IHttpActionResult GetProvenance(Guid id)
		{
			return Ok(PropertyManager.Current.GetProvenance(id));
		}

		// POST api/properties/list
		/// <summary>
		/// Returns list of properties by IDs
		/// </summary>
		/// <param name="id">List of internal GUIDs</param>
		/// <returns>List of properties</returns>
		[Route("api/properties/list")]
		[HttpPost]
		public IHttpActionResult GetPropertiesPOST([FromBody] Guid[] id)
		{
			var res = PropertyManager.Current.GetProperties(id);

			return Ok(res);
		}

		// GET api/properties/list?id={1}&id={2}
		/// <summary>
		/// Returns list of properties by IDs
		/// </summary>
		/// <param name="id">List of properties GUIDs</param>
		/// <returns>List of properties</returns>
		[Route("api/properties/list")]
		[HttpGet]
		public IHttpActionResult GetPropertiesGET([FromUri] Guid[] id)
		{
			var res = PropertyManager.Current.GetProperties(id);

			return Ok(res);
		}
	}
}
