using RSC.CVSP;
using RSC.CVSP.EntityFramework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
namespace CVSPService
{
	partial class ChemValidatorProcessing
	{
		public static void UpdateDepositionsStatuses()
		{
			CVSPStore db = new EFCVSPStore();
			IEnumerable<Guid> guids = db.GetDepositionsByStatus(DepositionStatus.Processing);
			foreach (Guid guid in guids)
			{
				string depositionRootDirectory = Path.Combine(ConfigurationManager.AppSettings["data_path"], guid.ToString());
				string chunksDirectory = Path.Combine(depositionRootDirectory, Resources.DirectoryName.Chunks.ToString());
				string uploadDirectory = Path.Combine(depositionRootDirectory, Resources.DirectoryName.Uploaded.ToString());
				if (!Directory.Exists(depositionRootDirectory) || !Directory.Exists(chunksDirectory) || !Directory.Exists(uploadDirectory))
					continue;

				int numOfChunks = Directory.GetFiles(chunksDirectory, "*.xml.gz").Count();
				int numOfUploadedChunks = Directory.GetFiles(uploadDirectory, "*." + Resources.StatusFileExtension.Uploaded).Count();
				if (numOfChunks != 0 && numOfChunks == numOfUploadedChunks)
					db.UpdateDepositionStatus(guid, DepositionStatus.Processed);

				//for revisions
				//int revisedAndProcessedRecordCount = db.GetRecords(new List<Guid>() { guid }).Where(r => r.IsRevised).Where(r => r.IsProcessed).Count();
				//if (revisedAndProcessedRecordCount > 0)
				//	db.UpdateDepositionStatus(guid, DepositionStatus.Processed);
			}
			
		}
	}
}
*/