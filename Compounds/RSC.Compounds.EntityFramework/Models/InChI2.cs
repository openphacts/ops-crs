using ChemSpider.Molecules;
using ChemSpider.Utilities;
using InChINet;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
	public class InChI2
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Required]
		public string Inchi { get; set; }

		[Required]
		[MaxLength(27)]
		//[Index("InChIKey_idx", Order = 1, IsUnique=true)]
		public string InChIKey { get; set; }
	}

	public class TestTable
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Required]
		public int One { get; set; }

		[Required]
		public int Two { get; set; }
	}
}
