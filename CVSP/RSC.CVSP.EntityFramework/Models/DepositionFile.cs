using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.CVSP.EntityFramework
{
	[Table("DepositionFiles")]
	public class ef_DepositionFile
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Required]
		public Guid Guid { get; set; }

		[Required]
		[MaxLength(100)]
		public string Name { get; set; }

		[ForeignKey("Deposition")]
		[Required]
		public int DepositionId { get; set; }

		public virtual ef_Deposition Deposition { get; set; }

		public virtual ICollection<ef_Record> Records { get; set; }

		public virtual ICollection<ef_Field> Fields { get; set; }
	}
}
