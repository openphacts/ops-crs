using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Process.ChunkManager.EntityFramework
{
	[Table("ChunkParameters")]
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
		[ForeignKey("Chunk")]
		public int ChunkId { get; set; }

		public virtual Chunk Chunk { get; set; }
	}
}
