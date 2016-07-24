using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.EntityFramework
{
	public static class SubstanceExtensions
	{
		public static Substance ToSubstance(this ef_Substance ef)
		{
			return new Substance()
			{
				Id = ef.Id,
				DataSourceId = ef.DataSourceId,
				ExternalIdentifier = ef.ExternalIdentifier
			};
		}
	}
}
