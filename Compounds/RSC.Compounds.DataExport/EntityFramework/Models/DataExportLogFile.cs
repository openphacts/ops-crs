using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.DataExport.EntityFramework
{
	[Table("DataExportLogFile")]
	public class ef_DataExportLogFile
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
		public int Id { get; set; }

		[ForeignKey("Log")]
		[Required]
		public int LogId { get; set; }

		public string FileName { get; set; }

		public DateTime? DateCreated { get; set; }

		public DateTime? DateUpdated { get; set; }

		/// <summary>
		/// The number of Records (usually Triples) in the file (required for VOID info).
		/// </summary>
		public int RecordCount { get; set; }

		public DataExportStatus Status { get; set; }

		public string ErrorMessage { get; set; }

		public virtual ef_DataExportLog Log { get; set; }
	}
}
