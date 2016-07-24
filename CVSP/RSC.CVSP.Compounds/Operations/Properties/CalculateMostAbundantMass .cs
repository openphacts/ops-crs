﻿using com.ggasoftware.indigo;
using RSC.Properties;
using System.Collections.Generic;

namespace RSC.CVSP.Compounds.Operations
{
	public class CalculateMostAbundantMass : OperationBase
	{
		public override void ProcessRecord(Record record, IDictionary<string, object> options = null)
		{
			var indigo = GetIndigo();

			IndigoObject obj = indigo.loadMolecule(record.Standardized);
			record.AddProperty(
				PropertyName.MOST_ABUNDANT_MASS,
				obj.mostAbundantMass(),
				OriginValueType.Calculated,
				PropertyManager.Current.GetIndigoProvenanceId(),
				PropertyManager.Current.GetUnitId("Da")
			);
		}

		public override IEnumerable<OperationInfo> GetOperations()
		{
			return new List<OperationInfo>() {
				new OperationInfo() {
					Id = "MAMCalculation",
					Name = "Most Abundant Mass Calculation",
					Description = "Calculate Most Abundant Mass"
				}
			};
		}
	}
}
