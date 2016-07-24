using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.EntityFramework
{
	public static class ProcessingParameterExtensions
	{
		public static ProcessingParameter ToProcessingParameter(this ef_ProcessingParameter ef)
		{
			return new ProcessingParameter()
			{
				Name = ef.Name,
				Value = ef.Value
			};
		}
	}
}
