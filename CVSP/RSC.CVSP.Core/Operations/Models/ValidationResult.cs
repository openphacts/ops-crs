using RSC.Logging;
using System.Collections.Generic;

namespace RSC.CVSP
{
	public class ValidationResult
	{
		public IEnumerable<Issue> Issues { get; set; }
	}
}
