using RSC.CVSP;
using RSC.CVSP.EntityFramework;
using RSC.CVSP.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVSPService
{
	partial class ChemValidatorProcessing
	{
/*
		public static void PrepareNewSubmissions()
		{
			CVSPStore db = new EFCVSPStore();
			IEnumerable<Guid> depositionsGuids = db.GetDepositionsByStatus(DepositionStatus.Submitted);
			foreach (Guid depositionGuid in depositionsGuids)
				ChemValidatorProcessing.PrepareNewCvspSubmission(depositionGuid);

			IEnumerable<Guid> gcnDepositionGuids = db.GetDepositionsByStatus(DepositionStatus.SubmittedForGCN);
			foreach (Guid depositionGuid in gcnDepositionGuids)
				ChemValidatorProcessing.PrepareNewGCNSubmission(depositionGuid);
		}

		public static void PrepareNewGCNSubmission(Guid depositionGuid)
		{
			//scan repository for new GCN submissions
			string depositionPath = Path.Combine(ConfigurationManager.AppSettings["data_path"], depositionGuid.ToString(),Resources.DirectoryName.Gcn.ToString());
			CVSPStore db = new EFCVSPStore();
			RSC.CVSP.Deposition deposition = db.GetDeposition(depositionGuid);
			if (LaunchPreparationJob(depositionGuid, depositionPath, deposition.DataDomain))
				db.UpdateDepositionStatus(depositionGuid, DepositionStatus.ProcessingForGCN);
		}

		public static void PrepareNewCvspSubmission(Guid depositionGuid)
		{
			//scan repository for new CVSP submissions
			string depositionPath = Path.Combine(ConfigurationManager.AppSettings["data_path"], depositionGuid.ToString());
			CVSPStore db = new EFCVSPStore();
			RSC.CVSP.Deposition deposition = db.GetDeposition(depositionGuid);
			if (LaunchPreparationJob(depositionGuid, depositionPath, deposition.DataDomain, true))
				db.UpdateDepositionStatus(depositionGuid, DepositionStatus.Processing);

		}

		private static bool LaunchPreparationJob(Guid depositionGuid, string depositionPath, DataDomain dd, bool prepareForGcn=false)
		{
			bool isProcessing = false;

			if (!Directory.Exists(depositionPath))
				return false;

			string inputDirectoryPath = Path.Combine(depositionPath, Resources.DirectoryName.Input.ToString());
			if (!Directory.Exists(inputDirectoryPath))
				return false;

			foreach (string filePath in Directory.GetFiles(inputDirectoryPath))
			{
				string noSpaceFileName = null;
				if (filePath.Contains(" "))
				{
					noSpaceFileName = filePath.Replace(" ", "");
					File.Move(filePath, noSpaceFileName);
				}
				else noSpaceFileName = filePath;

				if (!Utils.CanStartNewProcesses(depositionGuid, maxProcessLimit))
					continue;

				CommandParameters parameters = new CommandParameters();
				string comandFile;
				if (!prepareForGcn)
				{
					parameters.AddParameter(Resources.CommandParameter.processingType, Resources.ProcessingType.Prepare.ToString());
					parameters.AddParameter(Resources.CommandParameter.inputFilePath, noSpaceFileName);
					parameters.AddParameter(Resources.CommandParameter.outputDir, Path.Combine(depositionPath, Resources.DirectoryName.Chunks.ToString()));
					parameters.AddParameter(Resources.CommandParameter.logFilePath, Path.Combine(depositionPath, "preparation.log"));
					comandFile = Path.Combine(depositionPath, "command.params");
				}
				else
				{
					parameters.AddParameter(Resources.CommandParameter.processingType, Resources.ProcessingType.Prepare4GCNProcessing.ToString());
					parameters.AddParameter(Resources.CommandParameter.outputDir, Path.Combine(depositionPath, Resources.DirectoryName.Gcn.ToString(), Resources.DirectoryName.Chunks.ToString()));
					parameters.AddParameter(Resources.CommandParameter.logFilePath, Path.Combine(depositionPath, Resources.DirectoryName.Gcn.ToString(),"preparation.log"));
					comandFile = Path.Combine(depositionPath, Resources.DirectoryName.Gcn.ToString(),"command.params");

					Directory.CreateDirectory(Path.Combine(depositionPath, Resources.DirectoryName.Gcn.ToString()));
				}

				parameters.AddParameter(Resources.CommandParameter.depositionId, depositionGuid.ToString());
				parameters.AddParameter(Resources.CommandParameter.dataDomain, dd.ToString());
				
				if (dd == DataDomain.Substances)
					parameters.AddParameter(Resources.CommandParameter.mappedFieldsFile, Path.Combine(depositionPath, "sdfFields.txt"));
				
				parameters.Store(comandFile);

				Utils.LaunchProcess(depositionGuid, comandFile);
				isProcessing = true;

			}
			return isProcessing;
		}
*/
	}
}
