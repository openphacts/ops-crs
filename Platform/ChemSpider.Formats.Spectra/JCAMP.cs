using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jspecview.source;

namespace ChemSpider.Formats.Spectra
{
	public class JCAMP
	{
		public static bool CheckJCAMPFormat(string content, out string error)
		{
			try {
				error = JDXSource.checkJDXSource(content);
			}
			catch ( Exception ex ) {
				error = ex.Message;
			}

			return string.IsNullOrEmpty(error);
		}
	}
}
