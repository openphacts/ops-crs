using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.DataExport
{
	public class CompoundsExportException : RSCException
	{
		public CompoundsExportException() :
			base()
		{

		}

		public CompoundsExportException(string message)
			: base(message)
		{

		}

		public CompoundsExportException(string format, params object[] args)
			: base(format, args)
		{

		}
	}
}
