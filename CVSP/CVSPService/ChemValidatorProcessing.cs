using System;
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Linq;
using RSC.CVSP.Utils;
using RSC.CVSP;
using RSC.CVSP.EntityFramework;

namespace CVSPService
{
	partial class ChemValidatorProcessing
	{
		/// <summary>
		/// Maximum of number of processes that service can launch
		/// </summary>
		private static int maxProcessLimit;
		static string remoteFtpServer;
		static string userName;
		static string password;

		static ChemValidatorProcessing()
		{

			remoteFtpServer = ConfigurationManager.AppSettings["CVSP_FTP"];
			userName = ConfigurationManager.AppSettings["CVSP_FTP_Login"];
			password = ConfigurationManager.AppSettings["CVSP_FTP_Password"];

			if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["CVSPWorkerMaxProcesses"]))
				maxProcessLimit = Int32.Parse(ConfigurationManager.AppSettings["CVSPWorkerMaxProcessesLimit"]);
			else maxProcessLimit = 6;
			Trace.TraceInformation("\n\n#### New Round ####\nMaximum process limit per deposition is set to " + maxProcessLimit);
		}

		public static void ProcessDownloadDirectory()
		{
			string ftp_tmp_dir = ConfigurationManager.AppSettings["FTP_Dir4downloads"];

			var dirs = from dir in Directory.EnumerateDirectories(ftp_tmp_dir) select dir;
			foreach (var dir in dirs)
			{
				DirectoryInfo di = new DirectoryInfo(dir);
				TimeSpan difftime = DateTime.Now - di.LastWriteTime;
				if (difftime.Days > 3)
					try
					{
						Directory.Delete(dir, true);
						Trace.TraceInformation("Dir " + di.FullName + " was more tan 3 days old. Deleting it..");
					}
					catch (Exception ex)
					{
						Trace.TraceInformation("Dir " + di.FullName + " was more tan 3 days old. Couldn't delete it: " + ex.Message + "\n" + ex.StackTrace);
					}
				else
				{
					string[] files = Directory.GetFiles(di.FullName);
					if (files.Where(x => x.Contains("status.start")).Any())
					{
						if (File.Exists(Path.Combine(di.FullName, "status.start")))
							File.Delete(Path.Combine(di.FullName, "status.start"));
						using (StreamWriter sr = new StreamWriter(Path.Combine(di.FullName, "status.processing")))
							sr.WriteLine("download is being prepared");
						Utils.LaunchProcess(Guid.Empty, Resources.CommandParameter.commandParametersFile + "=" + Path.Combine(di.FullName, "download_options.txt"));
					}
				}
			}
		}

		public static void DowloadFarmResults()
		{
			//	commented for now as we do not use this functionality and I wanted to disable GetDepositions() method as it implememnted in the wrong maner
/*
			Uri baseCondorSubmissions = new Uri(ConfigurationManager.AppSettings["CVSP_FTP"]);
			EFCVSPStore db = new EFCVSPStore();
			IEnumerable<RSC.CVSP.Deposition> depositions = db.GetDepositions().Where(d => d.ProcessingParameters.IsProcessedLocally == false);
			foreach (RSC.CVSP.Deposition deposition in depositions)
			{
				CondorProcessing.downloadFromFarm(Path.Combine(ConfigurationManager.AppSettings["data_path"], deposition.UserGuid.ToString(), deposition.Id.ToString()),
					ConfigurationManager.AppSettings["CVSP_FTP"] + "/" + deposition.UserGuid.ToString() + "_" + deposition.Id.ToString());
			}
*/
		}

		/// <summary>
		/// converts all incoming file types to sdf, merges them, splits to chunks
		/// </summary>
/*
		public static void ProcessRevisedRecords()
		{
			EFCVSPStore db = new EFCVSPStore();
			IEnumerable<RSC.CVSP.Record> records = db.GetRecords(db.GetRevisedRecordsToProcess());

			//quickly set all records to unprocessed and unrevised
			foreach (RSC.CVSP.Record record in records)
			{
				//int dep_id = dbl.GetDepositionIdByRecId(rec_id);
				RSC.CVSP.Deposition deposition = db.GetDeposition(record.DepositionId);
				Guid userGuid = deposition.UserGuid;
				string processingDir = Path.Combine(ConfigurationManager.AppSettings["data_path"], deposition.UserGuid.ToString(), record.DepositionId.ToString());
				if (deposition.Status == DepositionStatus.Processed || deposition.Status == DepositionStatus.ToProcessRevisions)
					db.UpdateDepositionStatus(deposition.Id, DepositionStatus.Processing);

				string commandParams = Resources.CommandParameter.processingType + "=" + Resources.ProcessingType.Revise + Environment.NewLine;
				commandParams += Resources.CommandParameter.rec_id + "=" + record.Id + Environment.NewLine;
				commandParams += Resources.CommandParameter.mappedFieldsFile + "=" + Path.Combine(processingDir, "mappedFields.cvsp") + Environment.NewLine;
				commandParams += Resources.CommandParameter.logFilePath + "=" + Path.Combine(processingDir, record.Id + ".log") + Environment.NewLine;
				commandParams += Resources.CommandParameter.acidBaseXMLFilePath + "=" + Path.Combine(processingDir, "acidbase.xml") + Environment.NewLine;
				commandParams += Resources.CommandParameter.validationXMLFilePath + "=" + Path.Combine(processingDir, "validation.xml") + Environment.NewLine;
				commandParams += Resources.CommandParameter.standardizationXMLFilePath + "=" + Path.Combine(processingDir, "standardization.xml") + Environment.NewLine;
				commandParams += Resources.CommandParameter.doValidate + "=True" + Environment.NewLine;
				commandParams += Resources.CommandParameter.doStandardize + "=true" + Environment.NewLine;
				commandParams += Resources.CommandParameter.treatNoChirlaFlagWithUpOrDownBondsAsAbsoluteStereo + "=True" + Environment.NewLine;
				commandParams += Resources.CommandParameter.depositionId + "=" + record.DepositionId + Environment.NewLine;

				string paramsFile = Path.Combine(processingDir, record.Id + ".params");
				using (StreamWriter sw = new StreamWriter(paramsFile))
					sw.Write(commandParams);
				db.ResetRecordForRevision(record.Id);
				Utils.LaunchProcess(record.DepositionId, Resources.CommandParameter.commandParametersFile.ToString() + "=" + paramsFile);
			}
		}
*/
	}
}
