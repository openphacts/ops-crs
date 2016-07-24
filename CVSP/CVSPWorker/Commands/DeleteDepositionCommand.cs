using RSC.CVSP;
using RSC.CVSP.EntityFramework;
using RSC.CVSP.Utils;
using RSC.Process;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVSPWorker
{
	public class DeleteDepositionCommand  : IWorkerCommand
	{
		private readonly ICVSPStore cvsp = null;
		private readonly IFileStorage fileStorage = null;
		private readonly IChunkManager chunkManager = null;
		private readonly IJobManager jobManager = null;

		public DeleteDepositionCommand(ICVSPStore cvsp, IFileStorage fileStorage, IChunkManager chunkManager, IJobManager jobManager)
		{
			if (cvsp == null)
				throw new ArgumentNullException("cvsp");

			if (fileStorage == null)
				throw new ArgumentNullException("fileStorage");

			if (chunkManager == null)
				throw new ArgumentNullException("chunkManager");

			if (jobManager == null)
				throw new ArgumentNullException("jobManager");

			this.cvsp = cvsp;
			this.fileStorage = fileStorage;
			this.chunkManager = chunkManager;
			this.jobManager = jobManager;
		}

		public bool Execute(CVSPJob parameters)
		{
			//cleaning database for this deposition
			cvsp.DeleteDeposition(parameters.Deposition);

			fileStorage.CleanFiles(parameters.Deposition);

			chunkManager.DeleteDepositionChunks(parameters.Deposition);

			jobManager.DeleteDepositionJobs(parameters.Deposition);

			return true;
		}
	}
}
