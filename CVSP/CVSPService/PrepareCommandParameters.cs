using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using RSC.CVSP;

namespace CVSPService
{
    public class PrepareCommandParameters
    {

		public static string Get(string processingDir, string chunk_file, Resources.ProcessingType processingType, 
			bool doNotUseWebServices, bool processLocally, 
			string validationXml, string acidbaseXml,string standardizationXml,
			bool treatNoChirlaFlagWithUpOrDownBondsAsAbsoluteStereo, bool doCompoundParentGeneration, 
			bool doTautomerCanonicalization,
			Resources.Vendor vendor = Resources.Vendor.InChI)
		{
			//string chunk_file_status = chunk_file.Replace(".prepared", ".txt.gz");
			int chunk_id = Convert.ToInt32(Path.GetFileName(chunk_file).Replace(".txt.gz", ""));

			string commandParams = Resources.CommandParameter.processingType + "=" + processingType.ToString() + Environment.NewLine;
			//if (dataDomain == DataDomain.Reactions)
			//	commandParams += Resources.ProcessingParameter.dataDomain + "=" + DataDomain.Reactions.ToString() + Environment.NewLine;
			//else if (dataDomain == DataDomain.Spectra)
			//	commandParams += Resources.ProcessingParameter.dataDomain + "=" + DataDomain.Spectra.ToString() + Environment.NewLine;
			//else if (dataDomain == DataDomain.Crystals)
			//	commandParams += Resources.ProcessingParameter.dataDomain + "=" + DataDomain.Crystals.ToString() + Environment.NewLine;
			//else if (dataDomain == DataDomain.Substances)
			//	commandParams += Resources.ProcessingParameter.dataDomain + "=" + DataDomain.Substances.ToString() + Environment.NewLine;

			commandParams += Resources.CommandParameter.doValidate + "=True" + Environment.NewLine;

			if (doCompoundParentGeneration)
				commandParams += Resources.CommandParameter.doCompoundParentGeneration + "=True" + Environment.NewLine;
			else
				commandParams += Resources.CommandParameter.doCompoundParentGeneration + "=False" + Environment.NewLine;

			//if (doStandardize)
			//	commandParams += CVSPEnums.ProcessingParameter.doStandardize + "=True" + Environment.NewLine;
			//else
			//	commandParams += CVSPEnums.ProcessingParameter.doStandardize + "=False" + Environment.NewLine;

			if (doTautomerCanonicalization)
				commandParams += Resources.CommandParameter.doTautomerCanonicalization + "=True" + Environment.NewLine;
			else
				commandParams += Resources.CommandParameter.doTautomerCanonicalization + "=False" + Environment.NewLine;

			commandParams += Resources.CommandParameter.vendor + "=" + vendor.ToString() + Environment.NewLine;

			if (treatNoChirlaFlagWithUpOrDownBondsAsAbsoluteStereo)
				commandParams += Resources.CommandParameter.treatNoChirlaFlagWithUpOrDownBondsAsAbsoluteStereo + "=True" + Environment.NewLine;
			else commandParams += Resources.CommandParameter.treatNoChirlaFlagWithUpOrDownBondsAsAbsoluteStereo + "=False" + Environment.NewLine;


			if (doNotUseWebServices)
				commandParams += Resources.CommandParameter.doNotUseWebServices + "=True" + Environment.NewLine;
			else commandParams += Resources.CommandParameter.doNotUseWebServices + "=False" + Environment.NewLine;


			commandParams += Resources.CommandParameter.mappedFieldsFile + "=" + Path.Combine(processingDir, "mappedFields.cvsp") + Environment.NewLine;

			if (processLocally)
			{
				commandParams += Resources.CommandParameter.parent_process_id + "=" + System.Diagnostics.Process.GetCurrentProcess().Id + Environment.NewLine;
				commandParams += Resources.CommandParameter.inputFilePath + "=" + chunk_file + Environment.NewLine;
				commandParams += Resources.CommandParameter.statusFile + "=" + Path.Combine(processingDir, "Processed", chunk_id + ".processed") + Environment.NewLine;
				//commandParams += CVSPResources.ProcessingParameter.errFilePath + "=" + Path.Combine(processingDir, "Processed", chunk_id + ".err") + Environment.NewLine;
				commandParams += Resources.CommandParameter.logFilePath + "=" + Path.Combine(processingDir, "Processed", chunk_id + ".log") + Environment.NewLine;
				commandParams += Resources.CommandParameter.outputFilePath + "=" + Path.Combine(processingDir, "Processed", chunk_id + ".xml.gz") + Environment.NewLine;
				commandParams += Resources.CommandParameter.acidBaseXMLFilePath + "=" + acidbaseXml + Environment.NewLine;
				commandParams += Resources.CommandParameter.validationXMLFilePath + "=" + validationXml + Environment.NewLine;
				commandParams += Resources.CommandParameter.standardizationXMLFilePath + "=" + standardizationXml + Environment.NewLine;
			}
			else
			{
				commandParams += Resources.CommandParameter.statusFile + "=" + Path.Combine(processingDir, chunk_id + ".processed") + Environment.NewLine;
				commandParams += Resources.CommandParameter.inputFilePath + "=" + Path.Combine(processingDir, chunk_id + ".txt.gz") + Environment.NewLine;
				//commandParams += CVSPResources.ProcessingParameter.errFilePath + "=" + Path.Combine(processingDir, chunk_id + ".err") + Environment.NewLine;
				commandParams += Resources.CommandParameter.logFilePath + "=" + Path.Combine(processingDir, chunk_id + ".log") + Environment.NewLine;
				commandParams += Resources.CommandParameter.outputFilePath + "=" + Path.Combine(processingDir, chunk_id + ".xml.gz") + Environment.NewLine;
				commandParams += Resources.CommandParameter.acidBaseXMLFilePath + "=" + Path.Combine(processingDir, Path.GetFileName(acidbaseXml)) + Environment.NewLine;
				commandParams += Resources.CommandParameter.validationXMLFilePath + "=" + Path.Combine(processingDir, Path.GetFileName(validationXml)) + Environment.NewLine;
				commandParams += Resources.CommandParameter.standardizationXMLFilePath + "=" + Path.Combine(processingDir, Path.GetFileName(standardizationXml)) + Environment.NewLine;

			}


			return commandParams;
		}


        

    }
}
