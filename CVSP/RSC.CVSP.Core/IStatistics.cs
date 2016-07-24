using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{
	public interface IStatistics
	{
		/// <summary>
		/// takes deposition guid and returns a statistic information about deposition
		/// </summary>
		/// <param name="depositionId">Deposition guid</param>
		/// <returns>Deposition's statistic object</returns>
		Statistic GetDepositionStats(Guid depositionId);
	}
}
