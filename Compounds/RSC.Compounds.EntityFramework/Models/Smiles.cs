using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
	[Table("Smiles")]
	public class ef_Smiles
	{
		[DatabaseGenerated(DatabaseGeneratedOption.None), Key()]
		public Guid Id { get; set; }

		[Required]
		public string IndigoSmiles { get; set; }

		[Required]
		[MaxLength(32)]
		[Index("IndigoSmilesMd5_idx", IsUnique = true)]
		public string IndigoSmilesMd5 { get; set; }

		// as InChI (not smiles) is our unique identifier it is not guranteed that not more than a single compound will have same smiles
		public virtual ICollection<ef_Compound> Compounds { get; set; }
	}
}
