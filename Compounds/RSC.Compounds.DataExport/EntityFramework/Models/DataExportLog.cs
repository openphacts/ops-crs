using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RSC.Compounds.DataExport.EntityFramework
{
	/// <summary>
	/// Stores details of a data export.
	/// </summary>
	[Table("DataExportLog")]
	public class ef_DataExportLog
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		public DataExportType ExportType { get; set; }

		public DateTime? ExportDate { get; set; }

		public DateTime? DateCreated { get; set; }

		public DateTime? DateUpdated { get; set; }

		[Required]
		public Guid DataSourceId { get; set; }

		public int DataVersionId { get; set; }

		public string ExportDirectory { get; set; }

		public DataExportStatus Status { get; set; }

		public string ErrorMessage { get; set; }

		public virtual ICollection<ef_DataExportLogFile> Files { get; set; }

		public string State { get; set; }
	}

	public static class DataExportLogExtensions
	{
		/// <summary>
		/// Converts an ef_DataExportLog to a core DataExportLog.
		/// </summary>
		/// <param name="ef">ef_DataExportLog for conversion.</param>
		/// <returns>The converted core DataExportLog.</returns>
		public static DataExportLog ToDataExportLog(this ef_DataExportLog ef)
		{
			var log = new DataExportLog {
				Id = ef.Id,
				ExportType = ef.ExportType,
				ExportDate = ef.ExportDate,
				ExportStarted = ef.DateCreated,
				ExportFinished = ef.DateUpdated,
				DataSourceId = ef.DataSourceId,
				DataVersionId = ef.DataVersionId,
				ExportDirectory = ef.ExportDirectory,
				ErrorMessage = ef.ErrorMessage,
				Status = ef.Status,
			};

			log.Files = ef.Files
				.ToList()
				.Select(
					f =>
						new DataExportLogFile() {
							Id = f.Id,
							FileName = f.FileName,
							ExportStarted = f.DateCreated,
							ExportFinished = f.DateUpdated,
							RecordCount = f.RecordCount,
							Status = f.Status,
							ErrorMessage = f.ErrorMessage,
							Log = log
						});

			return log;
		}
	}
}
