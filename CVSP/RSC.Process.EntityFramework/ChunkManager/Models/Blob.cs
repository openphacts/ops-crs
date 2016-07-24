using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Process.ChunkManager.EntityFramework
{
	[Table("ChunkBlobs")]
	public partial class Blob
	{
		[Key()]
		public int Id { get; set; }

		[Required]
		public byte[] Data { get; set; }

		[Required]
		public virtual Chunk Chunk { get; set; }

	}
}
