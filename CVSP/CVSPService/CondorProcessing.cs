using System;
using System.Net;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using RSC.CVSP.Utils;
using RSC.CVSP.EntityFramework;

namespace CVSPService
{
	class CondorProcessing
	{
		static string remoteFtpServer;
		static string userName;
		static string password;

		static CondorProcessing()
		{
			remoteFtpServer = ConfigurationManager.AppSettings["CVSP_FTP"];
			userName = ConfigurationManager.AppSettings["CVSP_FTP_Login"];
			password = ConfigurationManager.AppSettings["CVSP_FTP_Password"];

		}

/*
		public static void killFarmProcessing(Guid depGuid)
		{
			//DepositionBusinessLayer dbl = new DepositionBusinessLayer();
			//int usr_id = dbl.GetUserIdByDepositionId(depGuid);
			EFCVSPStore db = new EFCVSPStore();
			RSC.CVSP.Deposition deposition = db.GetDeposition(depGuid);
			string kill_file = ConfigurationManager.AppSettings["CVSP_FTP"] + "/" + deposition.UserGuid + "_" + depGuid + "/kill.txt";
			//copy module list
			string killFile_temp = Path.GetTempFileName();
			using (StreamWriter sw = new StreamWriter(killFile_temp))
				sw.WriteLine("kill");

			FTP_Related.UploadFile2Ftp(killFile_temp, kill_file, userName, password);
		}
*/
		public static bool upload2Farm(Guid depositionGuid, Guid userGuid, string commandParamsFileContent, int chunk_id, string chunk_file,
			string mappedFieldsFile, string validationXml, string standardizationXml, string acidGroupsXml, out string deposition_chunk_folder,
			bool HigherPriorityQueue)
		{
			string deposition_folder = ConfigurationManager.AppSettings["CVSP_FTP"] + @"/" + userGuid + "_" + depositionGuid;
			deposition_chunk_folder = deposition_folder + @"/"+ chunk_id.ToString();
			string processingDir_remote = ConfigurationManager.AppSettings["processingDir_remote"] + @"\" + userGuid.ToString() + "_" + depositionGuid + @"\" + chunk_id.ToString();
			try
			{
				
				FTP_Related.createFtpDirectory(deposition_folder, userName, password);
				//create chunk folder
				FTP_Related.deleteFTPDirectory(deposition_chunk_folder, userName, password);//in case it is present from failed processing
				FTP_Related.createFtpDirectory(deposition_chunk_folder, userName, password);

				//copy module list
				string commandParamsFile = Path.GetTempFileName();
				using (StreamWriter sw = new StreamWriter(commandParamsFile))
					sw.Write(commandParamsFileContent);
				FTP_Related.UploadFile2Ftp(commandParamsFile, deposition_chunk_folder + "/command.params", userName, password);

				if (File.Exists(validationXml))
					FTP_Related.UploadFile2Ftp(validationXml, deposition_chunk_folder + "/validation.xml", userName, password);

				if (File.Exists(standardizationXml))
					FTP_Related.UploadFile2Ftp(standardizationXml, deposition_chunk_folder + "/standardization.xml", userName, password);

				if (File.Exists(acidGroupsXml))
					FTP_Related.UploadFile2Ftp(acidGroupsXml, deposition_chunk_folder + "/acidbase.xml", userName, password);

				//copy mappedFields.cvsp to FTP
				if (File.Exists(mappedFieldsFile))
				{
					//if (is_OP_Processing)
					//    HadoopProcessing.UploadFile2Ftp(sdTagsFile, in_folder + "/SDTagDictionary.txt");
					FTP_Related.UploadFile2Ftp(mappedFieldsFile, deposition_chunk_folder + "/mappedFields.cvsp", userName, password);
				}

				//creating condor job description file
				string tempFile_condor_job = Path.GetTempFileName();
				using (StreamWriter sw = new StreamWriter(tempFile_condor_job))
				{
					sw.WriteLine("Executable = " + processingDir_remote + @"\job.bat");
					sw.WriteLine("Universe = vanilla");
					sw.WriteLine("Initialdir = " + processingDir_remote);
					sw.WriteLine("output = condor.out" + Environment.NewLine + "error = condor.error" + Environment.NewLine + "log = condor.log");
					sw.WriteLine("should_transfer_files = YES");
					sw.WriteLine("when_to_transfer_output = ON_EXIT_OR_EVICT");
					//sw.WriteLine("transfer_output_files = 0.xml.gz");

					if (HigherPriorityQueue)
						sw.WriteLine("nice_user = True");
					sw.WriteLine("Queue");
				}
				FTP_Related.UploadFile2Ftp(tempFile_condor_job, deposition_chunk_folder + "/condor_job_windows.txt", userName, password);

				//creating batch file
				string tempFile_cvsp_batch = Path.GetTempFileName();
				using (StreamWriter sw = new StreamWriter(tempFile_cvsp_batch))
				{
					sw.WriteLine(@"net use \\grid00\" + ConfigurationManager.AppSettings["CondorUserNameOnGrid"] + " " + ConfigurationManager.AppSettings["CondorPasswordOnGrid"] + " /USER:" + ConfigurationManager.AppSettings["CondorUserNameOnGrid"] + "@rsc-us.org");
					//sw.WriteLine(ConfigurationManager.AppSettings["path_to_CVSPWorkerExe"] + " commandParametersFile=" + processingDir_remote + @"\command.params > " + processingDir_remote + @"\" + chunk_id + "_console.log 2>&1");
					sw.WriteLine(ConfigurationManager.AppSettings["path_to_CVSPWorkerExe"] + " commandParametersFile=" + processingDir_remote + @"\command.params");

				}
				FTP_Related.UploadFile2Ftp(tempFile_cvsp_batch, deposition_chunk_folder + "/job.bat", userName, password);

				//copy TXT to FTP
				if (File.Exists(chunk_file))
					FTP_Related.UploadFile2Ftp(chunk_file, deposition_chunk_folder + "/" + chunk_id + ".txt.gz", userName, password);

				//create start file
				FTP_Related.UploadEmptyFile2Ftp(deposition_chunk_folder + "/start.txt", userName, password);
				File.Delete(tempFile_condor_job);
				File.Delete(tempFile_cvsp_batch);
				File.Delete(commandParamsFile);
				return true;
			}
			catch (Exception ex)
			{
				Trace.TraceInformation(ex.Message + "\n" + ex.StackTrace);
				return false;
			}
		}


		public static bool downloadFromFarm(string LocalDepositionDirectory, string FarmDepositionDirectoryUri)
		{
			
			string processedDir = Path.Combine(LocalDepositionDirectory, "Processed");
			if (!Directory.Exists(processedDir))
			{
				DirectoryInfo di = Directory.CreateDirectory(processedDir);
				if (Directory.GetCreationTime(processedDir) != null)
					Trace.TraceInformation("Directory created: " + processedDir);
				else
				{
					Trace.TraceError("Directory could not be created: " + processedDir);
					return false;
				}
			}

			string chunks_dir = Path.Combine(LocalDepositionDirectory, "CHUNKS");
			if (!Directory.Exists(chunks_dir))
				Directory.CreateDirectory(chunks_dir);
			IEnumerable<string> chunks = Directory.EnumerateFiles(Path.Combine(LocalDepositionDirectory, "CHUNKS"), "*.processing");
			bool moreFiles2Download = false;
			if(chunks.Any())
				moreFiles2Download = true;
			foreach (string chunkFile in chunks)
			{
				string chunk_id = Path.GetFileNameWithoutExtension(chunkFile);
				string processedFarmDir = FarmDepositionDirectoryUri + "/" + chunk_id;
				if (FTP_Related.downloadFtpFile(processedFarmDir + "/" + chunk_id.ToString() + ".processed", 
					Path.Combine(LocalDepositionDirectory, "Processed", chunk_id + ".processed"), 
					userName, password))
				{
					if (FTP_Related.downloadFtpFile(processedFarmDir + "/" + chunk_id + ".xml.gz", Path.Combine(processedDir, chunk_id + ".xml.gz"), userName, password))
					{
						FTP_Related.deleteFTPDirectory(processedFarmDir, userName, password);
						System.Threading.Thread.Sleep(1000);
						File.Delete(Path.Combine(LocalDepositionDirectory, "CHUNKS", chunk_id + ".processing"));
						Trace.TraceInformation("Deleting chunk directory on farm: " + processedFarmDir);
					}
				}
			}
			return moreFiles2Download;
		}
	}
}
