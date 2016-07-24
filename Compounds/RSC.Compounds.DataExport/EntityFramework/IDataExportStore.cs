using System;
using System.Collections.Generic;

namespace RSC.Compounds.DataExport
{
	public class OpsDataExportOptions
	{
		/// <summary>
		/// Directory to use as temporary instead of the system one. Useful for large exports when system disk space is limited.
		/// </summary>
		public string TmpDir { get; set; }

		public DateTime ExportDate { get; set; }

		/// <summary>
		/// Data Source'es to RDF-export
		/// </summary>
		public IEnumerable<Guid> DataSourceIds { get; set; }

		/// <summary>
		/// DataExportLogs' ids to resume export for
		/// </summary>
		public List<int> ExportIds { get; set; }

		/// <summary>
		/// DataExportLogFiles' ids to resume export for
		/// </summary>
		public List<int> FileIds { get; set; }

		/// <summary>
		/// Run export again even if it's recorded as successfully finished in state
		/// </summary>
		public bool Override { get; set; }

		/// <summary>
		/// SureChEMBL export does not include synonyms and properties
		/// </summary>
		public bool Limited { get; set; }

		/// <summary>
		/// Compress output ttl files
		/// </summary>
		public bool Compress { get; set; }

		/// <summary>
		/// Any of supported URL forms - FILE, FTP, etc where export files will be copied
		/// </summary>
		public string UploadUrl { get; set; }

		/// <summary>
		/// Download URL to be used in RDF export files - generally it's different from UploadUrl
		/// </summary>
		public string DownloadUrl { get; set; }

		/// <summary>
		/// Username to be used when UploadUrl requires authentification
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// Password to be used when UploadUrl requires authentification
		/// </summary>
		public string Password { get; set; }
	}

	public enum DataExportType
	{
		None,
		Master,
		DataSource,
		Map,
		Void
	}

	public interface IDataExportStore
	{
		T LoadState<T>(int id);
		
		void SaveState<T>(int id, T state);

		DataVersion GetCurrentDataVersion(Guid dataSourceId);

		DataExportLog GetPreviousDataExportLog(int dataExportId);

		void CreateExport(IDataExport export);

		void UpdateExport(IDataExport export);

		void UpdateExportFile(IDataExportFile export);

		DataExportLog GetDataExportLog(int dataExportId);
	}
}
