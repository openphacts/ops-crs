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
/*
namespace CVSPService
{
	partial class ChemValidatorProcessing
	{
		public static void Prepare4Reprocessing()
		{
			CVSPStore db = new EFCVSPStore();
			IEnumerable<Guid> guids = db.GetDepositionsByStatus(DepositionStatus.ToReprocess);
			foreach (Guid guid in guids)
			{
				RSC.CVSP.Deposition deposition = db.GetDeposition(guid);
				CommandParameters commandParameters = new CommandParameters();
				commandParameters.AddParameter(Resources.CommandParameter.processingType, Resources.ProcessingType.Prepare4Reprocessing.ToString());
				commandParameters.AddParameter(Resources.CommandParameter.depositionId, guid.ToString());
				commandParameters.AddParameter(Resources.CommandParameter.submissionDirectory, Path.Combine(ConfigurationManager.AppSettings["data_path"], guid.ToString()));

				string commandFilePath = Path.Combine(ConfigurationManager.AppSettings["data_path"], guid.ToString(), "command.params");
				commandParameters.Store(commandFilePath);

				Utils.LaunchProcess(guid, commandFilePath);
				db.UpdateDepositionStatus(deposition.Id, DepositionStatus.Prepare4Reprocessing);
			}
		}

	}
}
*/