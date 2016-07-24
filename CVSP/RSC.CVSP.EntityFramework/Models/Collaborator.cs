using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.CVSP.EntityFramework
{
	[Table("Collaborators")]
	public class ef_Collaborator
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Required]
		public Guid UserGuid { get; set; }

		public virtual ef_RuleSet RuleSet { get; set; }
	}
}
