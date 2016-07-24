using RSC.CVSP;
using RSC.CVSP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RSC.CVSP.EntityFramework;
using System.IO;
using RSC.Process;
using System.Xml;
using System.Runtime.Serialization;
using RSC.Logging;
using RSC.Properties;
using System.Diagnostics;

namespace CVSPWorker
{
	public class UploadCommand : IWorkerCommand
	{
		private readonly ICVSPStore cvsp = null;
		private readonly IChunkManager chunkManager = null;
		private readonly IPropertyStore propertyStore = null;

		public UploadCommand(ICVSPStore cvsp, IChunkManager chunkManager, IPropertyStore propertyStore)
		{
			if (cvsp == null)
				throw new ArgumentNullException("cvsp");

			if (chunkManager == null)
				throw new ArgumentNullException("chunkManager");

			if (propertyStore == null)
				throw new ArgumentNullException("propertyStore");

			this.cvsp = cvsp;
			this.chunkManager = chunkManager;
			this.propertyStore = propertyStore;
		}

		public bool Execute(CVSPJob parameters)
		{
			chunkManager.ChangeStatus(parameters.Chunk, ChunkStatus.Processing);

			//check that deposition exists in db
			RSC.CVSP.Deposition deposition = cvsp.GetDeposition(parameters.Deposition);
			if (deposition == null)
				throw new DepositionNotFoundException();

			parameters.StartWatch("UploadCommand:chunkManager.GetRecords");
			var records = chunkManager.GetRecords(parameters.Chunk) as IEnumerable<RSC.CVSP.Record>;
			parameters.StopWatch("UploadCommand:chunkManager.GetRecords");

			Stopwatch watch = new Stopwatch();
			watch.Start();
			Trace.TraceInformation("Upload issues...");

			parameters.StartWatch("UploadCommand:UploadRecordsIssues");
			UploadRecordsIssues(records);
			parameters.StopWatch("UploadCommand:UploadRecordsIssues");

			Trace.TraceInformation("Upload issues... done: {0}", watch.Elapsed.ToString());
			watch.Restart();
			Trace.TraceInformation("Upload properties...");

			parameters.StartWatch("UploadCommand:UploadRecordsProperties");
			UploadRecordsProperties(records);
			parameters.StopWatch("UploadCommand:UploadRecordsProperties");

			Trace.TraceInformation("Upload properties... done: {0}", watch.Elapsed.ToString());
			watch.Restart();
			Trace.TraceInformation("Upload records...");

			parameters.StartWatch("UploadCommand:cvsp.CreateRecords");
			var guids = cvsp.CreateRecords(deposition.Id, records);
			parameters.StopWatch("UploadCommand:cvsp.CreateRecords");

			Trace.TraceInformation("Upload records... done: {0}", watch.Elapsed.ToString());

			chunkManager.ChangeStatus(parameters.Chunk, ChunkStatus.Processed);

			//	check if all chunks been processed and uploaded...
			CheckIfAllChunksProcessed(deposition.Id);

			return true;
		}

		private void UploadRecordsIssues(IEnumerable<Record> records)
		{
			foreach(var r in records)
				foreach (var i in r.Issues)
					LogManager.Logger.LogIssue(r.Id, i);

			LogManager.Logger.Flush();
		}

		private void UploadRecordsProperties(IEnumerable<Record> records)
		{
			var dictionary = records.ToDictionary(r => r.Id, r => r.Properties.AsEnumerable());
			propertyStore.AddRecordsProperties(dictionary);
		}

		private void CheckIfAllChunksProcessed(Guid depositionId)
		{
			var processedStats = chunkManager.GetDepositionChunksStats(depositionId, ChunkType.Processed);

			if (processedStats.Total == processedStats.Processed)
			{
				cvsp.UpdateDepositionStatus(depositionId, DepositionStatus.Processed);
			}
		}
	}
}
