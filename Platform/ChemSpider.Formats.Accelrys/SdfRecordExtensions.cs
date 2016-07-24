using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChemSpider.Molecules;

namespace ChemSpider.Formats.Accelrys
{
	public static class SdfRecordExtensions
	{
		public static string GetConcatFields(this SdfRecord sdf, string prefix, int start = 1)
		{
			string desc;
			StringBuilder sb = new StringBuilder();
			while ( !String.IsNullOrEmpty(desc = sdf.GetFieldValue(String.Format("{0}{1}", prefix, start++))) )
				sb.Append(desc);

			return sb.ToString();
		}
	}
}
