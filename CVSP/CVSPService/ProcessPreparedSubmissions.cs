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

namespace CVSPService
{
	partial class ChemValidatorProcessing
	{
/*
		public static void ProcessPreparedSubmissions()
		{
			CVSPStore db = new EFCVSPStore();
			IEnumerable<Guid> depositionsGuids = db.GetDepositionsByStatus(DepositionStatus.Processing);
			foreach (Guid depositionGuid in depositionsGuids)
				ChemValidatorProcessing.ProcessCVSPSubmission(depositionGuid);

			IEnumerable<Guid> gcnDepositionGuids = db.GetDepositionsByStatus(DepositionStatus.ProcessingForGCN);
			foreach (Guid depositionGuid in gcnDepositionGuids)
				ChemValidatorProcessing.ProcessGCNSubmission(depositionGuid);
		}

		public static void ProcessCVSPSubmission(Guid depositionGuid)
		{
			CVSPStore db = new EFCVSPStore();
			RSC.CVSP.Deposition deposition = db.GetDeposition(depositionGuid);
			string depositionRoot = Path.Combine(ConfigurationManager.AppSettings["data_path"], depositionGuid.ToString());
			string chunksDirectory = Path.Combine(depositionRoot, Resources.DirectoryName.Chunks.ToString());
			if (!Directory.Exists(chunksDirectory))
				return;

			if (!Directory.Exists(Path.Combine(depositionRoot, Resources.DirectoryName.Processed.ToString())))
				Directory.CreateDirectory(Path.Combine(depositionRoot, Resources.DirectoryName.Processed.ToString()));

			string[] preparedFiles = Directory.GetFiles(chunksDirectory, "*." + Resources.StatusFileExtension.Prepared);
			foreach (string file in preparedFiles)
			{
				if (!Utils.CanStartNewProcesses(depositionGuid, maxProcessLimit))
					return;
				LaunchProcessingJob(file, deposition);
			}

		}

		private static void ProcessGCNSubmission(Guid depositionGuid)
		{
			string depositionRoot = Path.Combine(ConfigurationManager.AppSettings["data_path"], depositionGuid.ToString(),Resources.DirectoryName.Gcn.ToString());
			if (!Directory.Exists(Path.Combine(depositionRoot, Resources.DirectoryName.Processed.ToString())))
				Directory.CreateDirectory(Path.Combine(depositionRoot, Resources.DirectoryName.Processed.ToString()));

			string chunksDirectory = Path.Combine(depositionRoot, Resources.DirectoryName.Chunks.ToString());
			if (!Directory.Exists(chunksDirectory))
				return;

			CVSPStore db = new EFCVSPStore();
			RSC.CVSP.Deposition deposition = db.GetDeposition(depositionGuid);
			
			string[] preparedFiles = Directory.GetFiles(chunksDirectory, "*." + Resources.StatusFileExtension.Prepared);
			foreach (string file in preparedFiles)
			{
				if (!Utils.CanStartNewProcesses(depositionGuid, maxProcessLimit))
					return;
				LaunchProcessingJob(file, deposition);
			}
		}

		private static bool LaunchProcessingJob(string chunkStatusFilePath, RSC.CVSP.Deposition deposition)
		{
			string depositionRootPath = Directory.GetParent(Directory.GetParent(chunkStatusFilePath).ToString()).ToString();
			string chunkGuid = Path.GetFileNameWithoutExtension(chunkStatusFilePath);

			File.Move(chunkStatusFilePath, chunkStatusFilePath.Replace(Resources.StatusFileExtension.Prepared.ToString(), Resources.StatusFileExtension.Processing.ToString()));

			//if (deposition.ProcessingParameters.IsProcessedLocally)
				LocalJob(deposition, depositionRootPath, chunkGuid);
			//else
			//	FarmJob(deposition, depositionRootPath, chunkGuid);

			return true;
		}

		private static void LocalJob(RSC.CVSP.Deposition deposition, string depositionRootPath, string chunkGuid)
		{
			CommandParameters cp = new CommandParameters();
			cp.AddParameter(Resources.CommandParameter.processingType, Resources.ProcessingType.Process.ToString());
			cp.AddParameter(Resources.CommandParameter.dataDomain, deposition.DataDomain.ToString());
			cp.AddParameter(Resources.CommandParameter.inputFilePath, Path.Combine(depositionRootPath, Resources.DirectoryName.Chunks.ToString(), chunkGuid + ".xml.gz"));
			cp.AddParameter(Resources.CommandParameter.outputFilePath, Path.Combine(depositionRootPath, Resources.DirectoryName.Processed.ToString(), chunkGuid + ".xml.gz"));
			cp.AddParameter(Resources.CommandParameter.logFilePath, Path.Combine(depositionRootPath, Resources.DirectoryName.Processed.ToString(), chunkGuid + ".log"));
			cp.AddParameter(Resources.CommandParameter.statusFile, Path.Combine(depositionRootPath, Resources.DirectoryName.Processed.ToString(), chunkGuid + "." + Resources.StatusFileExtension.Processed.ToString()));

			if (deposition.Status == DepositionStatus.ProcessingForGCN)
				cp.AddParameter(Resources.CommandParameter.isGcnDeposition, "true");

			else if (deposition.DataDomain == DataDomain.Substances)
			{
				cp.AddParameter(Resources.CommandParameter.doValidate, "true");
				cp.AddParameter(Resources.CommandParameter.doStandardize, "true");


				//if (deposition.Status == DepositionStatus.ProcessingForGCN || !deposition.ProcessingParameters.ValidationContentGuid.Equals(Guid.Empty))
				//{
				//	cp.AddParameter(Resources.CommandParameter.doValidate, "true");
				//	cp.AddParameter(Resources.CommandParameter.validationXMLFilePath, Path.Combine(depositionRootPath, "validation.xml"));
				//}
				//else
				//	cp.AddParameter(Resources.CommandParameter.doValidate, "false");

				//if (deposition.Status == DepositionStatus.ProcessingForGCN || !deposition.ProcessingParameters.StandardizationContentGuid.Equals(Guid.Empty))
				//{
				//	cp.AddParameter(Resources.CommandParameter.doStandardize, "true");
				//	cp.AddParameter(Resources.CommandParameter.standardizationXMLFilePath, Path.Combine(depositionRootPath, "standardization.xml"));
				//}
				//else
				//	cp.AddParameter(Resources.CommandParameter.doStandardize, "false");

				//if (deposition.Status == DepositionStatus.ProcessingForGCN || !deposition.ProcessingParameters.ValidationContentGuid.Equals(Guid.Empty)
				//	|| !deposition.ProcessingParameters.StandardizationContentGuid.Equals(Guid.Empty))
				//	cp.AddParameter(Resources.CommandParameter.acidBaseXMLFilePath, Path.Combine(depositionRootPath, "acidbase.xml"));

				//if (deposition.Status == DepositionStatus.ProcessingForGCN)
				//	cp.AddParameter(Resources.CommandParameter.doCompoundParentGeneration, "true");
				//else cp.AddParameter(Resources.CommandParameter.doCompoundParentGeneration, "false");

				//if (deposition.Status == DepositionStatus.ProcessingForGCN)
				//	cp.AddParameter(Resources.CommandParameter.doTautomerCanonicalization, "true");
				//else
				//	cp.AddParameter(Resources.CommandParameter.doTautomerCanonicalization, "false");

				//cp.AddParameter(Resources.CommandParameter.treatNoChirlaFlagWithUpOrDownBondsAsAbsoluteStereo, "true");
				//cp.AddParameter(Resources.CommandParameter.mappedFieldsFile, Path.Combine(depositionRootPath, "sdfFields." + Resources.StatusFileExtension.SdfMap.ToString()));
			}


			string localCommandFile = Path.Combine(depositionRootPath, Resources.DirectoryName.Processed.ToString(), chunkGuid + ".params");
			cp.Store(localCommandFile);

			Utils.LaunchProcess(deposition.Id, localCommandFile);
		}

		private static void FarmJob(RSC.CVSP.Deposition deposition, string depositionRootPath, string chunkGuid)
		{
			throw new NotImplementedException();

			string ftpDir;
			string processingDir_remote = ConfigurationManager.AppSettings["processingDir_remote"] + @"\" + depositionGuid + @"\" + chunk_id.ToString();
			string commandParams = PrepareCommandParameters.Get(processingDir_remote, chunkGuid, Resources.ProcessingStep.Process,
				false, deposition.ProcessingParameters.IsProcessedLocally, validationXml, acidbaseXml, standardizationXml, true, false, false);
			bool runInHighPriorityQueue = false;
			//if (TotalNumberOfRecords < 500)
			//	runInHighPriorityQueue = true;
			CondorProcessing.upload2Farm(depositionGuid, deposition.UserGuid, commandParams, chunk_id, chunkGuid, Path.Combine(depositionRoot, "sdfFields.txt"),
				validationXml, standardizationXml, acidbaseXml,
				out ftpDir, runInHighPriorityQueue);
		}
*/
	}
}
