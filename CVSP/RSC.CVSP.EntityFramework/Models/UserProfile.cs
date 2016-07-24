using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.CVSP.EntityFramework
{
	[Table("UserProfiles")]
	public class ef_UserProfile
	{
		public ef_UserProfile()
		{
			VariableCollection = new List<ef_UserVariable>();
			RuleSetCollection = new List<ef_RuleSet>();
		}

		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Required]
		public Guid Guid { get; set; }

		[DefaultValue("false")]
		public bool SendEmail { get; set; }

		public string FtpDirectory { get; set; }

		public Guid? Datasource { get; set; }

		public virtual ICollection<ef_UserVariable> VariableCollection { get; set; }

		public virtual ICollection<ef_RuleSet> RuleSetCollection { get; set; }
		public virtual ICollection<ef_Deposition> DepositionCollection { get; set; }
	}
}
