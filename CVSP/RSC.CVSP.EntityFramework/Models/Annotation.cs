using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.CVSP.EntityFramework
{
	[Table("Annotations")]
	public partial class ef_Annotation
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Index(IsUnique=true)]
		[MaxLength(50)]
		[Required]
		public string Name { get; set; }

		[Required]
		public string Title { get; set; }

		[Required]
		[DefaultValue(false)]
		public bool IsRequired { get; set; }
	}
}
