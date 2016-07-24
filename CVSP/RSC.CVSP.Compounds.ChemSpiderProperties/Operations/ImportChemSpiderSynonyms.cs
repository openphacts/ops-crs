using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using RSC.CVSP.Compounds.com.chemspider.www.PropertiesImport;
using RSC.CVSP.Compounds.com.chemspider.www.Synonyms;
using RSC.CVSP;
using RSC.Collections;
using RSC.Compounds;
using RSC.Properties;

namespace RSC.CVSP.Compounds.Operations
{
	public class ImportChemSpiderSynonyms : OperationBase
	{
		public override IEnumerable<Record> Process(IEnumerable<Record> records, IDictionary<string, object> options = null)
		{
			var csids = records.Cast<CompoundRecord>()
                .Where(c => c.Properties != null)
                .Where(c => c.Properties.Any(p => p.Name == PropertyName.CSID))
                .Select(c => c.GetProperty(PropertyName.CSID))
                .Cast<int>()
                .ToList();

			//Call the Synonyms.asmx ChemSpider Web Service.
			var ws_synonyms = new RSC.CVSP.Compounds.com.chemspider.www.Synonyms.Synonyms()
			{
				Timeout = ConfigurationManager.AppSettings.GetInt("ws_timeout", 30000)
			};

			//Get the number of retries from the config file.
			int ws_retries = ConfigurationManager.AppSettings.GetInt("ws_retries", 50);
			List<SynonymsInfo> synonyms = null;
			bool ws_success = false;

			//Keep retrying the web service until we have a success or number of retries exceeded.
			while (!ws_success)
			{
				try
				{
					synonyms = ws_synonyms.RetrieveByCSIDList(csids.ToArray(), ConfigurationManager.AppSettings["security_token"]).ToList();
					ws_success = true;
				}
				catch (Exception ex)
				{
					ws_retries--;
					if (ws_retries == 0)
						throw ex; //We have exceeded the retries count so throw this error.
				}
			}

			//Populate the Synonyms with the ChemSpider web service results.
            foreach (var compound in records.Cast<CompoundRecord>().Where(c => c.Properties != null && c.GetProperties().Any(i => i.Name == PropertyName.CSID)))
			{
				//Get the csid.
                int csid = Convert.ToInt32(compound.GetProperty(PropertyName.CSID));

				foreach (var synonym in synonyms.Where(s => s.CSID == csid))
				{
					//Set the flags.
					List<SynonymFlag> synonymFlags = new List<SynonymFlag>();

					if (synonym.IsDbId)
						synonymFlags.Add(new SynonymFlag { Name = "DBID" });

					//Add the synonym to the CompoundRecord.
					compound.AddChemSpiderSynonym(new Synonym
					{
                        IsTitle = synonym.IsCompoundTitle,
					    State = synonym.IsValidated ? 
                                CompoundSynonymState.eApproved : 
                                CompoundSynonymState.eUnconfirmed,
						Name = synonym.Synonym,
						LanguageId = synonym.LangId,
						Flags = (synonymFlags.Count > 0 ? synonymFlags : null)
					});
				}
			}

			return records;
		}

		public override IEnumerable<OperationInfo> GetOperations()
		{
			return new List<OperationInfo>() {
				new OperationInfo() {
					Id = "ChemSpiderSynonymsImport",
					Name = "Import ChemSpider Synonyms",
					Description = "Import ChemSpider Synonyms"
				}
			};
		}
	}
}
