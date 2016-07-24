using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{
	[DataContract]
	public enum DataDomain
	{
		[EnumMember]
		Substances,
		[EnumMember]
		Reactions,
		[EnumMember]
		Spectra,
		[EnumMember]
		Documents,
		[EnumMember]
		Crystals,
		[EnumMember]
		Unidentified
	}

	public static class Resources
	{
		[DataContract]
		public enum CommandParameter
		{
			processingType,
			command,
			commandParametersFile,
			totalRecordNumberFile,
			totalRecordNumber,
			adaptiveChunkSizeFile,
			doNotUseWebServices,
			doStandardize,
			doCompoundParentGeneration,
			doTautomerCanonicalization,
			doValidate,
			vendor,
			chunkSize,
			dir2delete,
			submissionDirectory,
			isFullReplaceOfData,//needed for cvsp to export records depending on full replace or update
			isGcnDeposition,
			inputFilePath,
			statusFile,
			validationXMLFilePath,
			standardizationXMLFilePath,
			acidBaseXMLFilePath,
			mappedFieldsFile,
			depositionId,
			rec_id,
			rec_ids,
			outputDir,
			outputFilePath,
			errFilePath,
			logFilePath,

			parent_process_id,
			treatNoChirlaFlagWithUpOrDownBondsAsAbsoluteStereo,
			dataDomain,
			//for download
			downloadType,
			downloadStd,
			issue_severities,
			issue_codes,
			standardizedFilter,
			downloadByFilters,
			downloadSelectedRecords
		}

		[DataContract]
		public enum ProcessingType
		{
			Prepare = 1,
			Revise,
			DeleteDeposition,
			Prepare4Reprocessing,
			XMLUpload2DB,
			XMLUpload2Gcn,
			Prepare4GCNProcessing,
			ProcessDownload,
			DoNothing,
			Process,
			Unknown
		}

		[DataContract]
		public enum StatusFileExtension
		{
			Prepared = 1,
			SdfMap,
			Processing,
			Processed,
			Uploading,
			Uploaded,
			Log,
			Xml
		}

		[DataContract]
		public enum DirectoryName
		{
			Input,
			Chunks,
			Processed,
			Uploaded,
			Gcn,
		}

		[DataContract]
		public enum Vendor { Indigo, OpenEye, ChemAxon, InChI }

		[DataContract]
		public enum LayoutOptions { DoNotThrow, Throw }

	}
}
