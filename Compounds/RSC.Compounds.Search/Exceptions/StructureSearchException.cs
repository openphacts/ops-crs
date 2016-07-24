using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.Search
{
	public class StructureSearchException : RSCException
	{
		public StructureSearchException(string message)
			: base(message)
		{
		}

		public StructureSearchException(string format, params object[] args)
			: base(format, args)
		{
		}
	}
}
