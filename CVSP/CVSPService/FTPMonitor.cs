using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Data;
using System.Text;
using System.Configuration;
using System.Configuration.Install;
using System.Diagnostics;
using RSC.CVSP.Utils;

using RSC.CVSP;
using RSC.CVSP.EntityFramework;
/*
namespace CVSPService
{
    public class FTPMonitor
    {
        //ftp file should be ".sdf", ".sdf.gz", or ".zip"
        public static void monitor()
        {
			//for now do not monitor FTP
			return;

            try
            {
                string ftp_dir = ConfigurationManager.AppSettings["ftp_data_path"];

				EFCVSPStore db = new EFCVSPStore();
				IEnumerable<RSC.CVSP.UserProfile> upList =  db.GetUserProfiles().Where(u=> !String.IsNullOrEmpty(u.FtpDirectory));

                //DataTable dt_ftpDirs = pbl.GetUserFTPDirectories2Monitor();
				foreach (RSC.CVSP.UserProfile up in upList)
                {
					string user_dir = Path.Combine(ftp_dir, up.FtpDirectory);
                    //int usr_id = Convert.ToInt32(dr["usr_id"].ToString());
                    //string user_name = db.getUserNameByUserId(usr_id);
					string[] files = Directory.GetFiles(user_dir);
                    if (countInputFiles(user_dir) == 1)
                    {
                        foreach (string file in files)
                        {
                            if ((file.ToLower().Contains(".sdf") || file.ToLower().Contains(".sdf.gz") || file.ToLower().Contains(".zip")) && !file.ToLower().Contains(".processed"))
                            {
								FileInfo fi = new FileInfo(file);
								using (StreamWriter sw = new StreamWriter(Path.Combine(fi.Directory.ToString(), "cvsp.log")))
								{
									Trace.TraceInformation("Found new FTP deposition from " + up.Id + " :" + file);

									if (!File.Exists(Path.Combine(fi.Directory.ToString(), "validation.xml")))
									{
										sw.WriteLine("validation.xml is not found.. skipping deposition");
										continue;
									}
									if (!File.Exists(Path.Combine(fi.Directory.ToString(), "acidbase.xml")))
									{
										sw.WriteLine("acidbase.xml is not found.. skipping deposition");
										continue;
									}
									if (!File.Exists(Path.Combine(fi.Directory.ToString(), "standardization.xml")))
									{
										sw.WriteLine("standardization.xml is not found.. skipping deposition");
										continue;
									}

									if (!File.Exists(Path.Combine(fi.Directory.ToString(), "mappedFields.cvsp")))
									{
										sw.WriteLine("mappedFields.cvsp is not found.. skipping deposition");
										continue;
									}

									if (!File.Exists(Path.Combine(fi.Directory.ToString(), "command.params")))
									{
										sw.WriteLine("command.params is not found.. skipping deposition");
										continue;
									}

									//Dictionary<Resources.CommandParameter, string> paramDict = new Dictionary<Resources.CommandParameter, string>();
									CommandParameters commandParameters = new CommandParameters(Path.Combine(fi.Directory.ToString(), "command.params"));
									
									bool doCompoundParentGeneration = false;
									if (commandParameters.Dictionary.ContainsKey(Resources.CommandParameter.doCompoundParentGeneration))
										Boolean.TryParse(commandParameters.Dictionary[Resources.CommandParameter.doCompoundParentGeneration], out doCompoundParentGeneration);
									Console.WriteLine("doCompoundParentGeneration=" + doCompoundParentGeneration);

									//bool doStandardize = false;
									//if (paramDict.ContainsKey(CVSPEnums.ProcessingParameter.doStandardize))
									//	Boolean.TryParse(paramDict[CVSPEnums.ProcessingParameter.doStandardize], out doStandardize);
									//Console.WriteLine("doStandardize=" + doStandardize);

									//bool doValidate = false;
									//if (paramDict.ContainsKey(CVSPEnums.ProcessingParameter.doValidate))
									//	Boolean.TryParse(paramDict[CVSPEnums.ProcessingParameter.doValidate], out doValidate);
									//Console.WriteLine("doValidate=" + doValidate);

									
									bool treatNoChirlaFlagWithUpOrDownBondsAsAbsoluteStereo = false;
									if (commandParameters.Dictionary.ContainsKey(Resources.CommandParameter.treatNoChirlaFlagWithUpOrDownBondsAsAbsoluteStereo))
										Boolean.TryParse(commandParameters.Dictionary[Resources.CommandParameter.treatNoChirlaFlagWithUpOrDownBondsAsAbsoluteStereo], out treatNoChirlaFlagWithUpOrDownBondsAsAbsoluteStereo);
									Console.WriteLine("treatNoChirlaFlagWithUpOrDownBondsAsAbsoluteStereo=" + treatNoChirlaFlagWithUpOrDownBondsAsAbsoluteStereo);

									//string lowFileName = fi.Name.ToLower();
									//DataDomain dataType = DataDomain.Unidentified;
									//if (lowFileName.Contains(".mol") || lowFileName.Contains(".sdf") || lowFileName.Contains(".cdx"))
									//	dataType = DataDomain.Substances;
									//else if (lowFileName.Contains(".rxn") || lowFileName.Contains(".rdf"))
									//	dataType = DataDomain.Reactions;
									//else if (lowFileName.Contains(".jdx") || lowFileName.Contains(".dx"))
									//	dataType = DataDomain.Spectra;
									//else if (lowFileName.Contains(".cif"))
									//	dataType = DataDomain.Crystals;
									//else continue;

									//DepositionBusinessLayer dbl = new DepositionBusinessLayer();
									//int dep_id = dbl.CreateDeposition(up.Id, dataType, fi.Name, DepositionStatus.locked, false, doStandardize);
									//RSC.CVSP.ProcessingParameters pp = new RSC.CVSP.ProcessingParameters(false, Guid.Empty, Guid.Empty, Guid.Empty);
									//pp.CompoundParentGeneration = doCompoundParentGeneration;
									
									RSC.CVSP.Deposition deposition = new RSC.CVSP.Deposition(up.Id, DataDomain.Substances);
									Guid depGuid = db.CreateDeposition(deposition);
									//int dep_id = db.CreateDeposition(up.Id, dataType, fi.Name, DepositionStatus.locked, false, doStandardize);
									//if (doCompoundParentGeneration && doStandardize)
									//	dbl.SetDepositionAsCRSDeposition(dep_id, true);

									sw.WriteLine("deposition id assigned: " + depGuid);
									string savePath = Path.Combine(ConfigurationManager.AppSettings["data_path"], up.Id.ToString(), depGuid.ToString(), "input", fi.Name);
									if (!Directory.Exists(Path.Combine(ConfigurationManager.AppSettings["data_path"], up.Id.ToString())))
										Directory.CreateDirectory(Path.Combine(ConfigurationManager.AppSettings["data_path"], up.Id.ToString()));
									if (!Directory.Exists(Path.Combine(ConfigurationManager.AppSettings["data_path"], up.Id.ToString(), depGuid.ToString())))
										Directory.CreateDirectory(Path.Combine(ConfigurationManager.AppSettings["data_path"], up.Id.ToString(), depGuid.ToString()));
									if (!Directory.Exists(Path.Combine(ConfigurationManager.AppSettings["data_path"], up.Id.ToString(), depGuid.ToString(), "input")))
										Directory.CreateDirectory(Path.Combine(ConfigurationManager.AppSettings["data_path"], up.Id.ToString(), depGuid.ToString(), "input"));
									File.Copy(file, savePath);

									string deposition_dir = Path.Combine(ConfigurationManager.AppSettings["data_path"], up.Id.ToString(), depGuid.ToString());
									File.Copy(Path.Combine(fi.Directory.ToString(), "validation.xml"), Path.Combine(deposition_dir, "validation.xml"));
									File.Copy(Path.Combine(fi.Directory.ToString(), "acidbase.xml"), Path.Combine(deposition_dir, "acidbase.xml"));
									File.Copy(Path.Combine(fi.Directory.ToString(), "standardization.xml"), Path.Combine(deposition_dir, "standardization.xml"));
									File.Copy(Path.Combine(fi.Directory.ToString(), "mappedFields.cvsp"), Path.Combine(deposition_dir, "mappedFields.cvsp"));


									File.Move(file, Path.Combine(Path.GetDirectoryName(file), Path.GetFileName(file).ToLower() + ".processed"));
									db.UpdateDepositionStatus(depGuid,DepositionStatus.Submitted);
									//dbl.SetDepositionProcessingStatus(depGuid, DepositionStatus.submitted);
								}
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceInformation(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private static int countInputFiles(string dir)
        {
            int count = 0;
            foreach (string file in Directory.GetFiles(dir))
            {
                if ((file.ToLower().Contains(".sdf") || file.ToLower().Contains(".sdf.gz") || file.ToLower().Contains(".zip")) && !file.ToLower().Contains("processed"))
                {
                    count++;
                }
            }
            return count;
        }
    }
}
*/