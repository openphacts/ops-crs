using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds
{
	public interface IStatistics
	{
		/// <summary>
		/// Returns general statistics information about the system
		/// </summary>
		/// <returns></returns>
		Statistics GetGeneralStatistics();
	}
}
