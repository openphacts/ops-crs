using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
	public class CompoundStandardization : IRecordStandardization
	{
		public Standardization Standardization { get; private set; }

		public CompoundStandardization(IAcidity acidity, IStandardizationModule standardizationModule, IStandardizationChargesModule standardizationChargesModule, IStandardizationFragmentsModule standardizationFragmentsModule, IStandardizationStereoModule standardizationStereoModule, IStandardizationMetalsModule standardizationMetalsModule)
		{
			//if (!String.IsNullOrEmpty(acidBaseXmlFilePath) && File.Exists(acidBaseXmlFilePath))
			//	Acidity = new Acidity(File.ReadAllText(acidBaseXmlFilePath));

			//if (!String.IsNullOrEmpty(standardizationXmlFilePath) && File.Exists(standardizationXmlFilePath))
			//	Standardization = new Standardization(File.ReadAllText(standardizationXmlFilePath), Acidity);

			Standardization = new Standardization(acidity, standardizationModule, standardizationChargesModule, standardizationFragmentsModule, standardizationStereoModule, standardizationMetalsModule);
		}

		public Record Standardize(Record record)
		{
			var result = Standardization.Standardize(record.Original, Resources.Vendor.InChI);

			record.Issues = record.Issues.Concat(result.Issues).ToList();
			record.Standardized = result.Standardized;

			return record;
		}
	}
}
