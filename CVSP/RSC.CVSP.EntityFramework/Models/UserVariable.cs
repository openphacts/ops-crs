using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.CVSP.EntityFramework
{
	[Table("UserVariables")]
	public class ef_UserVariable
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		[Required]
		[StringLength(4000)]
		public string Value { get; set; }

		[Required]
		[StringLength(4000)]
		public string Description { get; set; }

		public virtual ef_UserProfile UserProfile { get; set; }
	}
}
