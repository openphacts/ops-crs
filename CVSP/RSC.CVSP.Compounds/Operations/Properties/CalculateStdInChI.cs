using RSC.Properties;
using System.Collections.Generic;
using System;

namespace RSC.CVSP.Compounds.Operations
{
	public class CalculateStdInChI : OperationBase
	{
		public override void ProcessRecord(Record record, IDictionary<string, object> options = null)
		{
			string[] stdInchi = InChINet.InChIUtils.mol2inchiinfo(record.Standardized, InChINet.InChIFlags.Standard);
            if (stdInchi[0] == null || stdInchi[1] == null) throw new Exception("Standard InChI not calculated");
			record.AddProperty(
				PropertyName.STD_INCHI,
				stdInchi[0],
				OriginValueType.Calculated,
				PropertyManager.Current.GetInChIProvenanceId("InChINet.InChIFlags.Standard")
			);

			record.AddProperty(
				PropertyName.STD_INCHI_KEY,
				stdInchi[1],
				OriginValueType.Calculated,
				PropertyManager.Current.GetInChIProvenanceId("InChINet.InChIFlags.Standard")
			);
		}

		public override IEnumerable<OperationInfo> GetOperations()
		{
			return new List<OperationInfo>() {
				new OperationInfo() {
					Id = "StdInChICalculation",
					Name = "Std InChI Calculation",
					Description = "Calculate Std InChI"
				}
			};
		}
	}
}
