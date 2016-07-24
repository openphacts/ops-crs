using RSC.Logging;
using System.Collections.Generic;

namespace RSC.CVSP
{
	public class StandardizationResult
	{
		public string Standardized { get; set; }
		public IEnumerable<Issue> Issues { get; set; }
        public IEnumerable<Transformation> Transformations { get; set; }
    }
}
