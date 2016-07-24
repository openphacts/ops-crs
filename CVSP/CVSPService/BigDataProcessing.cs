using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Configuration;
using System.Security.AccessControl;
using System.IO;

using RSC.CVSP.Utils;
using RSC.CVSP;

namespace CVSPService
{
	public class BigDataProcessing
	{
		/// <summary>
		/// monitor has no interaction with any DB whatsoever, so this processing will not appear in CVSP interface
		/// Validation is in offline mode meaning it will not call any Web services, e.g. name validation, ChemSpider ID look up, etc
		/// 
		/// 1. monitor iterates directory big_data_offline_path (DIR1) from App settings
		/// 
		/// 2. then iterates subdirectories (DIR1/SUBDIR1) with bunch of *.sdf.gz files (ChemSpider export?) and creates 5K record *.txt.gz chunks in "CHUNKS" subdirectory (DIR1/SUBDIR1/CHUNKS). Once chunkization is over "chunkizedAll.txt" is created in DIR1/SUBDIR1
		/// 
		/// 3. then uploads chunks and required HTCondor job files to farm. Once uploaded all a "uploadedAll.txt" file is created in DIR1/SUBDIR1
		/// Condor farm processing is launched based on modules referenced in modules.cvsp file
		/// 
		/// 4. download XML results to DIR1/SUBDIR1/Processed directory. Once all downloaded and no unfinished jobs left a "downloadedAll.txt" file is created in DIR1/SUBDIR1
		///
		/// Required files in DIR1/SUBDIR1 to start processing:
		/// modules.cvsp (may contain validation modules only or a mix of validation and standardization modules as Enums)
		/// mappedFields.cvsp
		/// ValidationRules.xml
		/// StandardizationRules.xml
		/// acidgroups.xml
		/// </summary>

		static BigDataProcessing()
		{

			if (File.Exists(Path.Combine(Environment.CurrentDirectory, "processing.log")))
				File.Delete(Path.Combine(Environment.CurrentDirectory, "processing.log"));
			if (File.Exists(Path.Combine(Environment.CurrentDirectory, "processing.err")))
				File.Delete(Path.Combine(Environment.CurrentDirectory, "processing.err"));

		}

		public static void monitor()
        {
            string big_data_offline_path = null;
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["big_data_offline_path"]))
            {
                big_data_offline_path = ConfigurationManager.AppSettings["big_data_offline_path"];
                if (!Directory.Exists(big_data_offline_path))
                {
                    Trace.TraceError("Directory does not exists: " + big_data_offline_path);
                    return;
                }
            }

            foreach (string deposition_directory in Directory.EnumerateDirectories(big_data_offline_path))
            {


				Dictionary<Resources.CommandParameter, string> paramDict = new Dictionary<Resources.CommandParameter, string>();
				string processingOptionsFile = Path.Combine(deposition_directory, "command.params");
				if (!File.Exists(processingOptionsFile))
				{
					Console.WriteLine(processingOptionsFile + " is missing");
					Console.WriteLine("Can't start processing of " + deposition_directory);
					continue;
				}
				CommandParameters commandParameters = new CommandParameters(processingOptionsFile);
				//Utils.getCommandParams(processingOptionsFile, paramDict);


				if (!File.Exists(paramDict[Resources.CommandParameter.mappedFieldsFile]))
				{
					Console.WriteLine(paramDict[Resources.CommandParameter.mappedFieldsFile] + " is missing");
					Console.WriteLine("Can't start processing of " + deposition_directory);
					continue;
				}


				if (!File.Exists(paramDict[Resources.CommandParameter.standardizationXMLFilePath]))
				{
					Console.WriteLine(paramDict[Resources.CommandParameter.standardizationXMLFilePath] + " is missing");
					Console.WriteLine("Can't start processing of " + deposition_directory);
					continue;
				}


				if (!File.Exists(paramDict[Resources.CommandParameter.acidBaseXMLFilePath]))
				{
					Console.WriteLine(paramDict[Resources.CommandParameter.acidBaseXMLFilePath] + " is missing");
					Console.WriteLine("Can't start processing of " + deposition_directory);
					continue;
				}

				if (!File.Exists(paramDict[Resources.CommandParameter.validationXMLFilePath]))
				{
					Console.WriteLine(paramDict[Resources.CommandParameter.validationXMLFilePath] + " is missing");
					Console.WriteLine("Can't start processing of " + deposition_directory);
					continue;
				}

				Guid guid = Guid.Empty;
                foreach (string guid_t in Directory.EnumerateFiles(deposition_directory, "*.guid"))
                    guid = Guid.Parse(Path.GetFileNameWithoutExtension(guid_t));//just one guid file is supppose to be there
                if (guid  == Guid.Empty)
                {
                    guid = Guid.NewGuid();
                    using (File.Create(Path.Combine(deposition_directory, guid + ".guid"))) { }
                }

				DataDomain dd;
				Enum.TryParse(paramDict[Resources.CommandParameter.dataDomain], out dd);

				string baseCondor = ConfigurationManager.AppSettings["CVSP_FTP"];
				string remoteFtpFolder = baseCondor + @"/0_0_" + guid;


				string allChunkizedStatusFile = Path.Combine(deposition_directory, "allChunkized.cvsp");
				string chunksDir = Path.Combine(deposition_directory, "CHUNKS");
				if (!File.Exists(allChunkizedStatusFile))
				{
					//check that all neccessary files are present
					string status_file = Path.Combine(deposition_directory, "start.status");
					if (!File.Exists(status_file))
						continue;
					else File.Delete(status_file);
					
					if (!Directory.Exists(chunksDir))
                        Directory.CreateDirectory(chunksDir);
                    int lastChunkId = 0;
                    IEnumerable<string> files = Directory.EnumerateFiles(deposition_directory, "*.sdf.gz");
                    foreach (string file in files)
                    {
                        lastChunkId = FileOperations.splitFileIntoChunks(5000, file, chunksDir, 
							dd, lastChunkId);
                        lastChunkId++;
                    }
                    
					using (StreamWriter sw = new StreamWriter(allChunkizedStatusFile))
						sw.WriteLine("All files has beed uploaded to farm");
				}

				if(File.Exists(allChunkizedStatusFile))
				{
					string allUploadedToFarmStatusFile = Path.Combine(deposition_directory, "allUploaded2Farm.cvsp");
					if (!File.Exists(allUploadedToFarmStatusFile))
					{
						//bool doValidate;
						//Boolean.TryParse(paramDict[CVSPEnums.ProcessingParameter.doValidate], out doValidate);

						//bool doStandardize;
						//Boolean.TryParse(paramDict[CVSPEnums.ProcessingParameter.doStandardize], out doStandardize);

						bool doCompoundParentGeneration;
						Boolean.TryParse(paramDict[Resources.CommandParameter.doCompoundParentGeneration], out doCompoundParentGeneration);

						bool doTautomerCanonicalization;
						Boolean.TryParse(paramDict[Resources.CommandParameter.doTautomerCanonicalization], out doTautomerCanonicalization);

						Resources.Vendor vendor;
						Enum.TryParse(paramDict[Resources.CommandParameter.vendor], out vendor);

						bool doNotUseWebServices;
						Boolean.TryParse(paramDict[Resources.CommandParameter.doNotUseWebServices], out doNotUseWebServices);

						foreach (string chunk in Directory.EnumerateFiles(chunksDir,"*.prepared"))
						{
							int chunk_id = Convert.ToInt32(Path.GetFileNameWithoutExtension(chunk));

							string xmlInputChunkFile = chunk.Replace(".prepared", "") + ".txt.gz";

							string processingDir_remote = ConfigurationManager.AppSettings["processingDir_remote"] + @"\0_0_" + guid + @"\" + chunk_id.ToString();
							string commandParams = PrepareCommandParameters.Get(processingDir_remote,
								xmlInputChunkFile,
								Resources.ProcessingType.Process,
								doNotUseWebServices, 
								false,
								paramDict[Resources.CommandParameter.validationXMLFilePath],
								paramDict[Resources.CommandParameter.acidBaseXMLFilePath],
								paramDict[Resources.CommandParameter.standardizationXMLFilePath],
								true, 
								doCompoundParentGeneration,
								doTautomerCanonicalization,
								vendor);
							
							string depositionFarmChunkFolder;
							CondorProcessing.upload2Farm(guid, Guid.Empty, commandParams, chunk_id, xmlInputChunkFile,
								paramDict[Resources.CommandParameter.mappedFieldsFile],
								paramDict[Resources.CommandParameter.validationXMLFilePath],
								paramDict[Resources.CommandParameter.standardizationXMLFilePath],
								paramDict[Resources.CommandParameter.acidBaseXMLFilePath], 
								//guid,
								out depositionFarmChunkFolder,false);

							File.Move(chunk, chunk.Replace(".prepared", ".processing"));
						}

						using (StreamWriter sw = new StreamWriter(allUploadedToFarmStatusFile))
							sw.WriteLine("All files has beed uploaded to farm");
					}
					else
					{
						string allDownloadedStatusFile = Path.Combine(deposition_directory, "allDownloaded.cvsp");
						if (File.Exists(allUploadedToFarmStatusFile) && !File.Exists(allDownloadedStatusFile) &&
							!CondorProcessing.downloadFromFarm(deposition_directory, remoteFtpFolder))
							using (StreamWriter sw = new StreamWriter(allDownloadedStatusFile))
								sw.WriteLine("All files has beed downloaded from farm");
					}
					

					
				}
                
            }
        }

	}
}
