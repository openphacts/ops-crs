using RSC.CVSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
//using System.Web.Http.Cors;

namespace RSC.CVSP.Compounds
{
	//[EnableCors("*", "*", "*")]
	public class ParenChildController : ApiController
	{
		private readonly ICVSPStore repository;

		public ParenChildController(ICVSPStore repository)
		{
			if (repository == null)
				throw new ArgumentNullException("repository");

			this.repository = repository;
		}

		// GET api/records/851107d4-ec5d-4cdf-903b-8ed94aa4132d/parents
		/// <summary>
		/// Returns record's parents by GUID
		/// </summary>
		/// <param name="guid">Internal record GUID</param>
		/// <returns>List of record parents</returns>
		[Route("api/records/{guid}/parents")]
		public IHttpActionResult GetRecordParents(Guid guid)
		{
			var record = repository.GetRecord(guid) as CompoundRecord;

			if (record == null)
				return NotFound();

			return Ok(record.GetParents());
		}
	}
}
