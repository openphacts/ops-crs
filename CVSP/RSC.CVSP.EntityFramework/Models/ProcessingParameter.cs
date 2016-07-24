using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.CVSP.EntityFramework
{
	[Table("ProcessingParameters")]
	public class ef_ProcessingParameter
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Index]
		[Required]
		[MaxLength(100)]
		public string Name { get; set; }

		[Required]
		public string Value { get; set; }

		[ForeignKey("Deposition")]
		[Required]
		public int DepositionId { get; set; }
		[Required]
		public virtual ef_Deposition Deposition { get; set; }
	}
}
