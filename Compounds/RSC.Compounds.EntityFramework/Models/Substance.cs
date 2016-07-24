using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
	[Table("Substances")]
	public class ef_Substance
	{
		[DatabaseGenerated(DatabaseGeneratedOption.None), Key()]
		public Guid Id { get; set; }

		[Required]
        [MaxLength(200)]
        [Index("ExternalIdentifier_DataSourceId_idx", IsUnique = true, Order = 1)]
		public string ExternalIdentifier { get; set; }

		[Required]
        [Index("ExternalIdentifier_DataSourceId_idx", IsUnique = true, Order = 0)]
		public Guid DataSourceId { get; set; }

		public virtual ICollection<ef_Revision> Revisions { get; set; }
	}
}
