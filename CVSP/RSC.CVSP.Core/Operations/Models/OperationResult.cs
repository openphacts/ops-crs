using RSC.Logging;
using System.Collections.Generic;

namespace RSC.CVSP
{
	public class OperationResult
	{
		public OperationResult()
		{
			Issues = new List<Issue>();
		}
		public object Result { get; set; }
		public IEnumerable<Issue> Issues { get; set; }
	}
}
