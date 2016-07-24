using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace RSC.CVSP
{
	[Serializable]
	[DataContract]
	public class ProcessingParameter
	{
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string Value { get; set; }
	}

	public static class ProcessingParametersExtensions
	{
		public static bool Has(this IEnumerable<ProcessingParameter> parameters, string name)
		{
			return parameters.Where(p => p.Name.Equals(name)).Any();
		}

		public static ProcessingParameter GetParameter(this IEnumerable<ProcessingParameter> parameters, string name)
		{
			return parameters.Where(p => p.Name.Equals(name)).FirstOrDefault();
		}

		public static bool AsBool(this IEnumerable<ProcessingParameter> parameters, string name)
		{
			if (!parameters.Has(name))
				return false;

			var parameter = parameters.GetParameter(name);

			return Convert.ToBoolean(parameter.Value);
		}
	}
}
