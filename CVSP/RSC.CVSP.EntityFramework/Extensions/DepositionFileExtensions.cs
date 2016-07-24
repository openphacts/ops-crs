using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.EntityFramework
{
	public static class DepositionFileExtensions
	{
		public static DepositionFile ToDepositionFile(this ef_DepositionFile ef)
		{
			return new DepositionFile()
			{
				Id = ef.Guid,
				Name = ef.Name
			};
		}
	}
}
