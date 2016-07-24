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
	public class NMRFeature
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[DataMember]
		public int Id { get; set; }

		[Required]
		[MaxLength(100)]
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string Description { get; set; }
	}
}
