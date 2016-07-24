using InChINet;
using RSC.Properties;
using System.Collections.Generic;

namespace RSC.CVSP.Compounds.Operations
{
	public class CalculateNonStdInChI : OperationBase
	{
		public override void ProcessRecord(Record record, IDictionary<string, object> options = null)
		{
			string[] nonStdInchi = InChIUtils.mol2inchiinfo(record.Standardized, InChINet.InChIFlags.CRS);

			record.AddProperty(
				PropertyName.NON_STD_INCHI,
				nonStdInchi[0],
				OriginValueType.Calculated,
				PropertyManager.Current.GetInChIProvenanceId("InChINet.InChIFlags.CRS")
			);

			record.AddProperty(
				PropertyName.NON_STD_INCHI_KEY,
				nonStdInchi[1],
				OriginValueType.Calculated,
				PropertyManager.Current.GetInChIProvenanceId("InChINet.InChIFlags.CRS")
			);
		}

		public override IEnumerable<OperationInfo> GetOperations()
		{
			return new List<OperationInfo>() {
				new OperationInfo() {
					Id = "NonStdInChICalculation",
					Name = "Non Standard InChI Calculation",
					Description = "Calculate Non Standard InChI"
				}
			};
		}
	}
}
