using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Process
{
	public class ChunksStatistics
	{
		public int Total { get; set; }

		public int New { get; set; }

		public int Processing { get; set; }

		public int Processed { get; set; }

		public int Failed { get; set; }
	}
}
