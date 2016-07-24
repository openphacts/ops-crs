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
		public static void DeleteOldSubmissions()
		{
			int NumOfDays = Convert.ToInt32(ConfigurationManager.AppSettings["DeleteSubmissionsOlderXDays"]);
			if (NumOfDays == 0)
				return;
			CVSPStore db = new EFCVSPStore();
			IEnumerable<Guid> guids = db.GetDepositionsByAge(NumOfDays);
				
			foreach (Guid guid in guids)
				db.UpdateDepositionStatus(guid, DepositionStatus.ToDelete);
		}


		private static void SubmitDeleteJob(Guid depositionGuid)
		{
			CommandParameters commandParameters = new CommandParameters();
			commandParameters.AddParameter(Resources.CommandParameter.processingType, Resources.ProcessingType.DeleteDeposition.ToString());
			commandParameters.AddParameter(Resources.CommandParameter.depositionId, depositionGuid.ToString());
			string depositionRootDirectory = Path.Combine(ConfigurationManager.AppSettings["data_path"], depositionGuid.ToString());
			commandParameters.AddParameter(Resources.CommandParameter.submissionDirectory, depositionRootDirectory);
			string commandFilePath = Path.Combine(depositionRootDirectory, "command.params");
			commandParameters.Store(commandFilePath);

			CVSPStore db = new EFCVSPStore();
			db.UpdateDepositionStatus(depositionGuid, DepositionStatus.Deleting);
			Utils.LaunchProcess(depositionGuid, commandFilePath);
			
		}

		public static void DeleteScheduledSubmissions()
		{
			EFCVSPStore db = new EFCVSPStore();
			IEnumerable<Guid> guids = db.GetDepositionsByStatus(DepositionStatus.ToDelete);
			foreach (Guid guid in guids)
				if (Utils.CanStartNewProcesses(guid, maxProcessLimit))
					SubmitDeleteJob(guid);
		}

	}
}
*/