using System;

namespace RSC.Compounds.DataExport
{
	public class DataExportLogFile
	{
		public int Id { get; set; }

		/// <summary>
		/// The filename of the exported file.
		/// </summary>
		public string FileName { get; set; }

		public DateTime? ExportStarted { get; set; }

		public DateTime? ExportFinished { get; set; }

		/// <summary>
		/// The number of Records (usually Triples) in the file (required for VOID info).
		/// </summary>
		public int RecordCount { get; set; }

		public DataExportStatus Status { get; set; }

		public string ErrorMessage { get; set; }

		public DataExportLog Log { get; set; }
	}
}
