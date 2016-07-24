using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.EntityFramework
{
	public class EFStatistics : IStatistics
	{
		/// <summary>
		/// Returns general statistics information about the system
		/// </summary>
		/// <returns></returns>
		public Statistics GetGeneralStatistics()
		{
			using (var db = new CompoundsContext())
			{
				return new Statistics()
				{
					CompoundsNumber = db.Compounds.Count(),
					DatasourcesNumber = db.Substances.Select(s => s.DataSourceId).Distinct().Count()
				};
			}
		}
	}
}
