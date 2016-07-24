using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Process.ChunkManager.EntityFramework
{
	[Table("Chunks")]
	public partial class Chunk
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Required]
		[Index]
		public Guid ExternalId { get; set; }

		[Required]
		public ChunkStatus Status { get; set; }

		public int NumberOfRecords { get; set; }

		public virtual ICollection<Parameter> Parameters { get; set; }

		public virtual Blob Blob { get; set; }
	}
}
