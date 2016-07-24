using RSC.CVSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CVSPWeb.WebAPI
{
	[EnableCors("*", "*", "*")]
	public class RecordsController : ApiController
    {
		private readonly ICVSPStore repository;

		public RecordsController(ICVSPStore repository)
		{
			if (repository == null)
				throw new ArgumentNullException("repository");

			this.repository = repository;
		}

		// GET api/records/851107d4-ec5d-4cdf-903b-8ed94aa4132d
		/// <summary>
		/// Returns record by GUID
		/// </summary>
		/// <param name="guid">Internal record GUID</param>
		/// <returns>Record object</returns>
		[Route("api/records/{guid}")]
		public IHttpActionResult GetRecord(Guid guid)
		{
			var res = repository.GetRecord(guid);

			return Ok(res);
		}

		// GET api/records/851107d4-ec5d-4cdf-903b-8ed94aa4132d/json
		/// <summary>
		/// Returns record's JSON by GUID
		/// </summary>
		/// <param name="guid">Internal record GUID</param>
		/// <returns>Record's JSON representation</returns>
		[Route("api/records/{guid}/json")]
		public HttpResponseMessage GetRecordJson(Guid guid)
		{
			var record = repository.GetRecord(guid);

			HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(record.ToJson()),
			};

			result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = string.Format("{0}.json", guid) };

			return result;
		}

		// GET api/records/851107d4-ec5d-4cdf-903b-8ed94aa4132d/issues
		/// <summary>
		/// Returns record's issues
		/// </summary>
		/// <param name="guid">Internal record GUID</param>
		/// <returns>List of record's issues</returns>
		[Route("api/records/{guid}/issues")]
		public IHttpActionResult GetRecordIssues(Guid guid)
		{
			var res = repository.GetRecordIssues(guid);

			return Ok(res);
		}

		// GET api/records/851107d4-ec5d-4cdf-903b-8ed94aa4132d/fields
		/// <summary>
		/// Returns record's fields
		/// </summary>
		/// <param name="guid">Internal record GUID</param>
		/// <returns>List of record's fields</returns>
		[Route("api/records/{guid}/fields")]
		public IHttpActionResult GetRecordFields(Guid guid)
		{
			var res = repository.GetRecordFields(guid);

			return Ok(res);
		}

		// GET api/records/851107d4-ec5d-4cdf-903b-8ed94aa4132d/properties
		/// <summary>
		/// Returns record's properties
		/// </summary>
		/// <param name="guid">Internal record GUID</param>
		/// <returns>List of record's properties</returns>
		[Route("api/records/{guid}/properties")]
		public IHttpActionResult GetRecordProperties(Guid guid)
		{
			var res = repository.GetRecordProperties(guid);

			return Ok(res);
		}

		// GET api/records/list?id={1}&id={2}
		/// <summary>
		/// Returns list of records by array of GUIDs
		/// </summary>
		/// <param name="guid">List of internal GUIDs</param>
		/// <returns>List of records</returns>
		[Route("api/records/list")]
		[HttpPost]
		public IHttpActionResult GetRecordsPOST([FromBody] Guid[] id, [FromUri] string filter = null)
		{
			var res = repository.GetRecords(id, filter == null ? null : filter.Split('|'));

			return Ok(res);
		}

		// GET api/records/list?id={1}&id={2}
		/// <summary>
		/// Returns list of records by array of GUIDs
		/// </summary>
		/// <param name="guid">List of internal GUIDs</param>
		/// <returns>List of records</returns>
		[Route("api/records/list")]
		[HttpGet]
		public IHttpActionResult GetRecordsGET([FromUri] Guid[] id, [FromUri] string filter = null)
		{
			var res = repository.GetRecords(id, filter == null ? null : filter.Split('|'));

			return Ok(res);
		}
	}
}
