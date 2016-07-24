using RSC.Logging;
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
	public class LoggerController : ApiController
    {
		private readonly ILogStore logStore;

		public LoggerController(ILogStore logStore)
		{
			if (logStore == null)
				throw new ArgumentNullException("logStore");

			this.logStore = logStore;
		}

		// GET api/logger/entrytypes
		/// <summary>
		/// Returns list of registered entry types
		/// </summary>
		/// <returns>List of entry types</returns>
		[Route("api/logger/{entrytypes}")]
		public IHttpActionResult GetEntryTypes()
		{
			return Ok(LogManager.Logger.EntryTypes);
		}

		// POST api/logger/entries
		/// <summary>
		/// Returns list of log entries by IDs
		/// </summary>
		/// <param name="id">List of internal GUIDs</param>
		/// <returns>List of log entries</returns>
		[Route("api/logger/entries")]
		[HttpPost]
		public IHttpActionResult GetEntriesPOST([FromBody] Guid[] id)
		{
			var res = logStore.GetLogEntries(id);

			return Ok(res);
		}

		// GET api/logger/entries?id={1}&id={2}
		/// <summary>
		/// Returns list of log entries by IDs
		/// </summary>
		/// <param name="id">List of internal GUIDs</param>
		/// <returns>List of log entries</returns>
		[Route("api/logger/entries")]
		[HttpGet]
		public IHttpActionResult GetEntriesGET([FromUri] Guid[] id)
		{
			var res = logStore.GetLogEntries(id);

			return Ok(res);
		}
	}
}
