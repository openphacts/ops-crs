using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.EntityFramework
{
	[Table("Properties")]
	public class ef_Property
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Required]
		public Guid PropertyId { get; set; }

		[Required]
		[ForeignKey("Revision")]
		public Guid RevisionId { get; set; }
		[Required]
		public virtual ef_Revision Revision { get; set; }
	}
}
