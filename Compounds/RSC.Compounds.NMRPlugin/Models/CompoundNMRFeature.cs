using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.NMRFeatures.Models
{
	[DataContract]
	public class CompoundNMRFeature
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		//[ForeignKey("Compound")]
		[DataMember]
		public int CompoundId { get; set; }

		[ForeignKey("NMRFeature")]
		[Required]
		[DataMember]
		public int NMRFeatureId { get; set; }

		[Required]
		[DataMember]
		public int Count { get; set; }

		public virtual NMRFeature NMRFeature { get; set; }
	}
}
