using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.CVSP.EntityFramework
{
	[Table("RuleSets")]
	public class ef_RuleSet
	{
		public ef_RuleSet()
		{
			Collaboraters = new List<ef_Collaborator>();
		}

		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Required]
		public Guid Guid { get; set; }

		[Required]
		public int RuleType { get; set; }

		[Required]
		public DateTime DateCreated { get; set; }

		public DateTime? DateRevised { get; set; }

		[Required]
		[StringLength(100)]
		public string Title { get; set; }

		[Required]
		public string Description { get; set; }

		[Required]
		public string Body { get; set; }

		[DefaultValue("false")]
		public bool IsDefault { get; set; }

		[DefaultValue("false")]
		public bool IsApproved { get; set; }

		[DefaultValue("false")]
		public bool IsPublic { get; set; }

		[DefaultValue(0)]
		public int CountOfCloned { get; set; }

		public virtual ICollection<ef_Collaborator> Collaboraters { get; set; }

		public virtual ef_UserProfile UserProfile { get; set; }

	}
}
