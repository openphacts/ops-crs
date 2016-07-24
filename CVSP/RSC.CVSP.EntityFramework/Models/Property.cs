using RSC.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.CVSP.EntityFramework
{
	[Table("Properties")]
	public class ef_Property
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
		public int Id { get; set; }

		//[Index]
		[Required]
		public Guid PropertyId { get; set; }

		[ForeignKey("Record")]
		[Required]
		public int RecordId { get; set; }
		public virtual ef_Record Record { get; set; }
	}
}
