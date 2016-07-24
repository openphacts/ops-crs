using RSC.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
	public static class RecordExtensions
	{
		public static void AddChemSpiderProperty(this Record record, string name, double? value, double? error)
		{
			if (value == null)
				return;

			record.AddProperty(
				name,
				(double)value,
				error,
				OriginValueType.Calculated,
				PropertyManager.Current.GetChemSpiderProvenanceId()
			);
		}

		public static void AddChemSpiderProperty(this Record record, string name, object value, Condition condition = null)
		{
			if (value == null)
				return;

			record.AddProperty(
				name,
				value,
				OriginValueType.Calculated,
				PropertyManager.Current.GetChemSpiderProvenanceId(),
				null,
				condition
			);
		}
	}
}
