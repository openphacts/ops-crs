namespace CVSPEntityFramework
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.ComponentModel.DataAnnotations;
	using System.ComponentModel.DataAnnotations.Schema;
	using System.Data.Entity.Spatial;
using System.ComponentModel;

	[Table("workflow_properties")]
	public partial class EFWorkflowProperties
	{
		public EFWorkflowProperties()
		{

		}

		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[DefaultValue("false")]
		public bool SDFTagsCollected { get; set; }

		[DefaultValue("false")]
		public bool IsCRSDeposition { get; set; }

		public int ChunkSize { get; set; }


		[DefaultValue("false")]
		public bool IsProcessedLocally { get; set; }

		[DefaultValue("false")]
		public bool DoStandardize { get; set; }

		[ForeignKey("ValidationContent")]
		public int ValidationContentId { get; set; }

		[ForeignKey("StandardizationContent")]
		public int StandardizationContentId { get; set; }

		[ForeignKey("AcidBaseContent")]
		public int AcidBaseContentId { get; set; }

		[DefaultValue("false")]
		public bool AllowRelativeStereo { get; set; }

		public virtual EFUserContent ValidationContent { get; set; }


		public virtual EFUserContent StandardizationContent { get; set; }


		public virtual EFUserContent AcidBaseContent { get; set; }


	}
}
