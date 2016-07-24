using RSC.Compounds;
using RSC.CVSP;
using RSC.CVSP.Compounds;
using RSC.Collections;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using RSC.Properties;

namespace CVSPWorker
{
    public class Upload2GCNCommand : IWorkerCommand
    {
        private readonly ICVSPStore cvsp = null;
        private readonly ISubstanceBulkUpload substances = null;
		private readonly ICompoundStore compounds = null;
		private readonly IPropertyStore properties = null;
        private readonly RSC.CVSP.IStatistics stats;
        public Upload2GCNCommand(ICVSPStore cvsp, ISubstanceBulkUpload substances, IPropertyStore properties, RSC.CVSP.IStatistics stats, ICompoundStore compounds)
        {
            if (cvsp == null)
                throw new ArgumentNullException("cvsp");

            if (substances == null)
                throw new ArgumentNullException("substances");

			if (compounds == null)
				throw new ArgumentNullException("compounds");

			if (properties == null)
                throw new ArgumentNullException("properties");

            if (stats == null)
                throw new ArgumentNullException("stats");

            this.cvsp = cvsp;
            this.substances = substances;
			this.compounds = compounds;
			this.properties = properties;
            this.stats = stats;
        }

        public bool Execute(CVSPJob parameters)
        {
            Stopwatch totalStopWatch = new Stopwatch();
            totalStopWatch.Start();

            cvsp.UpdateDepositionStatus(parameters.Deposition, DepositionStatus.Depositing2GCN);

			var start = parameters.GetInt("start", 0);
			var stop = parameters.GetInt("stop", 0);

            var chunkSize = CalculateChunkSize(parameters.Deposition);

            while (true)
            {
                Stopwatch stopWatch = new Stopwatch();
                Trace.WriteLine(string.Format("Getting records' GUIDs from {0} to {1}...", start, start + chunkSize));
                stopWatch.Start();

				var guids = cvsp.GetDepositionRecords(parameters.Deposition, start, stop != 0 ? Math.Min(chunkSize, stop - start < 0 ? 0 : stop - start) : chunkSize);
				Trace.WriteLine(string.Format("Done: {0}", stopWatch.Elapsed.ToString()));

				if (!guids.Any())
					break;

				if (parameters.GetBool("newOnly"))
				{
					Trace.WriteLine(string.Format("Checking existing records..."));
					stopWatch.Restart();

					var existing = compounds.GetExistingRevisions(guids);
					guids = guids.Where(id => !existing.Contains(id)).ToList();

					Trace.WriteLine(string.Format("Done: {0}", stopWatch.Elapsed.ToString()));

					if (!guids.Any())
					{
						start += chunkSize;
						continue;
					}
				}

                Trace.WriteLine(string.Format("Reading next chunk ({0}) of records...", guids.Count()));
                stopWatch.Restart();

                //var records = cvsp.GetRecords(guids, new string[] { "Id", "Ordinal", "DataDomain", "Original", "Standardized", "Issues", "Properties", "Dynamic" });
                var records = cvsp.GetRecords(guids);

                Trace.WriteLine(string.Format("Done: {0}", stopWatch.Elapsed.ToString()));

                //	upload compounds...
                Trace.WriteLine(string.Format("Uploading records (number of records: {0}) to GCN...", guids.Count()));
                stopWatch = new Stopwatch();
				stopWatch.Restart();

                UploadCompoundChunk(parameters.Deposition, records.OfType<CompoundRecord>());

                Trace.WriteLine(string.Format("Done: {0}", stopWatch.Elapsed.ToString()));

                start += chunkSize;

                Trace.WriteLine(string.Format("Number of processed records: {0}", start));
            }

            cvsp.UpdateDepositionStatus(parameters.Deposition, DepositionStatus.Deposited2GCN);

            Trace.WriteLine(string.Format("Total time: {0}", totalStopWatch.Elapsed.ToString()));

            return true;
        }

        private bool UploadCompoundChunk(Guid depositionId, IEnumerable<CompoundRecord> records)
        {
            var data = new List<RSC.Compounds.RecordData>();

			Stopwatch stopWatch = new Stopwatch();
			Trace.WriteLine("Getting record properties...");
			stopWatch.Start();

            //	pre-load records' properties in bulk mode in order to prevent loading properties for every record...
            var recordProperties = properties.GetRecordsProperties(records.Select(r => r.Id), new string[] {
				PropertyName.STD_INCHI,
				PropertyName.STD_INCHI_KEY,
				PropertyName.NON_STD_INCHI,
				PropertyName.NON_STD_INCHI_KEY,
				PropertyName.ORIGINAL_SMILES,
				PropertyName.STANDARDIZED_SMILES,
                PropertyName.CSID});

			Trace.WriteLine(string.Format("Done: {0}", stopWatch.Elapsed.ToString()));

            foreach (var rec in recordProperties)
            {
                var record = records.FirstOrDefault(r => r.Id == rec.Key);
                if (record != null)
                    record.Properties = rec.Value;
            }

            //	upload compounds...
            foreach (var record in records.Where(r => r.DataDomain == DataDomain.Substances))
            {
                //	we do not work with records if external regId is not specified...
				if (string.IsNullOrEmpty(record.RegId) ||							//	record has external ID. We currently work only with records that has ExtID
					string.IsNullOrEmpty(record.Standardized) ||					//	record was standardized successfully
					record.StandardizedNonStdInChI == null 							//	record has NonStdInChI generated
					/*record.Issues.Where(i => i.IssueDefinition.Severity == RSC.Logging.Severity.Error).Any()*/)	//	record has any errors
				{
					if (string.IsNullOrEmpty(record.RegId))
						Trace.WriteLine(string.Format("Error: Record with ordinal number {0} doesn't have External Id", record.Ordinal));
					if (string.IsNullOrEmpty(record.Standardized))
						Trace.WriteLine(string.Format("Error: Record with ordinal number {0} is not standardized", record.Ordinal));
					if (record.StandardizedNonStdInChI == null)
						Trace.WriteLine(string.Format("Error: Non Standard InChI wasn't generated for record with ordinal number {0}", record.Ordinal));

					continue;
				}

                var compound = new RSC.Compounds.Compound()
                {
                    Id = record.Id.ObjectId ?? Guid.Empty,
                    Mol = record.Standardized,
                    Smiles = new RSC.Compounds.Smiles(record.StandardizedSmiles),
                    StandardInChI = record.StandardizedStdInChI,
                    NonStandardInChI = record.StandardizedNonStdInChI
                };

                //Create the External Ids for the compound.
                var externalIds = new List<ExternalReference>();

                //Add the ChemSpider Id to the list of Identifiers.
                if (!string.IsNullOrEmpty(record.ChemSpiderId))
                {
                    externalIds.Add(new ExternalReference()
                    {
                        Type = new ExternalReferenceType { UriSpace = "http://rdf.chemspider.com/" },
                        Value = record.ChemSpiderId
                    });
                }

                //Populate ExternalIds.
                compound.ExternalReferences = externalIds;

                var revision = new RSC.Compounds.Revision()
                {
                    DateCreated = DateTime.Now,
                    Sdf = record.Original,
                    Issues = record.Issues,
                    Properties = record.PropertyIDs,
                    CompoundId = record.Id.ObjectId ?? Guid.Empty
                };

                var recordData = new RSC.Compounds.RecordData()
                {
                    ExternalId = record.RegId,
                    Compound = compound,
                    Revision = revision,
                };

                if (record.GetParents().Any())
                {
                    recordData.Parents = record.GetParents().Select(p => new RSC.Compounds.Parent()
                    {
                        Relationship = p.Relationship,
                        Mol = p.MolFile,
                        StandardInChI = p.StdInChI,
                        NonStandardInChI = p.NonStdInChI,
                        TautomericNonStdInChI = p.TautomericInChI
                    }).ToList();
                }

                if (record.GetChemSpiderSynonyms().Any())
                {
                    compound.Synonyms = record.GetChemSpiderSynonyms();
                }

                data.Add(recordData);
            }

            //Uncomment for testing purposes - capture the upload to GCN JSON so it can be unit tested.
            //using (StreamWriter sw = new StreamWriter(@"C:\temp\uploadtogcn.json", false, Encoding.UTF8))
            //{
            //    sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(data, new Newtonsoft.Json.JsonSerializerSettings { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All, Formatting = Newtonsoft.Json.Formatting.Indented }));
            //}

			Trace.WriteLine(string.Format("Pushing data to Compounds bucket: {0} records...", data.Count));

            substances.BulkUpload(depositionId, data);

            return true;
        }

        private int CalculateChunkSize(Guid guid)
        {
            if (ConfigurationManager.AppSettings.HasKey("upload2gcn_chunk_size"))
                return ConfigurationManager.AppSettings.GetInt("upload2gcn_chunk_size");

            var depositionStats = stats.GetDepositionStats(guid);

            if (depositionStats.RecordsNumber <= 100) return 10;
            else if (depositionStats.RecordsNumber <= 1000) return 25;
            else if (depositionStats.RecordsNumber <= 10000) return 50;
            else if (depositionStats.RecordsNumber <= 100000) return 100;
            else return 500;
        }
    }
}
