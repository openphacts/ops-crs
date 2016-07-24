using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
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

		[Required]
		[ForeignKey("Revision")]//do not remove foreign key
		public Guid RevisionId { get; set; }
		[Required]
		public virtual ef_Revision Revision { get; set; }
	}
}
