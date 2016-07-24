using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Process.JobManager.EntityFramework
{
	[Table("JobParameters")]
	public partial class Parameter
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Required]
		[MaxLength(50)]
		public string Name { get; set; }

		[Required]
		public string Value { get; set; }

		[Required]
		[ForeignKey("Job")]
		public int JobId { get; set; }

		public virtual Job Job { get; set; }
	}
}
