using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.EntityFramework
{
	public class EFDatasourceStore : IDatasourceStore
	{
		/// <summary>
		/// Returns data source Ids page by page
		/// </summary>
		/// <param name="start">Index where to start returning data sources from</param>
		/// <param name="coun">Number of returned data sources</param>
		/// <returns>List of data source Ids</returns>
		public IEnumerable<Guid> GetDataSourceIds(int start = 0, int count = -1)
		{
			using (var db = new CompoundsContext())
			{
				//	Empty request...
				if (count == 0)
					return new List<Guid>();

				var query = db.Substances
					.Select(s => s.DataSourceId)
					.Distinct();

				// TODO: Skip/Take makes no sense without sorting

				if (start > 0)
					query = query.Skip(start);

				if (count > 0)
					query = query.Take(count);

				return query.ToList();
			}
		}

		/// <summary>
		/// Returns compounds count for the specific datasource
		/// </summary>
		/// <param name="datasourceId">Datasource Id</param>
		/// <returns>Number of compounds</returns>
		public int GetCompoundsCount(Guid datasourceId)
		{
			using (var db = new CompoundsContext())
			{
				return db.Substances.Where(s => s.DataSourceId == datasourceId)
					.Join(db.Revisions, s => s.Id, sr => sr.SubstanceId, (s, sr) => new { sr.SubstanceId, sr.CompoundId, sr.Version })
					.OrderBy(s => new { s.SubstanceId, s.Version })
					.GroupBy(s => s.SubstanceId)
					.Select(g => new { SubstanceId = g.Key, CompoundId = g.FirstOrDefault().CompoundId, Version = g.FirstOrDefault().Version })
					.Select(s => s.CompoundId)
					.Distinct()
					.Count();
			}
		}

		/// <summary>
		/// Returns compound Ids for the specific datasource page by page
		/// </summary>
		/// <param name="datasourceId">Datasource Id</param>
		/// <param name="start">Index where to start returning compounds from</param>
		/// <param name="coun">Number of returned compounds</param>
		/// <returns>List of compound Ids</returns>
		public IEnumerable<Guid> GetCompoundIds(Guid datasourceId, int start = 0, int count = -1)
		{
			using (var db = new CompoundsContext())
			{
				var query = db.Substances.Where(s => s.DataSourceId == datasourceId)
					.Join(db.Revisions, s => s.Id, sr => sr.SubstanceId, (s, sr) => new { sr.SubstanceId, sr.CompoundId, sr.DateCreated, sr.Version })
					.OrderByDescending(s => new { s.SubstanceId, s.Version })
					.GroupBy(s => s.SubstanceId)
					.Select(g => new { SubstanceId = g.Key, CompoundId = g.FirstOrDefault().CompoundId, DateCreated = g.FirstOrDefault().DateCreated, Version = g.FirstOrDefault().Version })
					.OrderBy(s => s.DateCreated)
					.Select(s => s.CompoundId)
					.Distinct();

				if (start > 0)
					query = query.Skip(start);

				if (count > 0)
					query = query.Take(count);

				return query.ToList();
			}
		}
	}
}
