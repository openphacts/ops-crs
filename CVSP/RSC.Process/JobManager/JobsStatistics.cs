using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Process
{
	public class JobsStatistics
	{
		public int Total { get; set; }

		public int New { get; set; }

		public int Processing { get; set; }

		public int Processed { get; set; }

		public int Failed { get; set; }

		public int Delayed { get; set; }

		public IEnumerable<string> Commands { get; set; }

		public TimeSpan? ProcessingTime { get; set; }

		public IDictionary<string, long> Stopwatches { get; set; }
	}
}
