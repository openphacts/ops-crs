using RSC.CVSP;
using RSC.CVSP.EntityFramework;
using RSC.CVSP.Utils;
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
		public static void UploadProcessedResults()
		{
			CVSPStore db = new EFCVSPStore();
			IEnumerable<Guid> guids = db.GetDepositionsByStatus(DepositionStatus.Processing);
			foreach (Guid guid in guids)
			{
				if (!Utils.CanStartNewProcesses(guid, maxProcessLimit))
					continue;
				string processedDir = Path.Combine(ConfigurationManager.AppSettings["data_path"], guid.ToString(), Resources.DirectoryName.Processed.ToString());
				string uploadDir = Path.Combine(ConfigurationManager.AppSettings["data_path"], guid.ToString(), Resources.DirectoryName.Uploaded.ToString());
				if (!Directory.Exists(processedDir))
					Directory.CreateDirectory(processedDir);
				if (!Directory.Exists(uploadDir))
					Directory.CreateDirectory(uploadDir);
				UploadXMLDir(processedDir, uploadDir, guid, maxProcessLimit);

			}

			IEnumerable<Guid> guids2 = db.GetDepositionsByStatus(DepositionStatus.ProcessingForGCN);
			foreach (Guid guid in guids2)
			{
				if (!Utils.CanStartNewProcesses(guid, maxProcessLimit))
					continue;
				string processedDir = Path.Combine(ConfigurationManager.AppSettings["data_path"], guid.ToString(), Resources.DirectoryName.Gcn.ToString(),Resources.DirectoryName.Processed.ToString());
				string uploadDir = Path.Combine(ConfigurationManager.AppSettings["data_path"], guid.ToString(), Resources.DirectoryName.Gcn.ToString(), Resources.DirectoryName.Uploaded.ToString());
				if (!Directory.Exists(processedDir))
					Directory.CreateDirectory(processedDir);
				if (!Directory.Exists(uploadDir))
					Directory.CreateDirectory(uploadDir);
				UploadXMLDir(processedDir, uploadDir, guid, maxProcessLimit, true);
			}
		}

		public static void UploadXMLDir(string XMLDirPath, string UploadDirPath, Guid depositionGuid, int maxProcessLimit, bool upload2Gcn=false)
		{
			CVSPStore db = new EFCVSPStore();
			RSC.CVSP.Deposition deposition = db.GetDeposition(depositionGuid);
			foreach (string statusFile in Directory.GetFiles(XMLDirPath, "*." + Resources.StatusFileExtension.Processed))
			{
				if (!Utils.CanStartNewProcesses(deposition.Id, maxProcessLimit))
					break;

				string chunkGuid = Path.GetFileNameWithoutExtension(statusFile);
				CommandParameters commandParameters = new CommandParameters();
				if(upload2Gcn)
					commandParameters.AddParameter(Resources.CommandParameter.processingType, Resources.ProcessingType.XMLUpload2Gcn.ToString());
				else
					commandParameters.AddParameter(Resources.CommandParameter.processingType, Resources.ProcessingType.XMLUpload2DB.ToString());
				commandParameters.AddParameter(Resources.CommandParameter.inputFilePath, Path.Combine(XMLDirPath, chunkGuid + ".xml.gz"));
				commandParameters.AddParameter(Resources.CommandParameter.depositionId, deposition.Id.ToString());
				commandParameters.AddParameter(Resources.CommandParameter.logFilePath, Path.Combine(UploadDirPath, chunkGuid + ".log"));
				commandParameters.AddParameter(Resources.CommandParameter.statusFile, Path.Combine(UploadDirPath, chunkGuid + "." + Resources.StatusFileExtension.Uploaded));

				if (deposition.DataDomain == DataDomain.Spectra)
					commandParameters.AddParameter(Resources.CommandParameter.dataDomain, DataDomain.Spectra.ToString());
				else if (deposition.DataDomain == DataDomain.Crystals)
					commandParameters.AddParameter(Resources.CommandParameter.dataDomain, DataDomain.Crystals.ToString());
				else if (deposition.DataDomain == DataDomain.Reactions)
					commandParameters.AddParameter(Resources.CommandParameter.dataDomain, DataDomain.Reactions.ToString());
				else
					commandParameters.AddParameter(Resources.CommandParameter.dataDomain, DataDomain.Substances.ToString());

				string commandFilePath = Path.Combine(UploadDirPath, chunkGuid + ".params");
				commandParameters.Store(commandFilePath);

				Utils.LaunchProcess(deposition.Id, commandFilePath);
				if (File.Exists(statusFile))
					File.Delete(statusFile);
				File.Create(Path.Combine(Directory.GetParent(statusFile).ToString(), chunkGuid + "." + Resources.StatusFileExtension.Uploading.ToString()));
			}
		}
	}
}
*/