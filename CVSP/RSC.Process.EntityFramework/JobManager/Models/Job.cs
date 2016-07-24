using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Process.JobManager.EntityFramework
{
	[Table("Jobs")]
	public partial class Job
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Required]
		[Index]
		public Guid ExternalId { get; set; }

		[Required]
		public DateTime Created { get; set; }

		public DateTime? Started { get; set; }

		public DateTime? Finished { get; set; }

		[Required]
		public JobStatus Status { get; set; }

		public string Error { get; set; }

		public virtual  ICollection<Parameter> Parameters { get; set; }

		public virtual ICollection<Watch> Watches { get; set; }
	}
}
