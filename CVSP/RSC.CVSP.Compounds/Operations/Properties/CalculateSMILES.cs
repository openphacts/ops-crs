using com.ggasoftware.indigo;
using RSC.Properties;
using System.Collections.Generic;

namespace RSC.CVSP.Compounds.Operations
{
	public class CalculateSMILES : OperationBase
	{
		public override void ProcessRecord(Record record, IDictionary<string, object> options = null)
		{
			var indigo = GetIndigo();

			IndigoObject original = indigo.loadMolecule(record.Original);
			IndigoObject standardized = indigo.loadMolecule(record.Standardized);

			record.AddProperty(
				PropertyName.STANDARDIZED_SMILES,
				standardized.canonicalSmiles(),
				OriginValueType.Calculated,
				PropertyManager.Current.GetIndigoProvenanceId()
			);

			record.AddProperty(
				PropertyName.ORIGINAL_SMILES,
				original.canonicalSmiles(),
				OriginValueType.Calculated,
				PropertyManager.Current.GetIndigoProvenanceId()
			);
		}

		public override IEnumerable<OperationInfo> GetOperations()
		{
			return new List<OperationInfo>() {
				new OperationInfo() {
					Id = "SMILESCalculation",
					Name = "SMILES Calculation",
					Description = "Calculate SMILES"
				}
			};
		}
	}
}
