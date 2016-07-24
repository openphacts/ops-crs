using RSC.Compounds.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.EntityFramework
{
	public static class ScopeExtensions
	{
		public static IQueryable<ef_Compound> SearchScope(this IQueryable<ef_Compound> query, CompoundsSearchScopeOptions scope, CompoundsContext db)
		{
			if (scope != null && !scope.IsEmpty())
			{
				if (scope.Datasources != null && scope.Datasources.Any())	//	if we want to limit search results by the list of specific datasources...
				{
					query = query
						.Join(db.Revisions, c => c.Id, r => r.CompoundId, (c, r) => new { c, r })
						.Join(db.Substances, cr => cr.r.SubstanceId, s => s.Id, (cr, s) => new { s, cr })
						.Where(scr => scope.Datasources.Contains(scr.s.DataSourceId))
						.Select(scr => scr.cr.c);
				}
				else if (scope.RealOnly)	//	... or if we want to search through the real (not virtual) compounds only...
				{
					query = query
						.Join(db.Revisions, c => c.Id, r => r.CompoundId, (c, r) => new { c, r })
						.Select(cr => cr.c);
				}
			}

			return query;
		}
	}
}
