using RSC.Logging;
using System.Collections.Generic;

namespace RSC.CVSP
{
	public interface IValidation
	{
		IEnumerable<Issue> Validate(string data, IDictionary<string,object> options=null);
	}
}
