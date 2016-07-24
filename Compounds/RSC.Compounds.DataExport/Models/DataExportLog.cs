using System;
using System.Collections.Generic;

namespace RSC.Compounds.DataExport
{
	public class DataExportLog
	{
		public int Id { get; set; }

		public DataExportType ExportType { get; set; }

		public DateTime? ExportDate { get; set; }

		public DateTime? ExportStarted { get; set; }

		public DateTime? ExportFinished { get; set; }

		public Guid DataSourceId { get; set; }

		public int DataVersionId { get; set; }

		public string ExportDirectory { get; set; }

		public DataExportStatus Status { get; set; }

		public string ErrorMessage { get; set; }

		public IEnumerable<DataExportLogFile> Files { get; set; }
	}
}
