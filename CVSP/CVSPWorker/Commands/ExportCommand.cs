using ChemSpider.Molecules;
using CVSPWorker.Models;
using RSC.CVSP;
using RSC.CVSP.Compounds;
using RSC.CVSP.Search;
using RSC.Logging;
using RSC.Search;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CVSPWorker
{
    public class ExportCommand : IWorkerCommand
    {
        private readonly int CHUNK_SIZE = 100;

        private readonly ICVSPStore cvsp = null;
        private readonly ILogStore logStore;

        public ExportCommand(ICVSPStore cvsp, ILogStore logStore)
        {
            if (cvsp == null)
                throw new ArgumentNullException("cvsp");

            if (logStore == null)
                throw new ArgumentNullException("logStore");

            this.cvsp = cvsp;
            this.logStore = logStore;
        }

        public bool Execute(CVSPJob parameters)
        {
            var query = parameters.Query;
            var path = parameters.File;

            if (File.Exists(path))
                File.Delete(path);

            CVSPRecordsSearchModel search = Newtonsoft.Json.JsonConvert.DeserializeObject<CVSPRecordsSearchModel>(query);

            var rid = SearchUtility.RunSearchAsync(typeof(RecordsSearch), search.searchOptions, search.commonOptions, search.scopeOptions, search.resultOptions);

            RequestStatus status = RequestManager.Current.GetStatus(rid);
            while (status.Status == ERequestStatus.Processing)
            {
                Trace.WriteLine("Searching...");

                Thread.Sleep(2000);

                status = RequestManager.Current.GetStatus(rid);
            }

            if (status.Status == ERequestStatus.ResultReady)
            {
                Trace.WriteLine("Search done. Number records to export: " + status.Count.ToString());

                var index = 0;
                while (index < status.Count)
                {
                    Trace.WriteLine("Dump next " + CHUNK_SIZE + " records...");

                    var guids = RequestManager.Current.GetResults(rid, index, CHUNK_SIZE).Cast<Guid>().ToList();

                    var records = cvsp.GetRecords(guids);

                    Dump(path, records.OfType<CompoundRecord>(), search);

                    index += CHUNK_SIZE;

                    if(status.Count - index > 0)
                        Trace.WriteLine("Done. Left to export " + (status.Count - index) + " records.");
                }
            }
            else
            {
                Trace.WriteLine("Something went wrong... search status: " + status.Status.ToString());
            }

            return true;
        }

		private void Dump(string path, IEnumerable<CompoundRecord> compounds, CVSPRecordsSearchModel search)
        {
            StringBuilder sb = new StringBuilder();

            compounds.ToList().ForEach(c =>
            {
                SdfRecord sdf = SdfRecord.FromString(c.Original);

                var logs = logStore.GetLogEntries(c.Issues.Where(i => search.searchOptions.Codes.Contains(i.Code)).Select(i => i.Id));

                if (logs.Any())
                    sdf.Properties.Add("issues", logs.Where(l => !string.IsNullOrEmpty(l.Message)).Select(l => l.Message.TrimEnd()).Distinct().ToList());

                sb.Append(sdf.ToString());
            });

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(sb.ToString());
            }
        }
    }
}
