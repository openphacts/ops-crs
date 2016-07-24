using ChemSpider.Molecules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
	public class CompoundValidation : IRecordValidation
	{
		public Validation Validation { get; private set; }

		public CompoundValidation(IAcidity acidity, IValidationModule validationModule,
            IValidationStereoModule validationStereoModule, IValidationRuleModule validationRuleModule)
		{
			//SdfFieldDictionary = dictionary;
			//if (!String.IsNullOrEmpty(acidBaseXmlFilePath) && File.Exists(acidBaseXmlFilePath))
			//	Acidity = new Acidity(File.ReadAllText(acidBaseXmlFilePath));

			//if (!String.IsNullOrEmpty(validationXmlFilePath) && File.Exists(validationXmlFilePath))
			//Validation = new Validation(File.ReadAllText(validationXmlFilePath),Acidity);

			Validation = new Validation(acidity, validationModule, validationStereoModule, validationRuleModule);
		}

		public Record Validate(Record record)
		{
			var result = Validation.Validate(record.Original);

			record.Issues = result.Issues.ToList();

			return record;
		}
	}
}
