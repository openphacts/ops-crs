using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.CVSP.EntityFramework
{
	[Table("Depositions")]
	public class ef_Deposition
	{
		public ef_Deposition()
		{
			Records = new List<ef_Record>();
		}

		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Required]
		[Index(IsUnique=true)]
		public Guid Guid { get; set; }

		public Guid DatasourceId { get; set; }

		[Required]
		public DateTime DateSubmitted { get; set; }

		public DateTime? DateReprocessed { get; set; }

		[Required]
		public int Status { get; set; }

		[Required]
		public int DataDomain { get; set; }

		[DefaultValue("false")]
		public bool IsPublic { get; set; }

		[Required]
		public virtual ICollection<ef_ProcessingParameter> ProcessingParameters { get; set; }

		[ForeignKey("UserProfile")]
		[Required]
		public int UserProfileId { get; set; }
		[Required]
		public virtual ef_UserProfile UserProfile { get; set; }
		public virtual ICollection<ef_Record> Records { get; set; }
		public virtual ICollection<ef_DepositionFile> Files { get; set; }
	}
}
