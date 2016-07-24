using RSC.Compounds.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
	[Table("ParentChildren")]
	public class ef_ParentChild
	{
		[DatabaseGenerated(DatabaseGeneratedOption.None), Key()]
		public Guid Id { get; set; }

		[Required]
        [Index]
		public ParentChildRelationship Type { get; set; }

		[Required]
		[ForeignKey("Parent")]//do not remove foreign key
		public Guid ParentId { get; set; }

		public virtual ef_Compound Parent { get; set; }

		[Required]
		[ForeignKey("Child")]//do not remove foreign key
		public Guid ChildId { get; set; }

		[Required]
		public virtual ef_Compound Child { get; set; }
	}
}
