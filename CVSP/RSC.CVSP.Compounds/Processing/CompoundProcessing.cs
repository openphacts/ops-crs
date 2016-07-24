using System;
using RSC.CVSP.Compounds.Operations;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;

namespace RSC.CVSP.Compounds
{
	public class CompoundProcessing : RecordProcessing
	{
		public CompoundProcessing(IEnumerable<ProcessingParameter> parameters)
		{
			var operations = ServiceLocator.Current.GetService(typeof(IEnumerable<IOperation>)) as IEnumerable<IOperation>;

			foreach (var operation in operations)
			{
				if (operation.GetOperations().Where(o => o.Id.Equals("ParentsCalculation")).Any())
				{
					if (!parameters.AsBool("ParentsGeneration"))
						continue;
				}
				else if (operation.GetOperations().Where(o => o.Id.Equals("ChemSpiderPropertiesImport")).Any())
				{
					if (!parameters.AsBool("ChemSpiderProperties"))
						continue;
				}
				else if (operation.GetOperations().Where(o => o.Id.Equals("ChemSpiderSynonymsImport")).Any())
				{
					if (!parameters.AsBool("ChemSpiderSynonyms"))
						continue;
				}
				else
				{
					if (!parameters.AsBool("PropertiesCalculation"))
						continue;
				}

				Operations.Add(operation);
			}
		}
	}
}
