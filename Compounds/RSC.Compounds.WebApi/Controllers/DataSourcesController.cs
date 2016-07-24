using System;
using System.Web.Http;
using System.Web.Http.Cors;

namespace RSC.Compounds.WebApi
{
	[EnableCors("*", "*", "*")]
	public class DataSourcesController : ApiController
	{
		private readonly IDatasourceStore datasources;

		public DataSourcesController(IDatasourceStore datasources)
		{
			if (datasources == null)
			{
				throw new ArgumentNullException("datasources");
			}

			this.datasources = datasources;
		}
		
		// GET api/datasources
		/// <summary>
		/// Returns list of datasources GUIDs registered in the system
		/// </summary>
		/// <param name="start">Index where to start returning data sources</param>
		/// <param name="count">Number of returned data sources</param>
		/// <returns>List of data sources GUIDs</returns>
		[Route("api/datasources")]
		public IHttpActionResult GetDataSources(int start = 0, int count = 0)
		{
			var res = datasources.GetDataSourceIds(start, count);

			return Ok(res);
		}

		// GET api/datasources/1d57a6a9-d574-41fb-aaaa-c930d1e26c99/compounds/count
		/// <summary>
		/// Returns compounds' count for the specific datasource
		/// </summary>
		/// <param name="id">Datasource GUID</param>
		/// <returns>Number of compounds</returns>
		[Route("api/datasources/{id}/compounds/count")]
		public IHttpActionResult GetCompoundsCount(Guid id)
		{
			var res = datasources.GetCompoundsCount(id);

			return Ok(res);
		}

		// GET api/datasources/1d57a6a9-d574-41fb-aaaa-c930d1e26c99/compounds
		/// <summary>
		/// Returns list of compounds' IDs for the specific datasource
		/// </summary>
		/// <param name="id">Datasource GUID</param>
		/// <param name="start">Index where to start returning compounds</param>
		/// <param name="count">Number of returned compounds</param>
		/// <returns>List of compounds's IDs</returns>
		[Route("api/datasources/{id}/compounds")]
		public IHttpActionResult GetCompoundsID(Guid id, int start = 0, int count = 10)
		{
			var ids = datasources.GetCompoundIds(id, start, count);

			return Ok(ids);
		}
	}
}
