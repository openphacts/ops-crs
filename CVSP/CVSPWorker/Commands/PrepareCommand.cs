using RSC.CVSP;
using RSC.CVSP.Compounds;
using RSC.CVSP.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChemSpider.Molecules;
using RSC.CVSP.EntityFramework;
using RSC.Process;

namespace CVSPWorker
{
	public class PrepareCommand : IWorkerCommand
	{
		private readonly ICVSPStore cvsp = null;
		private readonly IChunkManager chunkManager = null;
		private readonly IJobManager jobManager = null;
		private readonly IFileStorage fileStorage = null;

		public PrepareCommand(ICVSPStore cvsp, IChunkManager chunkManager, IJobManager jobManager, IFileStorage fileStorage)
		{
			if (cvsp == null)
				throw new ArgumentNullException("cvsp");

			if (chunkManager == null)
				throw new ArgumentNullException("chunkManager");

			if (jobManager == null)
				throw new ArgumentNullException("jobManager");

			if (fileStorage == null)
				throw new ArgumentNullException("fileStorage");

			this.cvsp = cvsp;
			this.chunkManager = chunkManager;
			this.jobManager = jobManager;
			this.fileStorage = fileStorage;
		}

		public bool Execute(CVSPJob parameters)
		{
			var files = fileStorage.GetFiles(parameters.Deposition);

			//	mark deposition as Processing...
			cvsp.UpdateDepositionStatus(parameters.Deposition, DepositionStatus.Processing);

			var deposition = cvsp.GetDeposition(parameters.Deposition);

			IReadRecords reader = null;

			switch (parameters.DataDomain)
			{
				case DataDomain.Substances:
					reader = new ReadCompoundRecords(files.First());
					break;
				default:
					break;
			}

			int chunkSize = calculateChunkSize(files.First());

			while (true)
			{
				var records = reader.ReadChunk(chunkSize);
				if (records.Count() == 0)
					break;

				var chunkId = chunkManager.CreateChunk(parameters.Deposition, ChunkType.Original, records);

				jobManager.NewJob(new CVSPJob()
				{
					Command = "process",
					Deposition = parameters.Deposition,
					Chunk = chunkId,
					DoValidate = deposition.Parameters.AsBool("Validate"),
					DoStandardize = deposition.Parameters.AsBool("Standardize")
				});
			}

			reader.Release();
			return true;
		}

		private int calculateChunkSize(string filePath)
		{
			FileInfo fi = new FileInfo(filePath);
			int splitChunkSize = 0;

			double uncompressedFileSizeInMb = fi.Length / 1000000.0;
			bool isComressed = false;
			if (Path.GetExtension(filePath).ToLower().Equals(".gz") || Path.GetExtension(filePath).ToLower().Equals(".zip"))
				isComressed = true;
			if (isComressed)
				uncompressedFileSizeInMb = uncompressedFileSizeInMb * 10.0;//roughly 1/10 compression is assumed

			if (uncompressedFileSizeInMb <= 10) splitChunkSize = 100;
			else if (uncompressedFileSizeInMb <= 100) splitChunkSize = 200;
			else if (uncompressedFileSizeInMb <= 1000) splitChunkSize = 500;
			else if (uncompressedFileSizeInMb <= 10000) splitChunkSize = 1000;
			else splitChunkSize = 5000;

			return splitChunkSize;
		}
	}
}
