using RSC.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.CVSP.EntityFramework
{
	[Table("Issues")]
	public class ef_Issue
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Index]
		[Required]
		[MaxLength(10)]
		public string Code { get; set; }

		[Required]
		public Guid LogId { get; set; }

		[ForeignKey("Record")]
		[Required]
		public int RecordId { get; set; }
		public virtual ef_Record Record { get; set; }
	}

	public static class IssueExtensions
	{
		public static Issue ToIssue(this ef_Issue ef)
		{
			return new Issue()
			{
				Id = ef.LogId,
				Code = ef.Code
			};
		}
	}
}
