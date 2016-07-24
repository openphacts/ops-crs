using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Process.JobManager.EntityFramework
{
	[Table("JobWatches")]
	public class Watch
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Required]
		[MaxLength(50)]
		public string Name { get; set; }

		public DateTime Begin { get; set; }

		public DateTime End { get; set; }

		[Required]
		[ForeignKey("Job")]
		public int JobId { get; set; }

		public virtual Job Job { get; set; }
	}
}
