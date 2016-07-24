using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
	//describes depositor provided annotations: Link, Synonym, Comment, Smiles, Inchi, etc.
	[Table("Annotations")]
	public class ef_Annotation
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Required]
		public int Type { get; set; }

		[Required]
		public string Value { get; set; }

		[Required]
		public virtual ef_Revision Revision { get; set; }

	}
}
