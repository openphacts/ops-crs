using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ChemSpider.Database;
using RSC.Compounds.Old;

namespace RSC.Compounds.Database
{
	public class SQLDatasourcesStore : IDatasourcesStore
	{
		/// <summary>
		/// Returns total number of datasources registered in the system
		/// </summary>
		/// <returns></returns>
		public int GetDatasourcesCount()
		{
			int count = 0;

			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				count = conn.ExecuteScalar<int>("select count(distinct dsn_guid) from substances");
			}

			return count;
		}

		/// <summary>
		/// Returns data sources' IDs page by page
		/// </summary>
		/// <param name="start">Index where to start returning data sources from</param>
		/// <param name="coun">Number of returned data sources</param>
		/// <returns>List of data sources' IDs</returns>
		public IEnumerable<Guid> GetDataSourcesID(int start = 0, int count = 0)
		{
			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				//	by default we return all data sources...
				if(start == 0 && count == 0)
					return conn.FetchColumn<Guid>(@"SELECT distinct dsn_guid FROM substances");
				//	... else we return data sources page by page
				else
					return conn.FetchColumn<Guid>(@"with cte as (
														SELECT dsn_guid, ROW_NUMBER() OVER(ORDER BY dsn_guid) as rn
														FROM (SELECT distinct dsn_guid FROM substances) t
													)
													SELECT dsn_guid FROM cte 
													WHERE rn BETWEEN @start and @start + @count - 1", new { start = start + 1, count = count });
			}
		}

		/// <summary>
		/// Returns compounds' count for the specific datasource
		/// </summary>
		/// <param name="id">Datasource GUID</param>
		/// <returns>Number of compounds</returns>
		public int GetCompoundsCount(Guid id)
		{
			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				return conn.ExecuteScalar<int>(@"select count(distinct csid)
												from substances s inner join sid_csid sc on s.sid = sc.sid
												where s.dsn_guid = @guid", new { @guid = id });
			}
		}

		/// <summary>
		/// Returns compounds' IDs for the specific datasource page by page
		/// </summary>
		/// <param name="id">Datasource GUID</param>
		/// <param name="start">Index where to start returning compounds from</param>
		/// <param name="coun">Number of returned compounds</param>
		/// <returns>List of compounds' IDs</returns>
		public IEnumerable<int> GetCompoundsID(Guid id, int start = 0, int count = 10)
		{
			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				return conn.FetchColumn<int>(@"with cte as (
													SELECT csid, ROW_NUMBER() OVER(ORDER BY csid) as rn
													FROM (
														select distinct csid
														from substances s inner join sid_csid sc on s.sid = sc.sid
														where s.dsn_guid = @guid
													) t
												)
												SELECT csid FROM cte 
												WHERE rn BETWEEN @start and @start + @count - 1", new { guid = id, start = start + 1, count = count });
			}
		}
	}
}
