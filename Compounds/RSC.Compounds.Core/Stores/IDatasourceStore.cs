using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds
{
	public interface IDatasourceStore
	{
		/// <summary>
		/// Returns data source Ids page by page
		/// </summary>
		/// <param name="start">Index where to start returning data sources from</param>
		/// <param name="coun">Number of returned data sources</param>
		/// <returns>List of data source Ids</returns>
		IEnumerable<Guid> GetDataSourceIds(int start = 0, int count = -1);

		/// <summary>
		/// Returns compounds count for the specific datasource
		/// </summary>
		/// <param name="datasourceId">Datasource Id</param>
		/// <returns>Number of compounds</returns>
		int GetCompoundsCount(Guid datasourceId);

		/// <summary>
		/// Returns compound Ids for the specific datasource page by page
		/// </summary>
		/// <param name="datasourceId">Datasource Id</param>
		/// <param name="start">Index where to start returning compounds from</param>
		/// <param name="coun">Number of returned compounds</param>
		/// <returns>List of compound Ids</returns>
		IEnumerable<Guid> GetCompoundIds(Guid datasourceId, int start = 0, int count = -1);
	}
}
